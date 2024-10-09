using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;

public class UsersShowUserViewModel
{
    public ApplicationUser User { get; set; }
    public List<BroadcastWithLikesViewModel> Broadcasts { get; set; }
    public ApplicationUser LoggedInUser { get; set; }
    public Dictionary<int, bool> UserLikes { get; set; }
    public List<UserProfileViewModel> FollowedUsers { get; set; }
}
