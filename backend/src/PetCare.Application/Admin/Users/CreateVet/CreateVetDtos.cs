namespace PetCare.Application.Admin.Users.CreateVet;

public sealed class CreateVetRequest
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class CreateVetResponse
{
    public string Id { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = "Vet";
    public string Message { get; set; } = "Vet account created.";
}
