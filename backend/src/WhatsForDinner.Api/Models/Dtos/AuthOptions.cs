namespace WhatsForDinner.Api.Models.Dtos;

public class AuthOptions
{
    public GoogleAuthOptions Google { get; set; } = new();
    public JwtAuthOptions Jwt { get; set; } = new();
}

public class GoogleAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
}

public class JwtAuthOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "whatsfordinner-api";
    public string Audience { get; set; } = "whatsfordinner-spa";
    public int ExpiryDays { get; set; } = 30;
}