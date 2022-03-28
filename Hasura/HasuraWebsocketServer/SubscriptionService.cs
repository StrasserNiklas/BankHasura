using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using SharedLibrary.Models;
using System.Threading.Tasks.Dataflow;

public class SubscriptionService
{
    private readonly GraphQLHttpClient graphqlClient;
    private readonly string paymentsSubscriptionRequestQuery;
    private readonly string transactionsSubscriptionRequestQuery;
    private readonly BufferBlock<SubscriptionResponse> resultBufferBlock;
    private int userId;

    public SubscriptionService(string url)
    {
        var dataFlowOptions = new ExecutionDataflowBlockOptions
        {
            EnsureOrdered = true,
            MaxDegreeOfParallelism = 1,
        };

        this.resultBufferBlock = new BufferBlock<SubscriptionResponse>(dataFlowOptions);

        this.graphqlClient = new GraphQLHttpClient(config =>
        {
            config.EndPoint = new Uri($"https{url}");
            config.WebSocketEndPoint = new Uri($"wss{url}");
            config.ConfigureWebsocketOptions = (x =>
            {
                x.SetRequestHeader("x-hasura-admin-secret", "XTWlYYXfTuM7SVx1zzUE1PpnTxnhSRgsTCBe5gFiWPm6gc6wegO6dqh2GwzVgxkU");
            });

        }, new SystemTextJsonSerializer());

        graphqlClient.HttpClient.DefaultRequestHeaders.Add("x-hasura-admin-secret", "XTWlYYXfTuM7SVx1zzUE1PpnTxnhSRgsTCBe5gFiWPm6gc6wegO6dqh2GwzVgxkU");

        this.paymentsSubscriptionRequestQuery = File.ReadAllText(@"Querys/GetPaymentsSubscription.graphql");
        this.transactionsSubscriptionRequestQuery = File.ReadAllText(@"Querys/GetTransactionsSubscription.graphql");
    }

    public ISourceBlock<SubscriptionResponse> ResultBufferBlock => this.resultBufferBlock;

    public void RelayData(SubscriptionRequest subscriptionRequest)
    {
        this.userId = subscriptionRequest.Id;
        this.Subscribe();
    }

    private void Subscribe()
    {
        var paymentSubscription = this.graphqlClient.CreateSubscriptionStream<PaymentsResult>(this.BuildPaymentSubscriptionRequest(this.userId));
        paymentSubscription.Subscribe(x => this.HandlePayments(x.Data.Payments));

        var transactionSubscription = this.graphqlClient.CreateSubscriptionStream<TransactionsResult>(this.BuildCreateTransactionRequest(this.userId));
        transactionSubscription.Subscribe(x => this.HandleTransactions(x.Data.Transactions));
    }

    private void HandlePayments(List<PaymentTransaction> payments)
    {
        var payment = payments.FirstOrDefault();

        if (payment is null)
        {
            return;
        }

        this.resultBufferBlock.Post(new SubscriptionResponse {  Result = payment });
    }

    private void HandleTransactions(List<PaymentTransaction> transactions)
    {
        var transaction = transactions.FirstOrDefault();

        if (transaction is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(transaction.Status))
        {
            return;
        }

        this.resultBufferBlock.Post(new SubscriptionResponse { Result = transaction });
    }

    private GraphQLRequest BuildPaymentSubscriptionRequest(int id)
    {
        var variables = new { Id = id };
        return new GraphQLRequest { Query = this.paymentsSubscriptionRequestQuery, Variables = variables };
    }

    private GraphQLRequest BuildCreateTransactionRequest(int id)
    {
        var variables = new { Id = id };
        return new GraphQLRequest { Query = this.transactionsSubscriptionRequestQuery, Variables = variables };
    }
}

