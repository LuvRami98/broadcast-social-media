namespace BroadcastSocialMedia.ViewModels
{
    public class ProfileIndexViewModel
    {
        public string Name { get; set; }
        public IFormFile ProfilePicture { get; set; }

        public string CurrentProfilePicturePath { get; set; }
    }
}
