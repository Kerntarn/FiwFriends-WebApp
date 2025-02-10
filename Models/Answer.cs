namespace FiwFriends.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }
        public string Content { get; set; }
        public int QuestionId { get; set; }
        public Question? Question { get; set; }
        public int FormId { get; set; }
        public Form? Form { get; set; }
    }
}