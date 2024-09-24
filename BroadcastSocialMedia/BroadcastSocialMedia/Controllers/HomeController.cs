using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BroadcastSocialMedia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            var dbUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .ThenInclude(u => u.Broadcasts)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (dbUser == null)
            {
                return NotFound();
            }

            var broadcasts = dbUser.ListeningTo
                .SelectMany(u => u.Broadcasts)
                .OrderByDescending(b => b.Published)
                .ToList();

            var viewModel = new HomeIndexViewModel
            {
                Broadcasts = broadcasts
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Broadcast(HomeBroadcastViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            var broadcast = new Broadcast
            {
                Message = viewModel.Message,
                Published = DateTime.Now,
                UserId = user.Id
            };

            if (viewModel.Image != null && viewModel.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.Image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.Image.CopyToAsync(fileStream);
                }

                broadcast.ImagePath = "/uploads/" + uniqueFileName;
            }

            _dbContext.Broadcasts.Add(broadcast);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
