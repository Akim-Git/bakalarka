using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using back_end_2.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }

    public DbSet<IdentityUserRole<string>> AspNetUserRoles { get; set; }
    public DbSet<IdentityRole> AspNetRoles { get; set; }

    public DbSet<Lobby> Lobbies { get; set; }
    public DbSet<Player> Players { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Questions)
            .WithOne()
            .HasForeignKey(q => q.QuizId);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne()
            .HasForeignKey(a => a.QuestionId);

        modelBuilder.Entity<Lobby>()
            .HasMany(l => l.Players)   // lobby má více hráčů
            .WithOne(p => p.Lobby)     // hráč patří pouze do jednoho lobby
            .HasForeignKey(p => p.LobbyId)  
            .OnDelete(DeleteBehavior.Cascade); // když se smaže lobby, smažou se i hráči

        //modelBuilder.Entity<Player>()
        //    .HasOne(p => p.Lobby)
        //    .WithMany(l => l.Players)
        //    .HasForeignKey(p => p.LobbyId)
        //    .OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<Lobby>()
        //    .HasOne(l => l.Quiz)
        //    .WithMany()
        //    .HasForeignKey(l => l.QuizId);
    }
}
