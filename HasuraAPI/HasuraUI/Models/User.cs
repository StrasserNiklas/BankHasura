namespace HasuraUI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class UserResult
    {
        public User Insert_user_one { get; set; }
    }

    public class UserExistsResult
    {
        public User User_by_pk { get; set; }
    }

    public class PaymentTransaction
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public User Sender { get; set; }
        public User Recipient { get; set; }
    }

    public class PaymentsResult
    {
        public List<PaymentTransaction> Payments { get; set; }
    }

    public class TransactionsResult
    {
        public List<PaymentTransaction> Transactions { get; set; }
    }
}
