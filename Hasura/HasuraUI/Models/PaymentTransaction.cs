using System;

namespace HasuraUI.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public User Sender { get; set; }
        public User Recipient { get; set; }
    }
}
