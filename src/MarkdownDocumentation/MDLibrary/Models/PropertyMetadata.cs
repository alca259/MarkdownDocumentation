namespace MDLibrary.Models;

public sealed class PropertyMetadata : BaseMetadata
{
    public string TypeName { get; set; }
    public string JsonValue { get; set; }
    public string FullClassName { get; set; }
    public string ClassName { get; set; }
}
