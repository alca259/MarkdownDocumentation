using MDLibrary;

namespace MDConsole;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var endpointsFilePath = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Endpoints.Api.xml";
        var logicFilePath = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Endpoints.Logic.xml";
        var outputFolder = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Output";

        var result = XmlCsprojReader.Load(endpointsFilePath);
        result.AddRange(XmlCsprojReader.Load(logicFilePath));
        MetadataResolver.ResolveTypeNames(result);

        var mdGenerator = new MarkdownGenerator(result, outputFolder);
        await mdGenerator.DoIt();

        Console.WriteLine("Bye, World!");
    }
}