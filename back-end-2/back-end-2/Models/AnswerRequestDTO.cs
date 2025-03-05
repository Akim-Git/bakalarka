namespace back_end_2.Models
{
    public class AnswerRequestDTO
    {
        public int QuestionId { get; set; }
        public string UserInput { get; set; }
        public string QuestionType { get; set; }
    }
}
