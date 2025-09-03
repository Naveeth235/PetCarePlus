using Microsoft.EntityFrameworkCore;

namespace PetCare.Api.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    
    // Add a trivial entity so the first migration has something to create:
    public DbSet<Pet> Pets => Set<Pet>();
}

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Species { get; set; } = default!;
    public DateTime? Dob { get; set; }
    public bool IsActive { get; set; } = true;
}
