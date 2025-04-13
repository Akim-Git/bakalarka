using back_end_2.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class TeamAnswer
{
    public int Id { get; set; }

    public int TeamId { get; set; }

    [ForeignKey("TeamId")]
    public Team Team { get; set; }

    public int QuestionId { get; set; }

    public string Username { get; set; } // Kdo hlasoval

    public string Answer { get; set; }   // Co vybral

    //public int Votes { get; set; }
}
