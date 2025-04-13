using System.ComponentModel.DataAnnotations;

namespace back_end_2.Models
{
    public class LobbyDTO
    {

        [Required]
       
        public string Name { get; set; }

        [Required]
        public int QuizId { get; set; }

        public string LobbyOwner { get; set; } = ""; // Defaultně prázdný string

        public string LobbyPassword { get; set; }
    }
}
