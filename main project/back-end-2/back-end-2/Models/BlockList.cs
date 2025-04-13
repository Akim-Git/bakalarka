namespace back_end_2.Models
{
    public class BlockList
    {
        public int Id { get; set; }
        public int LobbyId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ConnectionId { get; set; }
    }
}
