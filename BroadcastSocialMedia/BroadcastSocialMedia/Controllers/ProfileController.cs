using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            var broadcasts = _dbContext.Broadcasts.Where(b => b.UserId == user.Id).ToList();

            var viewModel = new ProfileIndexViewModel()
            {
                Name = user.Name ?? "",
                CurrentProfilePicturePath = user.ProfilePicturePath,
                Broadcasts = broadcasts,
                Username = user.UserName
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Update(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            user.Name = viewModel.Name;

            if (!string.IsNullOrEmpty(viewModel.SelectedImagePath))
            {
                user.ProfilePicturePath = viewModel.SelectedImagePath;
            }

            await _userManager.UpdateAsync(user);

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
                await _userManager.UpdateAsync(user);
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

    }
}
