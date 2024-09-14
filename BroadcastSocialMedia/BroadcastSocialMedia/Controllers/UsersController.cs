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
            var broadcasts = await _dbContext.Broadcasts.Where(b => b.User.Id == id)
                .OrderByDescending(b => b.Published)
                .ToListAsync();
            var user = await _dbContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

            var viewModel = new UsersShowUserViewModel()
            {
                Broadcasts = broadcasts,
                User = user
            };

            return View(viewModel);
        }

        [HttpPost, Route("/Users/Listen")]
        public async Task<IActionResult> ListenToUser(UsersListenToUserViewModel viewModel)
        {
            // Retrieve the logged-in user
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Redirect("/Account/Login"); // Redirect if user is not logged in
            }

            // Retrieve the user that the logged-in user wants to listen to
            var userToListenTo = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);

            if (userToListenTo != null && !loggedInUser.ListeningTo.Contains(userToListenTo))
            {
                // Logging the action: user attempting to listen to another user
                _logger.LogInformation("User: {UserId} is attempting to listen to {OtherUserId}", loggedInUser.Id, userToListenTo.Id);

                // Add the user to the listening list
                loggedInUser.ListeningTo.Add(userToListenTo);
                await _userManager.UpdateAsync(loggedInUser); // Update the user
                await _dbContext.SaveChangesAsync(); // Save the changes in the database
            }

            return Redirect("/");
        }


    }
}
