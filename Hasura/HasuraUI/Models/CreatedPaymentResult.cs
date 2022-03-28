using System;

namespace HasuraUI.Models
{
    public class CreatedPaymentResult
    {
        public PaymentTransaction Insert_Payments_One { get; set; }
    }

    public class Recipient
    {
        public string Name { get; set; }
    }

    public class Sender
    {
        public string Name { get; set; }
    }

    public class InsertPaymentsOne
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Recipient Recipient { get; set; }
        public Sender Sender { get; set; }
    }

    public class Data
    {
        public InsertPaymentsOne Insert_Payments_One { get; set; }
    }

}
