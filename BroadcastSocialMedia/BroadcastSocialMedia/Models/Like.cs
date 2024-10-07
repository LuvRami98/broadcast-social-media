namespace BroadcastSocialMedia.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public ApplicationUser User { get; set; } 

        public int BroadcastId { get; set; } 
        public Broadcast Broadcast { get; set; } 
    }
}
