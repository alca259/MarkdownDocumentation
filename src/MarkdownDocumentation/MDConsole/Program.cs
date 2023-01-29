using MDLibrary;
using MDLibrary.Models;

namespace MDConsole;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var inputFolder = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Input";
        var outputFolder = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Output";

        List<BaseMetadata> result = new();

        if (!Directory.Exists(inputFolder)) throw new ApplicationException($"Path: {inputFolder} not found.");

        var files = Directory.GetFiles(inputFolder, "*.xml", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            result.AddRange(XmlCsprojReader.Load(file));
        }

        var types = MetadataResolver.ResolveTypeNames(result);

        var mdGenerator = new MarkdownGenerator(types, outputFolder);
        await mdGenerator.DoIt();

        Console.WriteLine("Bye, World!");
    }
}