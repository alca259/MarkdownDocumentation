using MDLibrary;
using MDLibrary.Models;

namespace MDConsole;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var demoPath = @"Api.xml";

        XmlCsprojReader.Load<BaseMetadata>(demoPath);
    }
}