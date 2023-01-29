namespace MDLibrary.Models;

public sealed class TypeMetadata : BaseMetadata
{
    public string Remarks { get; set; }
    public List<MethodMetadata> Methods { get; set; } = new List<MethodMetadata>();
    public List<PropertyMetadata> Properties { get; set; } = new List<PropertyMetadata>();
    public List<FieldMetadata> Fields { get; set; } = new List<FieldMetadata>();
    public List<EventMetadata> Events { get; set; } = new List<EventMetadata>();
}
