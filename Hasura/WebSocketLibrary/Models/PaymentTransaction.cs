using System;

namespace SharedLibrary.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public DateTime Created_At { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = null;
        public string Description { get; set; }

        public User Sender { get; set; }
        public User Recipient { get; set; }
    }
}
