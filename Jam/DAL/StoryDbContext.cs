using Microsoft.EntityFrameworkCore;
using Jam.Models; 

namespace Jam.DAL;

public class StoryDbContext : DbContext
{
    public StoryDbContext(DbContextOptions<StoryDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    public DbSet<Story> Stories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Scene> Scenes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<PlayingSession> PlayingSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================================================
        // 1. USER RELATIONSHIPS (Preserve content, break link by setting FK to NULL)
        //    Requires Story.UserId and PlayingSession.UserId to be nullable (int?)
        // ==========================================================================

        // User -> Story: If a User is deleted, their Stories remain, but the Story.UserId is set to NULL
        modelBuilder.Entity<Story>()
            .HasOne(s => s.User)
            .WithMany(u => u.Stories)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // User -> PlayingSession: If a User is deleted, the sessions remain, but PlayingSession.UserId is set to NULL
        modelBuilder.Entity<PlayingSession>()
            .HasOne(ps => ps.User)
            .WithMany(u => u.PlayingSessions)
            .HasForeignKey(ps => ps.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ==========================================================================
        // 2. STORY STRUCTURE RELATIONSHIPS (Cascade Delete for Dependent Content)
        //    If the principal entity is deleted, all dependent entities are deleted
        // ==========================================================================

        // Story -> Scene: Deleting a Story automatically deletes all its Scenes
        modelBuilder.Entity<Story>()
            .HasMany(s => s.Scenes)
            .WithOne(sc => sc.Story)
            .HasForeignKey(sc => sc.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Story -> PlayingSession: Deleting a Story automatically deletes all associated session records (as they are now meaningless)
        modelBuilder.Entity<Story>()
            .HasMany(s => s.PlayingSessions)
            .WithOne(ps => ps.Story)
            .HasForeignKey(ps => ps.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Scene -> Question (One-to-One/Zero Relationship):
        // Deleting a Scene deletes its Question (if it has one)
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Scene)
            .WithOne(sc => sc.Question)
            .HasForeignKey<Question>(q => q.SceneId)
            .OnDelete(DeleteBehavior.Cascade);

        // Question -> Answer_option (One-to-Many Relationship): 
        // Deleting a Question automatically deletes ALL its associated AnswerOptions
        modelBuilder.Entity<Question>()
            .HasMany(q => q.AnswerOptions)
            .WithOne(ao => ao.Question)
            .HasForeignKey(ao => ao.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================================================
        // 3. SCENE NAVIGATION RELATIONSHIPS (Restrict Deletion to Preserve Integrity)
        //    A Scene can reference another Scene as its "NextScene"
        //    We use DeleteBehavior.Restrict to prevent circular cascade deletes
        //    If a Scene being referenced is deleted, the referencing Scene's 
        //    NextSceneId must be manually set to NULL beforehand
        // ==========================================================================

        // Scene -> NextScene (Self-referencing One-to-One/Zero Relationship):
        // Do not cascade delete on self-referenced relationship to avoid circular cascade delete issues
        modelBuilder.Entity<Scene>()
            .HasOne(s => s.NextScene)
            .WithMany() // no inverse navigation property
            .HasForeignKey(s => s.NextSceneId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enforce that NextSceneId is unique across all Scenes
        modelBuilder.Entity<Scene>()
            .HasIndex(s => s.NextSceneId)
            .IsUnique();

    }
}