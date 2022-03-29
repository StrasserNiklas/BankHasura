using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HasuraUI.Services
{
    public class BankingService
    {
        public event EventHandler OnUpdate;

        private readonly GraphQlService graphQlService;
        public List<PaymentTransaction> Transactions { get; }
        public List<PaymentTransaction> Payments { get; }

        private WebSocketService websocketService;

        public BankingService(GraphQlService graphQlService)
        {
            this.graphQlService = graphQlService;
            this.Transactions = new List<PaymentTransaction>();
            this.Payments = new List<PaymentTransaction>();
            this.websocketService = new WebSocketService(async (x) => await HandlePaymentsAndTransactions(x.Result));
        }

        public async Task<int> LoginAsync(string userInput)
        {
            var userId = await this.graphQlService.LoginAsync(userInput);
            this.websocketService.SendRequest(userId);
            return userId;
        }

        public async Task CreatePaymentAsync(int senderId, int recepientId, double amount, string description)
        {
            // TODO maybe check if recipient exists first
            var newPayment = new PaymentTransaction
            {
                Amount = amount,
                Description = description,
                Sender = new User() { Id = senderId },
                Recipient = new User() { Id = recepientId },
                Status = "Sent"
            };

            this.Payments.Add(newPayment);

            var result = await this.graphQlService.CreatePaymentAsync(senderId, recepientId, amount, description);
            var payment = result.Insert_payments_one;

            Console.WriteLine("Payment created");
            Console.WriteLine(JsonSerializer.Serialize(result));

            if (payment != null)
            {
                newPayment.Id = payment.Id;
                newPayment.Status = payment.Status;
                newPayment.Recipient.Name = payment.Recipient.Name;
                //payment.Amount = amount;
                //payment.Description = description;
                //payment.Sender.Id = senderId;
                //payment.Recipient.Id = recepientId;
                //this.Payments.Add(payment);
            }
        }

        public async Task GetPaymentsAfterLoginAsync(int id)
        {
            var result = await this.graphQlService.GetPaymentsAsync(id);
            Console.WriteLine("GetPaymentsAfterLoginAsync");
            Console.WriteLine(JsonSerializer.Serialize(result));
            this.Payments.AddRange(result.Payments);
        }

        public async Task GetTransactionsAfterLoginAsync(int id)
        {
            var result = await this.graphQlService.GetTransactionsAsync(id);
            Console.WriteLine("GetTransactionsAfterLoginAsync");
            Console.WriteLine(JsonSerializer.Serialize(result));
            this.Transactions.AddRange(result.Transactions);
        }

        private async Task HandlePaymentsAndTransactions(PaymentTransaction paymentOrTransaction)
        {
            if (paymentOrTransaction is null)
            {
                return;
            }

            if (paymentOrTransaction.Status == null)
            {
                Console.WriteLine("Transactions drin");
                Console.WriteLine(JsonSerializer.Serialize(paymentOrTransaction));

                if (this.Transactions.Any(x => x.Id == paymentOrTransaction.Id))
                {
                    return;
                }

                this.Transactions.Add(paymentOrTransaction);
                return;
            }

            Console.WriteLine("Payments drin");
            Console.WriteLine(JsonSerializer.Serialize(paymentOrTransaction));

            var paymentToEdit = this.Payments.SingleOrDefault(x => x.Id == paymentOrTransaction.Id);

            if (paymentToEdit is not null)
            {
                paymentToEdit.Status = paymentOrTransaction.Status;
            }

            this.OnUpdate(this, EventArgs.Empty);
            await Task.FromResult(1);
        }
    }
}
