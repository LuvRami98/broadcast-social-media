using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadcastSocialMedia.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment, ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            var broadcasts = await _dbContext.Broadcasts
                .Include(b => b.Likes)
                .Where(b => b.UserId == user.Id)
                .ToListAsync();

            var viewModel = new ProfileIndexViewModel
            {
                Name = user.Name ?? "",
                CurrentProfilePicturePath = user.ProfilePicturePath,
                Broadcasts = broadcasts.Select(b => new BroadcastWithLikesViewModel
                {
                    Broadcast = b,
                    LikeCount = b.Likes.Count,
                    UserLiked = b.Likes.Any(l => l.UserId == user.Id)
                }).ToList(),
                Username = user.UserName,
                FollowedUsers = await GetFollowedUsers(user.Id)
            };

            return View(viewModel);
        }


        public async Task<IActionResult> Update(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            if (!string.IsNullOrEmpty(viewModel.Name))
            {
                user.Name = viewModel.Name;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error updating profile.");
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ProfilePicture(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            if (viewModel.ProfilePicture != null && viewModel.ProfilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProfilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ProfilePicture.CopyToAsync(fileStream);
                }

                user.ProfilePicturePath = "/uploads/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error updating profile picture.");
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SelectExistingProfilePicture(string selectedImagePath)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                user.ProfilePicturePath = selectedImagePath;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsername(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            if (viewModel == null || string.IsNullOrEmpty(viewModel.Username))
            {
                ModelState.AddModelError("", "Please provide a username.");
                return RedirectToAction("Index");
            }

            var isUsernameTaken = _dbContext.Users.Any(u => u.UserName == viewModel.Username && u.Id != user.Id);
            if (isUsernameTaken)
            {
                ModelState.AddModelError("", "This username is already taken. Please choose another.");
                return RedirectToAction("Index");
            }

            user.UserName = viewModel.Username;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CheckUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { isTaken = false });
            }

            var isUsernameTaken = _dbContext.Users.Any(u => u.UserName == username);
            return Json(new { isTaken = isUsernameTaken });
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
