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

            var followingIds = await _dbContext.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.ListeningTo.Select(f => f.Id))
                .ToListAsync();

            var broadcasts = await _dbContext.Broadcasts
                .Include(b => b.User)
                .Include(b => b.Likes)
                .ThenInclude(l => l.User) 
                .Where(b => b.UserId == user.Id || followingIds.Contains(b.UserId))
                .OrderByDescending(b => b.Published)
                .ToListAsync();

            var viewModel = new HomeIndexViewModel
            {
                Broadcasts = broadcasts.Select(b => new BroadcastWithLikesViewModel
                {
                    Broadcast = b,
                    LikeCount = b.Likes.Count,
                    UserLiked = b.Likes.Any(l => l.UserId == user.Id),
                    LikedUsers = b.Likes.Select(l => new UserProfileViewModel
                    {
                        UserId = l.UserId,
                        Name = l.User.Name
                    }).ToList() 
                }).ToList(),
                FollowedUsers = await GetFollowedUsers(user.Id)
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

            if (string.IsNullOrEmpty(viewModel.Message) && viewModel.Image == null)
            {
                ModelState.AddModelError("", "Please provide either a message or an image.");
                return View("Index", viewModel);
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int broadcastId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var existingLike = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.BroadcastId == broadcastId && l.UserId == user.Id);

            if (existingLike != null)
            {
                _dbContext.Likes.Remove(existingLike);
            }
            else
            {
                var like = new Like
                {
                    BroadcastId = broadcastId,
                    UserId = user.Id
                };
                _dbContext.Likes.Add(like);
            }

            await _dbContext.SaveChangesAsync();

            var likeCount = await _dbContext.Likes.CountAsync(l => l.BroadcastId == broadcastId);
            _logger.LogInformation($"Broadcast ID: {broadcastId}, Like Count: {likeCount}, User ID: {user.Id}");
            return Content(likeCount.ToString()); 
        }


        public bool UserLikedBroadcast(int broadcastId)
        {
            var user = _userManager.GetUserAsync(User).Result;
            return _dbContext.Likes.Any(l => l.BroadcastId == broadcastId && l.UserId == user.Id);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBroadcast(int broadcastId)
        {
            var broadcast = await _dbContext.Broadcasts
                .Include(b => b.Likes) 
                .FirstOrDefaultAsync(b => b.Id == broadcastId);

            if (broadcast == null)
            {
                return NotFound();
            }

            _dbContext.Likes.RemoveRange(broadcast.Likes);

            _dbContext.Broadcasts.Remove(broadcast);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Featured()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            var broadcasts = await _dbContext.Broadcasts
                .Include(b => b.User)
                .Include(b => b.Likes)
                .OrderByDescending(b => b.Likes.Count)
                .Take(10)
                .ToListAsync();

            var followedUsers = await _dbContext.Users
                .Where(u => u.ListeningTo.Any(l => l.Id == user.Id))
                .Select(u => new UserProfileViewModel
                {
                    UserId = u.Id,
                    Name = u.Name,
                    ProfilePicturePath = u.ProfilePicturePath
                }).ToListAsync();

            var viewModel = new HomeIndexViewModel
            {
                Broadcasts = broadcasts.Select(b => new BroadcastWithLikesViewModel
                {
                    Broadcast = b,
                    LikeCount = b.Likes.Count,
                    UserLiked = user != null && b.Likes.Any(l => l.UserId == user.Id)
                }).ToList(),
                FollowedUsers = followedUsers 
            };

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
