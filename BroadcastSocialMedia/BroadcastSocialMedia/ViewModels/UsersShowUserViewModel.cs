using BroadcastSocialMedia.Models;

public class UsersShowUserViewModel
{
    public ApplicationUser User { get; set; }
    public List<Broadcast> Broadcasts { get; set; }
    public ApplicationUser LoggedInUser { get; set; } 
}
