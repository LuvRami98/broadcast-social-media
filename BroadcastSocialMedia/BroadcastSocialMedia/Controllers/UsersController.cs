using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ILogger<UsersController> logger, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(UsersIndexViewModel viewModel)
        {

            if (viewModel.Search != null)
            {
                var users = await _dbContext.Users.Where(u => u.Name.Contains(viewModel.Search))
                    .ToListAsync();
                viewModel.Result = users;
            }

            return View(viewModel);
        }

        [Route("/Users/{id}")]
        public async Task<IActionResult> ShowUser(string id)
        {
            var broadcasts = await _dbContext.Broadcasts
                .Where(b => b.User.Id == id)
                .OrderByDescending(b => b.Published)
                .ToListAsync();

            var user = await _dbContext.Users
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();

            var loggedInUser = await _userManager.Users
                .Include(u => u.ListeningTo)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            var viewModel = new UsersShowUserViewModel()
            {
                Broadcasts = broadcasts,
                User = user,
                LoggedInUser = loggedInUser
            };

            return View(viewModel);
        }

        [HttpPost, Route("/Users/Listen")]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> ListenToUser(UsersListenToUserViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                _logger.LogWarning("No logged-in user found.");
                return Redirect("/Account/Login");
            }

            _logger.LogInformation("Received Listen request with UserId: {UserId}", viewModel.UserId);
            _logger.LogInformation("Anti-Forgery Token: {AntiForgeryToken}", HttpContext.Request.Headers["X-CSRF-TOKEN"]);

            foreach (var header in HttpContext.Request.Headers)
            {
                _logger.LogInformation("Header: {Header} = {Value}", header.Key, header.Value);
            }

            var userToListenTo = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);
            if (userToListenTo == null)
            {
                _logger.LogWarning("User to listen to not found.");
                return BadRequest(); 
            }

            _logger.LogInformation("Adding user {OtherUserId} to {UserId}'s listening list", userToListenTo.Id, loggedInUser.Id);

            if (!loggedInUser.ListeningTo.Any(u => u.Id == userToListenTo.Id))
            {
                loggedInUser.ListeningTo.Add(userToListenTo);
                await _dbContext.SaveChangesAsync();  
                _logger.LogInformation("User {UserId} successfully added to listening list", userToListenTo.Id);
            }
            else
            {
                _logger.LogInformation("User {UserId} is already listening to {OtherUserId}", loggedInUser.Id, userToListenTo.Id);
            }

            return Ok(); 
        }

        [HttpPost, Route("/Users/Unlisten")]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> StopListeningToUser(UsersListenToUserViewModel viewModel)
        {
            _logger.LogInformation("Received Unlisten request with UserId: {UserId}", viewModel.UserId);
            _logger.LogInformation("Anti-Forgery Token: {AntiForgeryToken}", HttpContext.Request.Headers["X-CSRF-TOKEN"]);

            foreach (var header in HttpContext.Request.Headers)
            {
                _logger.LogInformation("Header: {Header} = {Value}", header.Key, header.Value);
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Redirect("/Account/Login");
            }

            var userToStopListeningTo = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);
            if (userToStopListeningTo == null)
            {
                _logger.LogWarning("User to stop listening to not found.");
                return BadRequest(); 
            }

            _logger.LogInformation("User {UserId} is attempting to stop listening to {OtherUserId}", loggedInUser.Id, userToStopListeningTo.Id);

            if (loggedInUser.ListeningTo.Contains(userToStopListeningTo))
            {
                _dbContext.Entry(loggedInUser).Collection(u => u.ListeningTo).Load();
                loggedInUser.ListeningTo.Remove(userToStopListeningTo);
                await _dbContext.SaveChangesAsync();  
                _logger.LogInformation("User {UserId} successfully removed from listening list", userToStopListeningTo.Id);
            }

            return Ok(); 
        }
    }
}
