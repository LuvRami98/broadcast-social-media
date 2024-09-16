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

            // Eagerly load the 'ListeningTo' collection for the logged-in user
            var loggedInUser = await _userManager.Users
                .Include(u => u.ListeningTo)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            var viewModel = new UsersShowUserViewModel()
            {
                Broadcasts = broadcasts,
                User = user,
                LoggedInUser = loggedInUser // Pass the logged-in user to the view model
            };

            return View(viewModel);
        }



        [HttpPost, Route("/Users/Listen")]
        public async Task<IActionResult> ListenToUser(UsersListenToUserViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Redirect("/Account/Login");
            }

            var userToListenTo = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);

            if (userToListenTo != null && !loggedInUser.ListeningTo.Any(u => u.Id == userToListenTo.Id)) 
            {
                _logger.LogInformation("User: {UserId} is attempting to listen to {OtherUserId}", loggedInUser.Id, userToListenTo.Id);

                loggedInUser.ListeningTo.Add(userToListenTo);
                await _userManager.UpdateAsync(loggedInUser);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("User: {UserId} is already listening to {OtherUserId}", loggedInUser.Id, userToListenTo.Id);
            }

            return Redirect($"/Users/{viewModel.UserId}");
        }


        [HttpPost, Route("/Users/Unlisten")]
        public async Task<IActionResult> StopListeningToUser(UsersListenToUserViewModel viewModel)
        {
            _logger.LogInformation("StopListeningToUser called for UserId: {UserId}", viewModel.UserId); // Add this for debugging

            // Get the logged-in user
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Redirect("/Account/Login");
            }

            // Get the user to stop listening to
            var userToStopListeningTo = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);

            // Check if the user exists and the logged-in user is currently listening to them
            if (userToStopListeningTo != null && loggedInUser.ListeningTo.Contains(userToStopListeningTo))
            {
                _logger.LogInformation("User: {UserId} is stopping listening to {OtherUserId}", loggedInUser.Id, userToStopListeningTo.Id);

                _dbContext.Entry(loggedInUser).Collection(u => u.ListeningTo).Load();
                loggedInUser.ListeningTo.Remove(userToStopListeningTo);

                _dbContext.Update(loggedInUser);
                await _dbContext.SaveChangesAsync();

                // Log success
                _logger.LogInformation("Successfully stopped listening to {OtherUserId}", userToStopListeningTo.Id);
            }

            TempData["SuccessMessage"] = "You have successfully stopped listening to the user.";
            return Redirect($"/Users/{viewModel.UserId}");

        }

    }
}
