using BroadcastSocialMedia.Models;

namespace BroadcastSocialMedia.ViewModels
{
    public class ProfileIndexViewModel
    {
        public string Name { get; set; }
        public IFormFile ProfilePicture { get; set; }

        public string CurrentProfilePicturePath { get; set; }
        public List<BroadcastWithLikesViewModel> Broadcasts { get; set; }
        public string SelectedImagePath { get; set; }

        public string Username { get; set; }
        public Dictionary<int, bool> UserLikes { get; set; }
    }
}
