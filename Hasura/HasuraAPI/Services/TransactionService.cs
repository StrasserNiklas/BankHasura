using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using HasuraAPI.Models;

namespace HasuraAPI.Services;

public class TransactionService : ITransactionService
{
    private const string TransactionDoneStatus = "Done";
    private readonly GraphQLHttpClient graphqlClient;
    private readonly string paymentsSubscriptionRequestQuery;
    private readonly string createTransactionRequestQuery;
    private readonly string updatePaymentRequestQuery;

    public TransactionService(string url)
    {
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

        this.paymentsSubscriptionRequestQuery = File.ReadAllText(@"Querys/PaymentSubscription.graphql");
        this.updatePaymentRequestQuery = File.ReadAllText(@"Querys/UpdatePayment.graphql");
        this.createTransactionRequestQuery = File.ReadAllText(@"Querys/CreateTransaction.graphql");
    }

    public void Subscribe()
    {
        var paymentSubscription = this.graphqlClient.CreateSubscriptionStream<PaymentResult>(this.BuildPaymentSubscriptionRequest());
        paymentSubscription.Subscribe(async x => await this.HandlePayments(x.Data.Payments));
    }

    private async Task HandlePayments(List<Payment> payments)
    {
        var payment = payments.FirstOrDefault();

        if (payment is null || payment.Status == TransactionDoneStatus)
        {
            return;
        }

        await this.graphqlClient.SendMutationAsync<Payment>(BuildCreateTransactionRequest(payment.Sender_Id, payment.Recipient_Id, payment.Amount, payment.Description));
        await this.graphqlClient.SendMutationAsync<Payment>(this.BuildUpdatePaymentRequest(payment.Id, TransactionDoneStatus));
    }

    private GraphQLRequest BuildPaymentSubscriptionRequest()
    {
        return new GraphQLRequest { Query = this.paymentsSubscriptionRequestQuery };
    }

    private GraphQLRequest BuildUpdatePaymentRequest(int id, string status)
    {
        var variables = new { Id = id, Status = status };
        return new GraphQLRequest { Query = this.updatePaymentRequestQuery, Variables = variables };
    }

    private GraphQLRequest BuildCreateTransactionRequest(int senderId, int recipientId, double amount, string description)
    {
        var variables = new { Sender = senderId, Recipient = recipientId, Amount = amount, Description = description };
        return new GraphQLRequest { Query = this.createTransactionRequestQuery, Variables = variables };
    }
}
