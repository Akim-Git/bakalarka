namespace back_end_2.Models
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string QuestionType { get; set; }
        public byte[] ImageDataQuestion { get; set; }
        public string TimeForAnswer { get; set; }
        public List<string> Answers { get; set; } = new List<string>(); // ✅ Je měnitelné
    }
}
