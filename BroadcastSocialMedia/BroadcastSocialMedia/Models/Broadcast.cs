﻿namespace BroadcastSocialMedia.Models
{
    public class Broadcast
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime Published { get; set; } = DateTime.Now;
    }
}
