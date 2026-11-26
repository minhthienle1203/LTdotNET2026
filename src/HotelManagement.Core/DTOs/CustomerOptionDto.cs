namespace HotelManagement.Core.DTOs;

public class CustomerOptionDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdentityText { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public string Text => $"{FullName} - {Phone}";
}
