using System.ComponentModel.DataAnnotations;

namespace back_end_2.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public string TimeForAnswer { get; set; }

        public int QuizId { get; set; }

        [Required]
        public string QuestionType { get; set; }

        public byte[]? ImageDataQuestion { get; set; }

        public ICollection<Answer> Answers { get; set; }
    }
}
