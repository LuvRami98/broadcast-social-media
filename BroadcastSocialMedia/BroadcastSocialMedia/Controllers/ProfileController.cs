using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BroadcastSocialMedia.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new ProfileIndexViewModel()
            {
                Name = user.Name ?? "",
                CurrentProfilePicturePath = user.ProfilePicturePath
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            user.Name = viewModel.Name;

            await _userManager.UpdateAsync(user);

            return Redirect("/");
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
    }

}
