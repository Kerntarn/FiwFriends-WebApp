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
}