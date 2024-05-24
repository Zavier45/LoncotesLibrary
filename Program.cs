using LoncotesLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using LoncotesLibrary.Models.DTOs;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddNpgsql<LoncotesLibraryDbContext>(builder.Configuration["LoncotesLibraryDbConnectionString"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/materials", (LoncotesLibraryDbContext db, int? genreid, int? materialtypeid) =>
{

    var filteredQuery = db.Materials.Where(m => m.OutOfCirculationSince == null).Include(m => m.Genre).Include(m => m.MaterialType).AsQueryable();

    if (genreid != null)
    {
        filteredQuery = filteredQuery.Where(m => m.GenreId == genreid);
    }
    if (materialtypeid != null)
    {
        filteredQuery = filteredQuery.Where(m => m.MaterialTypeId == materialtypeid);
    }
    var searchResult = filteredQuery.Select(m => new MaterialDTO
    {
        Id = m.Id,
        MaterialName = m.MaterialName,
        MaterialTypeId = m.MaterialTypeId,
        MaterialType = new MaterialTypeDTO
        {
            Id = m.MaterialType.Id,
            Name = m.MaterialType.Name,
            CheckoutDays = m.MaterialType.CheckoutDays
        },
        GenreId = m.GenreId,
        Genre = new GenreDTO
        {
            Id = m.Genre.Id,
            Name = m.Genre.Name
        },
        OutOfCirculationSince = m.OutOfCirculationSince
    }).ToList();

    return Results.Ok(searchResult);
});

app.MapGet("/api/materials/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    return db.Materials
     .Include(m => m.Genre)
     .Include(m => m.MaterialType)
     .Include(m => m.Checkouts)
     .ThenInclude(c => c.Patron)
     .OrderBy(m => m.MaterialName)
     .Select(m => new MaterialDTO
     {
         Id = m.Id,
         MaterialName = m.MaterialName,
         MaterialTypeId = m.MaterialTypeId,
         MaterialType = new MaterialTypeDTO
         {
             Id = m.MaterialType.Id,
             Name = m.MaterialType.Name,
             CheckoutDays = m.MaterialType.CheckoutDays
         },
         GenreId = m.GenreId,
         Genre = new GenreDTO
         {
             Id = m.Genre.Id,
             Name = m.Genre.Name
         },
         OutOfCirculationSince = m.OutOfCirculationSince,
         Checkouts = m.Checkouts.Select(c => new CheckoutDTO
         {
             Id = c.Id,
             PatronId = c.PatronId,
             Patron = new PatronDTO
             {
                 Id = c.Patron.Id,
                 FirstName = c.Patron.FirstName,
                 LastName = c.Patron.LastName,
                 Address = c.Patron.Address,
                 Email = c.Patron.Email
             },
             CheckoutDate = c.CheckoutDate,
             ReturnDate = c.ReturnDate
         }).ToList()

     }).Single(m => m.Id == id); ;
});

app.MapPost("/api/materials", (LoncotesLibraryDbContext db, Material material) =>
{
    db.Materials.Add(material);
    db.SaveChanges();
    return Results.Created($"/api/materials/{material.Id}", material);
});

app.MapDelete("/api/materials/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    Material material = db.Materials.SingleOrDefault(material => material.Id == id);
    if (material == null)
    {
        return Results.NotFound();
    }
    material.OutOfCirculationSince = DateTime.Now;
    db.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/api/materialtypes", (LoncotesLibraryDbContext db) =>
{
    return db.MaterialTypes.Select(mt => new MaterialTypeDTO
    {
        Id = mt.Id,
        Name = mt.Name,
        CheckoutDays = mt.CheckoutDays
    });
});

app.MapGet("/api/genres", (LoncotesLibraryDbContext db) =>
{
    return db.Genres.Select(g => new GenreDTO
    {
        Id = g.Id,
        Name = g.Name
    });
});

app.MapGet("/api/patrons", (LoncotesLibraryDbContext db) =>
{
    return db.Patrons.Select(p => new PatronDTO
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Address = p.Address,
        Email = p.Email
    });
});

