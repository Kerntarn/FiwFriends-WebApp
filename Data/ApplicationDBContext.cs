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

    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Join> Joins { get; set; }
    public DbSet<Form> Forms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        //1:N User make Posts
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnPosts)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        //M:N Users fav Posts   
        modelBuilder.Entity<Favorite>()
            .HasKey(f => new {f.UserId, f.PostId});

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.FavoritePosts)
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Post)
            .WithMany(p => p.FavoriteBy)
            .HasForeignKey(f => f.PostId);

        //M:N Users join Posts
        modelBuilder.Entity<Join>()
            .HasKey(j => new {j.UserId, j.PostId});

        modelBuilder.Entity<Join>()
            .HasOne(j => j.User)
            .WithMany(u => u.JoinedPosts)
            .HasForeignKey(j => j.UserId);

        modelBuilder.Entity<Join>()
            .HasOne(j => j.Post)
            .WithMany(p => p.Participants)
            .HasForeignKey(j => j.UserId);
        
        //1:1 Post and Form
        modelBuilder.Entity<Form>()
            .HasOne(f => f.Post)
            .WithOne(p => p.Form)
            .HasForeignKey<Form>(f => f.PostId)
            .IsRequired();       
    }   
}