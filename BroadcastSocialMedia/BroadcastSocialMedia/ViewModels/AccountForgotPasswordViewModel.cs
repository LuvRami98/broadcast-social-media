using System.ComponentModel.DataAnnotations;

namespace BroadcastSocialMedia.ViewModels
{
    public class AccountForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
