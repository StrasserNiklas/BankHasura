namespace HasuraAPI;

public class TransactionServiceWorker : IHostedService
{
    private readonly ITransactionService transactionService;

    public TransactionServiceWorker(ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.transactionService.Subscribe();
        return Task.FromResult(1);
    }

    public Task StopAsync(CancellationToken cancellationToken) 
    {
        return Task.FromResult(1);
    }
}