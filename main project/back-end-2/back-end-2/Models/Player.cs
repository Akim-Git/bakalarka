using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end_2.Models
{
    public class Player
    {
        public int Id { get; set; }

        // typ string , ptotože v log-in-lobby (LobbyController) je porovnání UserId z Players
        //s Id v AspNetUser . který má typ string
        public string UserId { get; set; }

        public string UserName { get; set; }

        public int Score { get; set; }

        public int LobbyId { get; set; }

        public string ConnectionId { get; set; }

        //public bool IsUserBlocked { get; set; } = false;

        [ForeignKey("LobbyId")]
        public Lobby Lobby { get; set; }

        public bool DidAnswer { get; set; } = false;
        public int QuestionId { get; set; }

        public string Answer { get; set; }

        public bool IsAnswerCorrect { get; set; } = false;
    }
}