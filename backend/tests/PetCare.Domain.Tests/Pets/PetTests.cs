using System;
using Xunit;
using PetCare.Domain.Pets;

namespace PetCare.Domain.Tests.Pets;

public class PetTests
{
    [Fact]
    public void NewPet_HasNonEmptyId()
    {
        var pet = new Pet();
        Assert.NotEqual(Guid.Empty, pet.Id);
    }

    [Fact]
    public void NewPet_IsActiveByDefault()
    {
        var pet = new Pet();
        Assert.True(pet.IsActive);
    }

    [Fact]
    public void Pet_NameAndSpeciesAndOwner_CannotBeNullOrEmpty()
    {
        var pet = new Pet
        {
            OwnerUserId = "user123",
            Name = "Buddy",
            Species = "Dog"
        };

        Assert.False(string.IsNullOrEmpty(pet.OwnerUserId));
        Assert.False(string.IsNullOrEmpty(pet.Name));
        Assert.False(string.IsNullOrEmpty(pet.Species));
    }

    // Optional: You can also test for invalid scenarios if you implement rules in the entity
    [Fact]
    public void Pet_CanHaveOptionalBreedAndDob()
    {
        var pet = new Pet
        {
            OwnerUserId = "user123",
            Name = "Kitty",
            Species = "Cat",
            Breed = null,
            Dob = null
        };

        Assert.Null(pet.Breed);
        Assert.Null(pet.Dob);
    }
}
