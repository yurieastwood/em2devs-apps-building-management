namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record AddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string Country
);
