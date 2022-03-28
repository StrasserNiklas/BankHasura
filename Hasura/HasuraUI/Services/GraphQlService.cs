using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using HasuraUI.Models;
using System;
using System.Threading.Tasks;

namespace HasuraUI.Services
{
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

            this.graphqlClient.WebSocketReceiveErrors.Subscribe(x => this.Handle(x));
            this.graphqlClient.WebsocketConnectionState.Subscribe(x => this.Handle(x));

            this.graphqlClient.InitializeWebsocketConnection();
        }

        public void Handle(Exception exction)
        {
            Console.WriteLine(exction.Message);
        }
        public void Handle(GraphQLWebsocketConnectionState exction)
        {
            Console.WriteLine(exction);
        }

        public async Task<PaymentsResult> GetPaymentsAsync(int id)
        {
            var result = await this.graphqlClient.SendQueryAsync<PaymentsResult>(this.Builder.GetPaymentsRequest(id));
            return result.Data;
        }

        //public IObservable<GraphQLResponse<PaymentsResult>> GetPaymentsSubscription(int id)
        //{
        //    return this.graphqlClient.CreateSubscriptionStream<PaymentsResult>(this.Builder.GetPaymentsSubscriptionRequest(id));
        //}

        public void GetPaymentsSubscription(int id, Action<PaymentsResult> action)
        {
            var sub = this.graphqlClient.CreateSubscriptionStream<PaymentsResult>(this.Builder.GetPaymentsSubscriptionRequest(id));
            sub.Subscribe(x => action(x.Data));
            Console.WriteLine("Subscribed to payments");
        }

        public async Task<TransactionsResult> GetTransactionsAsync(int id)
        {
            var result = await this.graphqlClient.SendQueryAsync<TransactionsResult>(this.Builder.GetTransactionsRequest(id));
            return result.Data;
        }

        //public IObservable<GraphQLResponse<TransactionsResult>> GetTransactionsSubscription(int id)
        //{
        //    return this.graphqlClient.CreateSubscriptionStream<TransactionsResult>(this.Builder.GetTransactionsSubscriptionRequest(id));
        //}

        public void GetTransactionsSubscription(int id, Action<TransactionsResult> action)
        {
            var sub = this.graphqlClient.CreateSubscriptionStream<TransactionsResult>(this.Builder.GetTransactionsSubscriptionRequest(id));
            sub.Subscribe(x => action(x.Data));
            Console.WriteLine("Subscribed to transactions");
        }

        public async Task<object> CreatePaymentAsync(int senderId, int recepientId, double amount, string description)
        {
            var result = await this.graphqlClient.SendMutationAsync<object>(this.Builder.CreatePaymentRequest(senderId, recepientId, amount, description));
            return result.Data;
        }

        public async Task<int> LoginAsync(string idInput)
        {
            if (int.TryParse(idInput, out int id))
            {
                var exists = await this.CheckIfUserExistsAsync(id);

                if (exists)
                {
                    return id;
                }
            }

            var createdId = await this.CreateUserAsync($"User{idInput}");
            return createdId;
        }

        private async Task<int> CreateUserAsync(string name)
        {
            var result = await this.graphqlClient.SendQueryAsync<UserResult>(this.Builder.CreateUserRequest(name));
            return result.Data.Insert_user_one.Id;
        }

        private async Task<bool> CheckIfUserExistsAsync(int id)
        {
            var result = await this.graphqlClient.SendQueryAsync<UserExistsResult>(this.Builder.CheckIfUserExistsRequest(id));
            return result.Data?.User_by_pk != null ? true : false;
        }
    }
}
