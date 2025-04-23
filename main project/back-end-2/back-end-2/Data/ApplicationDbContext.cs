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
    public DbSet<BlockList> BlockList { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<TeamAnswer> TeamAnswers { get; set; }
    public DbSet<TeamCommonAnswer> TeamCommonAnswers { get; set; }
    //public DbSet<TeamAnswerVote> TeamAnswersVotes { get; set; }

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

        //modelBuilder.Entity<Team>()
        //    .HasMany(t => t.Players)
        //    .WithOne(p => p.Team)
        //    .HasForeignKey(p => p.TeamId)
        //    .OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<Team>()
        //    .HasMany(t => t.Answers)
        //    .WithOne(a => a.Team) 
        //    .HasForeignKey(a => a.TeamId)
        //    .OnDelete(DeleteBehavior.Cascade);


        //modelBuilder.Entity<TeamAnswer>()
        //    .HasMany(a => a.Votes)
        //    .WithOne(v => v.TeamAnswer)
        //    .HasForeignKey(v => v.TeamAnswerId)
        //    .OnDelete(DeleteBehavior.Cascade);

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
