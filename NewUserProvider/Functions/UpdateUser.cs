using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewUserProvider.Functions
{
    public class UpdateUserByEmail(ILogger<UpdateUserByEmail> logger, DataContext context)
    {
        private readonly ILogger<UpdateUserByEmail> _logger = logger;
        private readonly DataContext _context = context;

        [Function("UpdateUserByEmail")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/email/{email}")] HttpRequestData req,
            string email)
        {
            _logger.LogInformation("Processing request to update user by email.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedUser = JsonConvert.DeserializeObject<User>(requestBody);

            if (updatedUser == null || email != updatedUser.Email)
            {
                return new BadRequestObjectResult("Invalid user data or email mismatch.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new NotFoundObjectResult("User not found.");
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            // Add other fields to update as necessary

            try
            {
                await _context.SaveChangesAsync();
                return new OkObjectResult(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
