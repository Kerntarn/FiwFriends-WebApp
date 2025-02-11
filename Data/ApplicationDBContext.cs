using Microsoft.EntityFrameworkCore;
using FiwFriends.Models;

namespace FiwFriends.Data;
public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; } // Replace `YourEntity` with your actual model class.
    public DbSet<Post> Posts { get; set; }
    public DbSet<Join> Joins { get; set; }
    public DbSet<Form> Forms { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        //1:N User MAKE Posts
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnPosts)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        //M:N Users FAV Posts   
        modelBuilder.Entity<Post>()
            .HasMany(p => p.FavoritedBy)
            .WithMany(u => u.FavoritePosts)
            .UsingEntity<Dictionary<string, object>>(
                "UserFavoritePost",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Post>().WithMany().HasForeignKey("PostId")
            );

        //M:N Users JOIN Posts 
        modelBuilder.Entity<Join>()
            .HasKey(j => new {j.UserId, j.PostId});

        modelBuilder.Entity<Join>()
            .HasOne(j => j.User)
            .WithMany(u => u.JoinedPosts)
            .HasForeignKey(j => j.PostId);

        modelBuilder.Entity<Join>()
            .HasOne(j => j.Post)
            .WithMany(p => p.Participants)
            .HasForeignKey(j => j.UserId);

        //M:N Post and Tag
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "PostTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Post>().WithMany().HasForeignKey("PostId")
            );

        //1:N Post RECEIVE Forms
        modelBuilder.Entity<Form>()
            .HasOne(f => f.Post)
            .WithMany(p => p.Forms)
            .HasForeignKey(f => f.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        //1:N Post ATTACH with Questions
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Post)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        //1:N User SUBMIT Forms
        modelBuilder.Entity<Form>()
            .HasOne(f => f.User)
            .WithMany(u => u.SubmittedForms)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        //N:1 Answer to Question
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        //1:N Form KEEP Answers
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Form)
            .WithMany(f => f.Answers)
            .HasForeignKey(a => a.FormId)
            .OnDelete(DeleteBehavior.Cascade);
    }   
}