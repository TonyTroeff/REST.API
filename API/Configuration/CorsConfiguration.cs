namespace API.Configuration;

public class CorsConfiguration
{
    public const string Section = "CORS";
    
    public IEnumerable<string> AllowedOrigins { get; set; }
    public IEnumerable<string> AllowedHeaders { get; set; }
    public IEnumerable<string> AllowedMethods { get; set; }
    public bool AllowCredentials { get; set; }
}