using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewUserProvider.Functions;

public class DeleteUser
{
    private readonly ILogger<DeleteUser> _logger;
    private readonly DataContext _context;

    public DeleteUser(ILogger<DeleteUser> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("DeleteUser")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/{id}")] HttpRequest req,
        string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return new BadRequestObjectResult("User ID is required.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return new NotFoundObjectResult($"User with ID '{id}' not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return new OkObjectResult($"User with ID '{id}' has been deleted.");
    }
}
