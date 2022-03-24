using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using HasuraSharedLib.Models;
using HasuraUI.Models;

namespace HasuraUI.Services
{
    public class BankingService
    {
        private readonly GraphQlService graphQlService;
        public List<PaymentTransaction> Transactions { get;  }
        public List<PaymentTransaction> Payments { get; }


        public BankingService(GraphQlService graphQlService)
        {
            this.graphQlService = graphQlService;
            this.Transactions = new List<PaymentTransaction>();
            this.Payments = new List<PaymentTransaction>();
        }

        public async Task<int> Login(string userInput)
        {
            return await this.graphQlService.Login(userInput);
        }

        public async Task CreatePayment(int senderId, int recepientId, double amount, string description)
        {
            // check if recipient exists first

            var result = await this.graphQlService.CreatePayment(senderId, recepientId, amount, description);
            var payment = result.Payments.FirstOrDefault();

            if (payment != null)
            {
                this.Payments.Add(payment);
            }
        }

        //private async Task HandlePayments(List<Payment> payments)
        //{
        //    var payment = payments.FirstOrDefault();

        //    if (payment is null || payment.Status == TransactionDoneStatus)
        //    {
        //        return;
        //    }

        //    await this.graphqlClient.SendMutationAsync<Payment>(BuildCreateTransactionRequest(payment.Sender_Id, payment.Recipient_Id, payment.Amount, payment.Description));
        //    await this.graphqlClient.SendMutationAsync<Payment>(this.BuildUpdatePaymentRequest(payment.Id, TransactionDoneStatus));
        //}
    }

    public class GraphQlService
    {
        private readonly GraphQLHttpClient graphqlClient;
        public GraphQlRequestBuilder Builder { get; set; }

        public GraphQlService(string url)
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
            this.Subscribe();

        }

        private void Subscribe()
        {
            this.graphqlClient.WebSocketReceiveErrors.Subscribe(x => this.Handle(x));

            //var paymentSubscription = this.graphqlClient.CreateSubscriptionStream<PaymentResult>(this.BuildPaymentSubscriptionRequest());
            //paymentSubscription.Subscribe(async x => await this.HandlePayments(x.Data.Payments));
        }

        //public IObservable MyProperty { get; set; }

        public void Handle(Exception exction)
        {

        }

        
        public async Task<PaymentsResult> CreatePayment(int senderId, int recepientId, double amount, string description)
        {
            var result = await this.graphqlClient.SendMutationAsync<PaymentsResult>(this.Builder.CreatePaymentRequest(senderId, recepientId, amount, description));
            return result.Data;
        }

        public async Task<int> Login(string idInput)
        {
            if (int.TryParse(idInput, out int id))
            {
                var exists = await this.CheckIfUserExists(id);

                if (exists)
                {
                    return id;
                }
            }

            var createdId = await this.CreateUser($"User{idInput}");
            return createdId;
        }

        private async Task<int> CreateUser(string name)
        {
            var result = await this.graphqlClient.SendQueryAsync<UserResult>(this.Builder.CreateUserRequest(name));
            return result.Data.Insert_user_one.Id;
        }

        private async Task<bool> CheckIfUserExists(int id)
        {
            var result = await this.graphqlClient.SendQueryAsync<UserExistsResult>(this.Builder.CheckIfUserExistsRequest(id));
            return result.Data.User_by_pk != null ? true : false;
        }

        
    }
}
