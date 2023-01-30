namespace MDLibrary.Models;

public sealed class UriMetadata : BaseMetadata
{
    public string Method { get; set; }

    public override string ToString()
    {
        var method = string.IsNullOrWhiteSpace(Method) ? "UNKNOWN" : Method;
        return $"[{method.ToUpper()}] {Name?.ToLower()?.Trim()}";
    }
}
