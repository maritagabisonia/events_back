using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace usermangment.Data.Models;

public class ApplicationDbContext : IdentityDbContext<AplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    // ბაზაში ქმნის ივენთების ცხრილს
    public DbSet<Events> Events { get; set; }

    // როლების შექმნის ფუნქციას იძახებს
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeadRoles(builder);
    }

    private static void SeadRoles(ModelBuilder builder)
    {
        builder.Entity<Events>()
                .HasKey(r => r.Id);

        builder.Entity<IdentityRole>().HasData
            (
               new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
               new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
               new IdentityRole() { Name = "Artist", ConcurrencyStamp = "3", NormalizedName = "Artist" }
            );
    }

}