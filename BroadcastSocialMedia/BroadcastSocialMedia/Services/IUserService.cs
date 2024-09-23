using BroadcastSocialMedia.Models; 
using System.Collections.Generic;    
using System.Threading.Tasks;       


namespace BroadcastSocialMedia.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetLoggedInUserAsync();
        Task ListenToUserAsync(string loggedInUserId, string targetUserId);
        Task UnlistenToUserAsync(string loggedInUserId, string targetUserId);
    }
}
