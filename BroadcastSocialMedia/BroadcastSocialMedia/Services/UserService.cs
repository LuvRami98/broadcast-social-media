using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models; 
using BroadcastSocialMedia.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<ApplicationUser> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<ApplicationUser> GetLoggedInUserAsync()
    {
        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task ListenToUserAsync(string loggedInUserId, string targetUserId)
    {
        var user = await _context.Users
            .Include(u => u.ListeningTo)
            .FirstOrDefaultAsync(u => u.Id == loggedInUserId);

        var targetUser = await _context.Users.FindAsync(targetUserId);

        if (user != null && targetUser != null && !user.ListeningTo.Contains(targetUser))
        {
            user.ListeningTo.Add(targetUser);
            await _context.SaveChangesAsync();
        }
    }


    public async Task UnlistenToUserAsync(string loggedInUserId, string targetUserId)
    {
        var user = await _context.Users
            .Include(u => u.ListeningTo)
            .FirstOrDefaultAsync(u => u.Id == loggedInUserId);

        var targetUser = await _context.Users.FindAsync(targetUserId);

        if (user != null && targetUser != null && user.ListeningTo.Contains(targetUser))
        {
            user.ListeningTo.Remove(targetUser);
            await _context.SaveChangesAsync();
        }
    }
}

