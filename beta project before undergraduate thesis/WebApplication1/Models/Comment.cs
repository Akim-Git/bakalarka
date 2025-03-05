using WebApplication1.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DogId { get; set; } // Cizí klíč pro odkaz na psa
    public Dog Dog { get; set; } // Navigační vlastnost pro získání informací o psu
}
