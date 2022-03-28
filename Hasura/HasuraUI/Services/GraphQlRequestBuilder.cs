using GraphQL;
using System.Net.Http;
using System.Threading.Tasks;

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
        private readonly HttpClient httpClient;

        public GraphQlRequestBuilder(HttpClient httpClient)
        {
            this.httpClient = httpClient;
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

        public void LoadFiles()
        {
            //this.createUserRequestQuery = await this.httpClient.GetStringAsync(@"Querys/CreateUser.graphql");
            //this.checkIfUserExistRequestQuery = await this.httpClient.GetStringAsync(@"Querys/CheckUserExists.graphql");
            //this.createPaymentRequestQuery = await this.httpClient.GetStringAsync(@"Querys/CreatePayment.graphql");
            //this.getPaymentsRequestQuery = await this.httpClient.GetStringAsync(@"Querys/GetPaymentsQuery.graphql");
            //this.getPaymentsRequestQuerySubscription = await this.httpClient.GetStringAsync(@"Querys/GetPaymentsSubscription.graphql");
            //this.getTransactionRequestQuery = await this.httpClient.GetStringAsync(@"Querys/GetTransactionsQuery.graphql");
            //this.getTransactionRequestQuerySubscription = await this.httpClient.GetStringAsync(@"Querys/GetTransactionsSubscription.graphql");

            this.createUserRequestQuery = @"
mutation CreateUser($name: String) {
  insert_user_one(object: {
    name: $name
    }) {
    id
  }
}
";
            this.checkIfUserExistRequestQuery = @"
query CheckIfUserExists($id: Int!) {
  user_by_pk(id: $id) {
    name
  }
}
";
            this.createPaymentRequestQuery = @"
mutation CreatePayment($sender: Int, $recipient: Int, $amount: numeric, $description: String) {
  insert_payments_one(object: { sender_id: $sender, recipient_id: $recipient, amount: $amount, description: $description}) {
    id
    created_at
    recipient {
      name
    }
    sender {
      name
    }
  }
}
";
            this.getPaymentsRequestQuery = @"
query GetPayments($id: Int!) {
  payments(where: { sender_id: {_eq: $id}} ) {
	id
    created_at
    amount
    status
    description
    recipient {
      name
    }
    sender {
      name
    }
  }
}
";
            this.getPaymentsRequestQuerySubscription = @"
subscription PaymentSubscription($id: Int!) {
  payments(limit: 1, order_by: {created_at: desc}, where: { sender_id: {_eq: $id}}) {
    id
	created_at
    status
    recipient {
      name
    }
  }
}

";
            this.getTransactionRequestQuery = @"
query GetTransactions($id: Int!) {
  transactions(where: {_or: [{sender_id: {_eq: $id}}, {recipient_id: {_eq: $id}}]} ) {
	id
    created_at
    amount
	description
    sender {
      name
    }
    recipient {
      name
    }
  }
}
";
            this.getTransactionRequestQuerySubscription = @"
subscription TransactionSubscription($id: Int!) {
  transactions(limit: 1, order_by: {created_at: desc}, where: {_or: [{sender_id: {_eq: $id}}, {recipient_id: {_eq: $id}}]}) {
    amount
    created_at
    description
    id
    recipient {
      name
    }
    sender {
      name
    }
  }
}
";
        }
    }

}
