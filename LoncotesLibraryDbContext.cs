using Microsoft.EntityFrameworkCore;
using LoncotesLibrary.Models;

public class LoncotesLibraryDbContext : DbContext
{
    public DbSet<Patron> Patrons { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<MaterialType> MaterialTypes { get; set; }
    public DbSet<Checkout> Checkouts { get; set; }

    public LoncotesLibraryDbContext(DbContextOptions<LoncotesLibraryDbContext> context) : base(context)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {
        modelBuilder.Entity<MaterialType>().HasData(new MaterialType[]
        {
            new MaterialType {Id =1, Name = "Book", CheckoutDays = 14},
            new MaterialType {Id = 2, Name = "DVD", CheckoutDays = 5},
            new MaterialType {Id = 3, Name = "Audio Book", CheckoutDays = 7}
        });

        modelBuilder.Entity<Genre>().HasData(new Genre[]
        {
            new Genre {Id = 1, Name = "History"},
            new Genre {Id = 2, Name = "Science Fiction"},
            new Genre {Id = 3, Name = "Romance"},
            new Genre {Id = 4, Name = "Fantasy"},
            new Genre {Id = 5, Name = "Biography"}
        });

        modelBuilder.Entity<Patron>().HasData(new Patron[]
        {
            new Patron {Id = 1, FirstName = "Maus", LastName = "Tiddlywinks", Address = "35 Kitty Cat Claws Ct", Email = "clawcouture@catpress.com", IsActive = true},
            new Patron {Id = 2, FirstName = "Samwise", LastName = "Joubert", Address = "5684 Dog Bone Blvd", Email = "samthewise@doggonit.com", IsActive = true}
        });

        modelBuilder.Entity<Material>().HasData(new Material[]
        {
            new Material {Id = 1, MaterialName = "A Magic Steeped In Poison", MaterialTypeId = 1, GenreId = 4},
            new Material {Id = 2, MaterialName = "Mansfield Park", MaterialTypeId = 3, GenreId = 3},
            new Material {Id = 3, MaterialName = "Gideon the Ninth", MaterialTypeId = 1, GenreId = 2},
            new Material {Id = 4, MaterialName = "History Begins At Sumer", MaterialTypeId = 1, GenreId = 1, OutOfCirculationSince = new DateTime (2020, 2, 15)},
            new Material {Id = 5, MaterialName = "Oppenheimer", MaterialTypeId = 2, GenreId = 5},
            new Material {Id = 6, MaterialName = "The Conquest of Peru", MaterialTypeId = 1, GenreId = 1},
            new Material {Id = 7, MaterialName = "The Princess Bride", MaterialTypeId = 2, GenreId = 3},
            new Material {Id = 8, MaterialName = "A Woman of No Importance", MaterialTypeId = 1, GenreId = 5},
            new Material {Id = 9, MaterialName = "Rasputin: Saint, Satyr, or Satan", MaterialTypeId = 1, GenreId = 5},
            new Material {Id = 10, MaterialName = "Hell Followed With Us", MaterialTypeId = 1, GenreId = 4}
        });
    }
}