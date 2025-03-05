namespace WebApplication1.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Sex { get; set; }
        public string Breed { get; set; }
        public byte[] ImageData { get; set; } // Vlastnost pro uložení binárních dat obrázku
        public int ShelterId { get; set; }
        public Shelter Shelter { get; set; }
        public ICollection<Comment> Comments { get; set; } // Navigační vlastnost pro komentáře
    }
}
