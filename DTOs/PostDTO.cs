using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class PostDTO
    {
        [Required(ErrorMessage = "Activity is required.")]
        public required string Activity { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(300, ErrorMessage = "Description cannot exceed 50 characters.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Expired time is required.")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Expired time must be in the future.")]
        public required DateTimeOffset ExpiredTime { get; set; }

        [Required(ErrorMessage = "Appointment time is required.")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Appointment time must be in the future.")]
        [DateLater(nameof(ExpiredTime))]
        public required DateTimeOffset AppointmentTime { get; set; }

        [Required(ErrorMessage = "Limit is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Limit must be at least 1.")]
        public required int Limit { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public required string Location { get; set; }

        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();

        public List<QuestionDTO> Questions { get; set; } = new List<QuestionDTO>();
    }


    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTimeOffset date && date <= DateTimeOffset.Now)
            {
                return new ValidationResult(ErrorMessage ?? "The date must be in the future.");
            }
            return ValidationResult.Success;
        }
    }

    public class DateLaterAttribute : ValidationAttribute{
        private readonly string _comparisonProperty;
        public DateLaterAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = value as DateTimeOffset?;

            var comparisonPropertyInfo = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (comparisonPropertyInfo == null) 
                return new ValidationResult($"Property '{_comparisonProperty}' not found.");
            
            var comparisonValue = comparisonPropertyInfo.GetValue(validationContext.ObjectInstance) as DateTimeOffset?;
            if (currentValue.HasValue && comparisonValue.HasValue && currentValue < comparisonValue) 
                return new ValidationResult($"{validationContext.DisplayName} must be later {_comparisonProperty}.");
            
            return ValidationResult.Success;
        }
    }
}
