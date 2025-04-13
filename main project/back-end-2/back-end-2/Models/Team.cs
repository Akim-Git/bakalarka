namespace back_end_2.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string LobbyId { get; set; }
        public string TeamName { get; set; }
        public int TeamScore { get; set; }

        public List<TeamMember> Players { get; set; } = new();

        public List<TeamAnswer> TeamAnswers { get; set; } = new();
        //public ICollection<TeamAnswer> Answers { get; set; } = new List<TeamAnswer>();
    }

}
