using BroadcastSocialMedia.Models;

namespace BroadcastSocialMedia.ViewModels
{
    public class BroadcastWithLikesViewModel
    {
        public Broadcast Broadcast { get; set; }
        public int LikeCount { get; set; }
        public bool UserLiked { get; set; }

        public List<UserProfileViewModel> LikedUsers { get; set; } = new List<UserProfileViewModel>();
    }

}
