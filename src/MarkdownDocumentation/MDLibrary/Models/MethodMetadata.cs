namespace MDLibrary.Models;

public sealed class MethodMetadata : BaseMetadata
{
    public bool IsConstructor { get; set; } = false;
    public string FullClassName { get; set; }
    public string ClassName { get; set; }
    public string Remarks { get; set; }
    public ReturnMetadata Returns { get; set; }
    public string Example { get; set; }
    public List<ExceptionMetadata> Exceptions { get; set; } = new List<ExceptionMetadata>();
    public List<ParameterMetadata> Parameters { get; set; } = new List<ParameterMetadata>();
    public List<ResponseMetadata> Responses { get; set; } = new List<ResponseMetadata>();
}
