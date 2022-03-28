using HasuraUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HasuraUI.Services
{
    public class BankingService
    {
        private readonly GraphQlService graphQlService;
        public List<PaymentTransaction> Transactions { get;  }
        public List<PaymentTransaction> Payments { get; }


        public BankingService(GraphQlService graphQlService)
        {
            this.graphQlService = graphQlService;
            this.Transactions = new List<PaymentTransaction>();
            this.Payments = new List<PaymentTransaction>();
        }

        public async Task<int> LoginAsync(string userInput)
        {
            return await this.graphQlService.LoginAsync(userInput);
        }

        public async Task<string> CreatePaymentAsync(int senderId, int recepientId, double amount, string description)
        {
            // TODO maybe check if recipient exists first

            var result = await this.graphQlService.CreatePaymentAsync(senderId, recepientId, amount, description);
            //var payment = result.Insert_Payments_One;

            //if (payment != null)
            //{
            //    payment.Amount = amount;
            //    payment.Description = description;
            //    payment.Sender.Id = senderId;
            //    payment.Recipient.Id = recepientId;
            //    this.Payments.Add(payment);
            //}

            return JsonSerializer.Serialize(result);
        }

        public async Task GetPaymentsAfterLoginAsync(int id)
        {
            var result = await this.graphQlService.GetPaymentsAsync(id);
            this.Payments.AddRange(result.Payments);
            //return JsonSerializer.Serialize(result.Payments);
        }

        public async Task GetTransactionsAfterLoginAsync(int id)
        {
            var result = await this.graphQlService.GetTransactionsAsync(id);
            //return JsonSerializer.Serialize(result.Transactions);
            this.Transactions.AddRange(result.Transactions);
        }

        public Task CreatePaymentSubscription(int id)
        {
            //var sub = 
                this.graphQlService.GetPaymentsSubscription(id, async (x) => await this.HandlePayments(x.Payments));
            //sub.Subscribe(x => this.HandlePayments(x.Data.Payments));
            return Task.FromResult(1);
        }

        public Task<string> CreateTransactionsSubscription(int id)
        {
            //var sub = 
                this.graphQlService.GetTransactionsSubscription(id, async (x) => await this.HandleTransactions(x.Transactions)); return Task.FromResult("");
            //sub.Subscribe(x => this.HandleTransactions(x.Data.Transactions));
            //return Task.FromResult(JsonSerializer.Serialize(sub));
        }

        private async Task HandlePayments(List<PaymentTransaction> payments)
        {
            Console.WriteLine("Payments drin");
            //throw new OutOfMemoryException();

            var payment = payments.FirstOrDefault();

            if (payment is null)
            {
                return;
            }

            var paymentToEdit = this.Payments.SingleOrDefault(x => x.Id == payment.Id);

            if (paymentToEdit is not null)
            {
                paymentToEdit.Status = payment.Status;
            }

            await Task.FromResult(1);
        }

        private async Task HandleTransactions(List<PaymentTransaction> transactions)
        {
            Console.WriteLine("Transactions drin");

            var payment = transactions.FirstOrDefault();

            if (payment is null)
            {
                return;
            }

            this.Transactions.Add(payment);

            await Task.FromResult(1);
        }
    }
}
