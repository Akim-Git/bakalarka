using back_end_2.Models;
using System.ComponentModel.DataAnnotations;

public class Quiz
{
    public int Id { get; set; } // primární klíč

    public string QuizOwner { get; set; } // vlastník kvízu

    [Required]
    public string Title { get; set; } // název kvízu (povinný atribut)

    public string? Description { get; set; } // popis kvízu

    public byte[]? ImageData { get; set; } // data obrázku jako binární pole

    public ICollection<Question> Questions { get; set; }// vztah "one-to-many" s entitou Question (několik otázek v kvízu)
}