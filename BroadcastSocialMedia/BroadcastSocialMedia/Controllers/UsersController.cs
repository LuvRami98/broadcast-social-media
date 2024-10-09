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
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Redirect("/Account/Login");
            }

            if (!string.IsNullOrEmpty(viewModel.Search))
            {
                viewModel.Result = await _dbContext.Users
                    .Where(u => u.Name.Contains(viewModel.Search))
                    .ToListAsync();
            }

            viewModel.FollowedUsers = await GetFollowedUsers(loggedInUser.Id);
            return View(viewModel);
        }

        public async Task<IActionResult> ShowUser(string userId)
        {
            var loggedInUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var broadcasts = await _dbContext.Broadcasts
                .Include(b => b.Likes)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var viewModel = new UsersShowUserViewModel
            {
                User = user,
                Broadcasts = broadcasts.Select(b => new BroadcastWithLikesViewModel
                {
                    Broadcast = b,
                    LikeCount = b.Likes.Count,
                    UserLiked = loggedInUser != null && b.Likes.Any(l => l.UserId == loggedInUser.Id)
                }).ToList(),
                LoggedInUser = loggedInUser,
                FollowedUsers = await GetFollowedUsers(loggedInUser.Id)
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

        public async Task<IActionResult> UsersNotFollowed()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Redirect("/Account/Login");
            }

            var followedUserIds = await _dbContext.Users
                .Where(u => u.ListeningTo.Any(l => l.Id == loggedInUser.Id))
                .Select(u => u.Id)
                .ToListAsync();

            var usersNotFollowed = await _dbContext.Users
                .Where(u => !followedUserIds.Contains(u.Id) && u.Id != loggedInUser.Id)
                .Select(u => new
                {
                    User = u,
                    FollowedByCount = _dbContext.Users
                        .Where(followedUser => followedUserIds.Contains(followedUser.Id) && followedUser.ListeningTo.Any(l => l.Id == u.Id))
                        .Count()
                })
                .OrderByDescending(u => u.FollowedByCount)  
                .ThenBy(u => u.User.Name)                   
                .Take(10)                                   
                .ToListAsync();

            var viewModel = usersNotFollowed.Select(u => new UserProfileViewModel
            {
                UserId = u.User.Id,
                Name = u.User.Name,
                ProfilePicturePath = u.User.ProfilePicturePath
            }).ToList();

            var followedUsers = await _dbContext.Users
                .Where(u => u.ListeningTo.Any(l => l.Id == loggedInUser.Id))
                .Select(u => new UserProfileViewModel
                {
                    UserId = u.Id,
                    Name = u.Name,
                    ProfilePicturePath = u.ProfilePicturePath
                }).ToListAsync();

            ViewBag.FollowedUsers = followedUsers;

            return View(viewModel);
        }


        public async Task<List<UserProfileViewModel>> GetFollowedUsers(string userId)
        {
            var followedUsers = await _dbContext.Users
                .Where(u => u.ListeningTo.Any(l => l.Id == userId))
                .Select(u => new UserProfileViewModel
                {
                    UserId = u.Id,
                    Name = u.Name,
                    ProfilePicturePath = u.ProfilePicturePath
                }).ToListAsync();

            return followedUsers;
        }
    }
}