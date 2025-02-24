using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class PostDTO
    {
        [Required]
        public required string Activity { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required DateTimeOffset ExpiredTime { get; set; }
        [Required]
        public required DateTimeOffset AppointmentTime { get; set; }
        [Required]
        public required int Limit { get; set; }
        [Required]
        public required string Location { get; set; }
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
        public List<QuestionDTO> Questions { get; set; } = new List<QuestionDTO>();
    }