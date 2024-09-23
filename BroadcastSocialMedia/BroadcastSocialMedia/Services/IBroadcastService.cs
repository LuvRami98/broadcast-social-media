using BroadcastSocialMedia.Models;   
using System.Collections.Generic;    
using System.Threading.Tasks;        


namespace BroadcastSocialMedia.Services
{
    public interface IBroadcastService
    {
        Task<List<Broadcast>> GetBroadcastsForUser(string userId);
    }
}
