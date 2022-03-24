using GraphQL;

namespace HasuraUI.Services
{
    public class GraphQlRequestBuilder
    {
        private string createUserRequestQuery;
        private string checkIfUserExistRequestQuery;
        private string createPaymentRequestQuery;
        private string getPaymentsRequestQuery;
        private string getPaymentsRequestQuerySubscription;
        private string getTransactionRequestQuery;
        private string getTransactionRequestQuerySubscription;
        private readonly HttpClient http;

        public GraphQlRequestBuilder(HttpClient httpClient)
        {
            this.http = httpClient;
        }

        public async Task LoadFiles()
        {
            this.createUserRequestQuery = await this.http.GetStringAsync(@"Querys/CreateUser.graphql");
            this.checkIfUserExistRequestQuery = await this.http.GetStringAsync(@"Querys/CheckUserExists.graphql");
            this.createPaymentRequestQuery = await this.http.GetStringAsync(@"Querys/CreatePayment.graphql");
            this.getPaymentsRequestQuery = await this.http.GetStringAsync(@"Querys/GetPaymentsQuery.graphql");
            this.getPaymentsRequestQuerySubscription = await this.http.GetStringAsync(@"Querys/GetPaymentsSubscription.graphql");
            this.getTransactionRequestQuery = await this.http.GetStringAsync(@"Querys/GetTransactionsQuery.graphql");
            this.getTransactionRequestQuerySubscription = await this.http.GetStringAsync(@"Querys/GetTransactionsSubscription.graphql");
        }

        public GraphQLRequest GetTransactionsSubscriptionRequest(int id)
        {
            var variables = new { Id = id };
            return new GraphQLRequest { Query = this.getTransactionRequestQuerySubscription, Variables = variables };
        }

        public GraphQLRequest GetTransactionsRequest(int id)
        {
            var variables = new { Id = id };
            return new GraphQLRequest { Query = this.getTransactionRequestQuery, Variables = variables };
        }

        public GraphQLRequest GetPaymentsSubscriptionRequest(int id)
        {
            var variables = new { Id = id };
            return new GraphQLRequest { Query = this.getPaymentsRequestQuerySubscription, Variables = variables };
        }

        public GraphQLRequest GetPaymentsRequest(int id)
        {
            var variables = new { Id = id };
            return new GraphQLRequest { Query = this.getPaymentsRequestQuery, Variables = variables };
        }

        public GraphQLRequest CreatePaymentRequest(int senderId, int recipientId, double amount, string description)
        {
            var variables = new { Sender = senderId, Recipient = recipientId, Amount = amount, Description = description };
            return new GraphQLRequest { Query = this.createPaymentRequestQuery, Variables = variables };
        }

        public GraphQLRequest CreateUserRequest(string name)
        {
            var variables = new { Name = name };
            return new GraphQLRequest { Query = this.createUserRequestQuery, Variables = variables };
        }

        public GraphQLRequest CheckIfUserExistsRequest(int id)
        {
            var variables = new { Id = id };
            return new GraphQLRequest { Query = this.checkIfUserExistRequestQuery, Variables = variables };
        }
    }

}
