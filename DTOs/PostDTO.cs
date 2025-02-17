using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class PostDTO
    {
        public required string Activity { get; set; }
        public required string Description { get; set; }
        public required string ExpiredTime { get; set; }
        public required string AppointmentTime { get; set; }
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
        public List<QuestionDTO> Questions { get; set; } = new List<QuestionDTO>();
    }