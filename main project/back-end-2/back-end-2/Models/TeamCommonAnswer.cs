using System.ComponentModel.DataAnnotations.Schema;

namespace back_end_2.Models
{
    public class TeamCommonAnswer
    {
        public int Id { get; set; }

        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public Team Team { get; set; }
        public string TeamName { get; set; }
        public int LobbyId { get; set; }

        public int QuestionId { get; set; }
        public string Answer { get; set; } 
    }
}
