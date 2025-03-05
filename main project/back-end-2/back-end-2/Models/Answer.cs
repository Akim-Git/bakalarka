using System.ComponentModel.DataAnnotations;

namespace back_end_2.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }
        public int QuestionId { get; set; }

        [Required]
        public bool IsCorrect { get; set; }
    }

}
