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
        private string getTransactionRequestQuery;

        public GraphQLRequest GetTransactionsRequest(int id)
        {
            var variables = new { Id = id };
            return new GraphQLRequest { Query = this.getTransactionRequestQuery, Variables = variables };
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
        }
    }

}
