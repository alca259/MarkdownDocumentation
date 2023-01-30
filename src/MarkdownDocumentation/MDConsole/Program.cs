using MDLibrary;
using MDLibrary.Models;

namespace MDConsole;

internal static class Program
{
    private static readonly List<ConsoleOption> _consoleOptions = new()
    {
        new ConsoleOption("i", "input"),
        new ConsoleOption("o", "output")
    };

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        SetArgumentValues(args);

        var inputFolder = _consoleOptions.Single(s => s.CommandFull == "input").Value ?? @".\\Input";
        var outputFolder = _consoleOptions.Single(s => s.CommandFull == "output").Value ?? @".\\Output";

        List<BaseMetadata> result = new();

        if (!Directory.Exists(inputFolder))
            Directory.CreateDirectory(inputFolder);

        var files = Directory.GetFiles(inputFolder, "*.xml", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            result.AddRange(XmlCsprojReader.Load(file));
        }

        var types = MetadataResolver.ResolveDependencies(result);

        var mdGenerator = new MarkdownGenerator(types, outputFolder);
        await mdGenerator.DoIt();

        Console.WriteLine("Bye, World!");
    }

    private static void SetArgumentValues(string[] args)
    {
        if (args == null || args.Length <= 0) return;

        foreach (var option in _consoleOptions)
        {
            SetArgument(args, option);
        }
    }

    private static void SetArgument(string[] args, ConsoleOption consoleOption)
    {
        var value = args.SkipWhile(i => !consoleOption.GetOptions().Contains(i.ToLower().Trim())).Skip(1).Take(1).FirstOrDefault();
        consoleOption.SetValue(value);
    }
}