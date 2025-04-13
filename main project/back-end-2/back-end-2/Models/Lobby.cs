using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace back_end_2.Models
{
    public class Lobby
    {
        public int Id { get; set; }

        [Required]
       // [StringLength(50, MinimumLength = 3, ErrorMessage = "Název lobby musí mít 3 až 50 znaků.")]
        public string Name { get; set; }

        public int QuizId { get; set; } // cizí klíč pro kvíz

        //public Quiz Quiz { get; set; }

        //// mnoho hráčů v lobby
        public ICollection<Player> Players { get; set; } = new List<Player>();

        public string LobbyOwner { get; set; }

        public bool IsActive { get; set; } = false; // nastaví se na true pouze , když do lobby se přípojí moderator

        public string LobbyPassword { get; set; } = "";

        public string ModerConnectionId { get; set; } = "";

        public bool AcceptingAnswers { get; set; } = false;
    }
}
