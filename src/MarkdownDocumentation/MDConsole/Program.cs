using MDLibrary;

namespace MDConsole;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var endpointsFilePath = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Endpoints.Api.xml";
        var logicFilePath = @"..\\..\\..\\..\\..\\..\\..\\..\\TempFiles\\Endpoints.Logic.xml";

        var result = XmlCsprojReader.Load(endpointsFilePath);
        var result2 = XmlCsprojReader.Load(logicFilePath);

        Console.WriteLine("Bye, World!");
    }
}