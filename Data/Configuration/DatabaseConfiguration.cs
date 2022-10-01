namespace Data.Configuration;

public class DatabaseConfiguration
{
    public const string Section = "Database";

    public string Provider { get; set; }
    public string ConnectionString { get; set; }
}