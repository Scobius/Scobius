namespace Scobius.Web.Models;

public record RequestResult
{
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}
