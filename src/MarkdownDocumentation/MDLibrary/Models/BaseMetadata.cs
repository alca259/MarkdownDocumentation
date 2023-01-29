namespace MDLibrary.Models;

public abstract class BaseMetadata
{
    private static int _counter = 1;

    public string Name { get; set; }
    public string Summary { get; set; }
    public string FullName { get; set; }
    public int Order { get; private set; } = _counter++;
    public string AssemblyName { get; set; }
}
