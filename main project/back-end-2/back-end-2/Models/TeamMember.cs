using System.ComponentModel.DataAnnotations.Schema;

namespace back_end_2.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
    }

}
