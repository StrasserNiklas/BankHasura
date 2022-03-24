using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace HasuraAPI;

public class TransactionService : ITransactionService
{
    private const string TransactionDoneStatus = "Done";
    private readonly GraphQLHttpClient graphqlClient;
    private readonly string paymentsSubscriptionRequestQuery;
    //private readonly string checkIfUserExistRequestQuery;
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
        //this.checkIfUserExistRequestQuery = File.ReadAllText(@"Querys/CheckIfUserExists.graphql");
        this.createTransactionRequestQuery = File.ReadAllText(@"Querys/CreateTransaction.graphql");
    }

    public void Subscribe()
    {
        this.graphqlClient.WebSocketReceiveErrors.Subscribe(x => this.Handle(x));  

        var paymentSubscription = this.graphqlClient.CreateSubscriptionStream<PaymentResult>(this.BuildPaymentSubscriptionRequest());
        paymentSubscription.Subscribe(async x => await this.HandlePayments(x.Data.Payments));
    }

    public void Handle(Exception exction)
    {

    }

    private async Task HandlePayments(List<Payment> payments)
    {
        var payment = payments.FirstOrDefault();

        if (payment is null || payment.Status == TransactionDoneStatus)
        {
            await Task.FromResult(0); 
        }

        //var exists = await this.CheckIfRecipientExists(payment.Recipient_Id);

        //if (!exists)
        //{
        //    await this.graphqlClient.SendMutationAsync<Payment>(this.BuildUpdatePaymentRequest(payment.Id, "Recipient does not exist!"));
        //}

        await this.graphqlClient.SendMutationAsync<Payment>(BuildCreateTransactionRequest(payment.Sender_Id, payment.Recipient_Id, payment.Amount, payment.Description));
        await this.graphqlClient.SendMutationAsync<Payment>(this.BuildUpdatePaymentRequest(payment.Id, TransactionDoneStatus));
    }

    //private async Task<bool> CheckIfRecipientExists(int id)
    //{
    //    var result = await this.graphqlClient.SendQueryAsync<User>(this.BuildCheckIfUserExistsRequest(id));
    //    return result.Data is not null ? true : false;
    //}

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

    //private GraphQLRequest BuildCheckIfUserExistsRequest(int id)
    //{
    //    var variables = new { Id = id};
    //    return new GraphQLRequest { Query = this.checkIfUserExistRequestQuery, Variables = variables };
    //}
}

//public class User
//{
//    public int Id { get; set; }

//    public string Name { get; set; }
//}

public class Payment
{
    public int Id { get; set; }
    public int Sender_Id { get; set; }
    public int Recipient_Id { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
}

public class PaymentResult
{
    public List<Payment> Payments { get; set; }
}