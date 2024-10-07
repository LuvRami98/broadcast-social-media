using BroadcastSocialMedia.Models;

namespace BroadcastSocialMedia.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<BroadcastWithLikesViewModel> Broadcasts { get; set; }
        public Dictionary<int, bool> UserLikes { get; set; }
    }
}
