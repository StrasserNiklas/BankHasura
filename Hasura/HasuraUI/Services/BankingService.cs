using HasuraUI.Models;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketLibrary;

namespace HasuraUI.Services
{
    public class WebSocketService
    {
        private WebSocketClient<SubscriptionRequest, SubscriptionResponse> webSocketClient;

        public WebSocketService(Action<SubscriptionResponse> callback)
        {
            this.webSocketClient = new WebSocketClient<SubscriptionRequest, SubscriptionResponse>(
                "wss://hasurawebsocketserver2022032823400.azurewebsites.net/ws",
                callback,
                null,
                new JsonSerializerOptions()
                );

            Console.WriteLine("Starting websockets");
            this.webSocketClient.StartProcessingAsync();
        }

        public void SendRequest(int id)
        {
            this.webSocketClient.Send(new SubscriptionRequest () { Id = id });
            Console.WriteLine("Sent id to websocket");
        }
    }

    public class BankingService
    {
        public event EventHandler OnUpdate;

        private readonly GraphQlService graphQlService;
        public List<PaymentTransaction> Transactions { get;  }
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

        public async Task<string> CreatePaymentAsync(int senderId, int recepientId, double amount, string description)
        {
            // TODO maybe check if recipient exists first

            var result = await this.graphQlService.CreatePaymentAsync(senderId, recepientId, amount, description);
            var payment = result.Insert_payments_one;

            if (payment != null)
            {
                payment.Id = payment.Id;
                payment.Amount = amount;
                payment.Description = description;
                payment.Sender.Id = senderId;
                payment.Recipient.Id = recepientId;
                this.Payments.Add(payment);
            }

            return JsonSerializer.Serialize(result);
        }

        public async Task GetPaymentsAfterLoginAsync(int id)
        {
            var result = await this.graphQlService.GetPaymentsAsync(id);
            this.Payments.AddRange(result.Payments);
        }

        public async Task GetTransactionsAfterLoginAsync(int id)
        {
            var result = await this.graphQlService.GetTransactionsAsync(id);
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
