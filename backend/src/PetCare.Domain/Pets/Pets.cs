using System;

namespace PetCare.Domain.Pets;

public class Pet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Owner link (AspNetUsers.Id string)
    public string OwnerUserId { get; set; } = default!;

    // Required
    public string Name { get; set; } = default!;
    public string Species { get; set; } = default!;

    // Optional
    public string? Breed { get; set; }
    public DateTime? Dob { get; set; }

    public bool IsActive { get; set; } = true;
}
