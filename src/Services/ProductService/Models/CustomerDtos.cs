namespace ProductService.Models;

public record CustomerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Cpf { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public Guid? DefaultAddressId { get; init; }
    public CustomerAddressDto? DefaultAddress { get; init; }
    public List<CustomerAddressDto> Addresses { get; init; } = new();
    public Dictionary<string, object>? Preferences { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateCustomerDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Cpf { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public Dictionary<string, object>? Preferences { get; init; }
}

public record UpdateCustomerDto
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Cpf { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsVerified { get; init; }
    public Guid? DefaultAddressId { get; init; }
    public Dictionary<string, object>? Preferences { get; init; }
}

public record CustomerAddressDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string? Neighborhood { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Reference { get; init; }
    public string? Label { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateCustomerAddressDto
{
    public Guid CustomerId { get; init; }
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string? Neighborhood { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Reference { get; init; }
    public string? Label { get; init; }
    public bool IsDefault { get; init; } = false;
}

public record UpdateCustomerAddressDto
{
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Neighborhood { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Reference { get; init; }
    public string? Label { get; init; }
    public bool? IsDefault { get; init; }
    public bool? IsActive { get; init; }
}

public record StoreInfoDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Cnpj { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string? Neighborhood { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public string? LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public TimeSpan OpeningTime { get; init; }
    public TimeSpan ClosingTime { get; init; }
    public bool IsOpenOnWeekends { get; init; }
    public bool IsOpenOnHolidays { get; init; }
    public decimal DeliveryBasePrice { get; init; }
    public decimal DeliveryPricePerKm { get; init; }
    public decimal FreeDeliveryThreshold { get; init; }
    public decimal MaxDeliveryDistance { get; init; }
    public int EstimatedDeliveryTimeMinutes { get; init; }
}

public record DeliveryCalculationDto
{
    public bool IsDeliveryAvailable { get; init; }
    public decimal Distance { get; init; } // Em KM
    public decimal DeliveryFee { get; init; }
    public int EstimatedDeliveryTimeMinutes { get; init; }
    public decimal FreeDeliveryThreshold { get; init; }
    public decimal MaxDeliveryDistance { get; init; }
    public decimal BasePrice { get; init; }
    public decimal PricePerKm { get; init; }
} 