app.MapGet("/api/patrons/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    return db.Patrons.Include(p => p.Checkouts)
    .Select(p => new PatronDTO
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Address = p.Address,
        Email = p.Email,
        Checkouts = p.Checkouts.Select(c => new CheckoutDTO
        {
            Id = c.Id,
            MaterialId = c.MaterialId,
            Material = new MaterialDTO
            {
                Id = c.Material.Id,
                MaterialName = c.Material.MaterialName,
                MaterialTypeId = c.Material.MaterialTypeId,
                MaterialType = new MaterialTypeDTO
                {
                    Id = c.Material.MaterialType.Id,
                    Name = c.Material.MaterialType.Name
                }
            },
            CheckoutDate = c.CheckoutDate,
            ReturnDate = c.ReturnDate
        }).ToList()
    }).Single(p => p.Id == id);
});

app.MapPut("/api/patrons.{id}", (LoncotesLibraryDbContext db, int id, Patron patron) =>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(patron => patron.Id == id);
    if (patronToUpdate == null)
    {
        return Results.NotFound();
    }
    patronToUpdate.Address = patron.Address;
    patronToUpdate.Email = patron.Email;
    db.SaveChanges();
    return Results.NoContent();
});

app.MapDelete("/api/patrons/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    Patron patron = db.Patrons.SingleOrDefault(p => p.Id == id);
    if (patron == null)
    {
        return Results.NotFound();
    }
    patron.IsActive = false;
    db.SaveChanges();
    return Results.NoContent();
});

app.MapPost("/api/checkouts", (LoncotesLibraryDbContext db, Checkout checkout) =>
{
    db.Checkouts.Add(checkout);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/api/checkouts/{id}", (LoncotesLibraryDbContext db, int id) =>
{
    Checkout checkin = db.Checkouts.SingleOrDefault(ch => ch.Id == id);
    if (checkin == null)
    {
        return Results.NotFound();
    }
    checkin.ReturnDate = DateTime.Today;
    db.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/api/materials/available", (LoncotesLibraryDbContext db) =>
{
    return db.Materials
    .Where(m => m.OutOfCirculationSince == null)
    .Where(m => m.Checkouts.All(co => co.ReturnDate != null))
    .Select(material => new MaterialDTO
    {
        Id = material.Id,
        MaterialName = material.MaterialName,
        MaterialTypeId = material.MaterialTypeId,
        GenreId = material.GenreId,
        OutOfCirculationSince = material.OutOfCirculationSince
    }).ToList();
});

app.MapGet("/api/checkout/overdue", (LoncotesLibraryDbContext db) =>
{
    return db.Checkouts
    .Include(p => p.Patron)
    .Include(co => co.Material)
    .ThenInclude(m => m.MaterialType)
    .Where(co =>
    (DateTime.Today - co.CheckoutDate).Days > co.Material.MaterialType.CheckoutDays && co.ReturnDate == null)
    .Select(co => new CheckoutDTO
    {
        Id = co.Id,
        MaterialId = co.MaterialId,
        Material = new MaterialDTO
        {
            Id = co.Material.Id,
            MaterialName = co.Material.MaterialName,
            MaterialTypeId = co.Material.MaterialTypeId,
            MaterialType = new MaterialTypeDTO
            {
                Id = co.Material.MaterialTypeId,
                Name = co.Material.MaterialType.Name,
                CheckoutDays = co.Material.MaterialType.CheckoutDays
            },
            GenreId = co.Material.GenreId,
            OutOfCirculationSince = co.Material.OutOfCirculationSince
        },
        PatronId = co.PatronId,
        Patron = new PatronDTO
        {
            Id = co.Patron.Id,
            FirstName = co.Patron.FirstName,
            LastName = co.Patron.LastName,
            Address = co.Patron.Address,
            Email = co.Patron.Email,
            IsActive = co.Patron.IsActive
        },
        CheckoutDate = co.CheckoutDate,
        ReturnDate = co.ReturnDate
    })
    .ToList();
});
app.Run();


