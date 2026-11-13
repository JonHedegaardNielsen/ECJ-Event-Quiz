using ECJ.EventQuiz.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ECJ.EventQuiz.Core.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<ParticipantAnswer> ParticipantAnswers => Set<ParticipantAnswer>();
    public DbSet<QuizSession> QuizSessions => Set<QuizSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(255).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Quiz>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Name).HasMaxLength(255).IsRequired();
            e.HasOne(q => q.Creator)
                .WithMany(u => u.Quizzes)
                .HasForeignKey(q => q.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Text).HasColumnName("Question").HasMaxLength(255).IsRequired();
            e.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerOption>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Answer).HasMaxLength(255).IsRequired();
            e.HasOne(a => a.Question)
                .WithMany(q => q.AnswerOptions)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Participant>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(255).IsRequired();
            // SignalR runtime state, not stored.
            e.Ignore(p => p.ConnectionId);
        });

        modelBuilder.Entity<QuizSession>(e =>
        {
            e.HasKey(s => s.RoomId);
            e.Property(s => s.RoomId).HasMaxLength(255);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

            // Live (in-memory) state used by the SignalR hub, not stored.
            e.Ignore(s => s.CurrentQuestionIndex);
            e.Ignore(s => s.Phase);
            e.Ignore(s => s.Scores);
            e.Ignore(s => s.QuestionStartedAtUtc);
            e.Ignore(s => s.HostConnectionId);

            e.HasOne(s => s.Quiz)
                .WithMany()
                .HasForeignKey(s => s.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Question>()
                .WithMany()
                .HasForeignKey(s => s.CurrentQuestionId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(s => s.Participants)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "QuizSessionParticipants",
                    j => j.HasOne<Participant>().WithMany().HasForeignKey("ParticipantId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<QuizSession>().WithMany().HasForeignKey("RoomId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasKey("RoomId", "ParticipantId"));

            e.HasMany(s => s.Answers)
                .WithOne()
                .HasForeignKey("RoomId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParticipantAnswer>(e =>
        {
            e.HasKey(a => new { a.ParticipantId, a.QuestionId });

            e.HasOne<Participant>()
                .WithMany()
                .HasForeignKey(a => a.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Question>()
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<AnswerOption>()
                .WithMany()
                .HasForeignKey(a => a.AnswerOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
