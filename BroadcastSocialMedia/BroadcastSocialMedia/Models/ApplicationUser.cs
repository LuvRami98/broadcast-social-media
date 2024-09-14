using Microsoft.AspNetCore.Identity;

namespace BroadcastSocialMedia.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }

        public ICollection<Broadcast> Broadcasts { get; set; }

        // Corrected many-to-many relationship definition for ListeningTo
        public ICollection<ApplicationUser> ListeningTo { get; set; } = new List<ApplicationUser>();

        // Optional: Users who are listening to this user (inverse relationship)
        public ICollection<ApplicationUser> Listeners { get; set; } = new List<ApplicationUser>();
    }
}
