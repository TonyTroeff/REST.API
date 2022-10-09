namespace API.Cache;

using System.Text;

public record ETag(string Value, bool IsStrong)
{
    private const string WeakETagIdentifier = "W/";
    
    public static ETag Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

        if (value.StartsWith("W/")) return new ETag(value[2..], false);
        return new ETag(value, true);
    }

    public override string ToString()
    {
        var sb = new StringBuilder(capacity: this.Value.Length + 2);
        if (!this.IsStrong) sb.Append(WeakETagIdentifier);
        sb.Append(this.Value);
        return sb.ToString();
    }
}