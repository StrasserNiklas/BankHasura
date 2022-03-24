using Microsoft.AspNetCore.Mvc;

namespace HasuraAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HasuraController : ControllerBase
{
    private readonly ITransactionService transactionService;

    public HasuraController(ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    //[Route("user")]
    //[HttpPost]
    //public async Task<ActionResult> CreateUser([FromQuery] string user)
    //{
    //    await this.transactionService.CreateUser(user);
    //    return Ok();
    //}

    //[Route("transaction")]
    //[HttpPost]
    //public async Task<ActionResult> CreateTransaction([FromQuery] string sender, string recipient, double amount)
    //{
    //    await this.transactionService.CreateTransaction(sender, recipient, amount);
    //    return Ok();
    //}
}
