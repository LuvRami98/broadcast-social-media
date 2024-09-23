using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.Services;
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
        private readonly IUserService _userService;
        private readonly IBroadcastService _broadcastService;

        public UsersController(ILogger<UsersController> logger, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IUserService userService, IBroadcastService broadcastService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _userService = userService;
            _broadcastService = broadcastService;
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

        public async Task<IActionResult> ShowUser(string userId)
        {
            var loggedInUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User)); // Make sure ListeningTo is loaded

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var broadcasts = await _broadcastService.GetBroadcastsForUser(userId);

            var viewModel = new UsersShowUserViewModel
            {
                User = user,
                Broadcasts = broadcasts,
                LoggedInUser = loggedInUser
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Listen(UsersListenToUserViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.UserId))
            {
                return BadRequest("UserId is required.");
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                _logger.LogWarning("No logged-in user found.");
                return Redirect("/Account/Login");
            }

            _logger.LogInformation("Received Listen request with UserId: {UserId}", viewModel.UserId);

            await _userService.ListenToUserAsync(loggedInUser.Id, viewModel.UserId);

            return Ok(); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlisten(UsersListenToUserViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.UserId))
            {
                return BadRequest("UserId is required.");
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                _logger.LogWarning("No logged-in user found.");
                return Redirect("/Account/Login");
            }

            await _userService.UnlistenToUserAsync(loggedInUser.Id, viewModel.UserId);

            TempData["SuccessMessage"] = $"You have stopped listening to the user.";
            return RedirectToAction("ShowUser", new { userId = viewModel.UserId });
        }

    }
}