namespace HasuraSharedLib.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int Sender_Id { get; set; }
        public int Recipient_Id { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
