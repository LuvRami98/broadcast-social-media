using System.ComponentModel.DataAnnotations;

public class HomeBroadcastViewModel : IValidatableObject
{
    public string Message { get; set; }
    public IFormFile Image { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Message) && Image == null)
        {
            yield return new ValidationResult("Either content or an image must be provided.");
        }
    }
}
