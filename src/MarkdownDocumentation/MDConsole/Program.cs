using MDLibrary;
using MDLibrary.Models;

namespace MDConsole;

internal static class Program
{
    private static readonly List<ConsoleOption> _consoleOptions = new()
    {
        new ConsoleOption("i", "input", $"Puedes especificar la ruta relativa o absoluta de ficheros XML de entrada.{Environment.NewLine}\t\tEstos deben ser los generados de documentación por el compilador.{Environment.NewLine}\t\tPor defecto .\\Input"),
        new ConsoleOption("o", "output", $"Puedes especificar la ruta relativa o absoluta de la salida de ficheros Markdown.{Environment.NewLine}\t\tPor defecto: .\\Output")
    };

    private static async Task Main(string[] args)
    {
        ShowHelp();
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

        Console.WriteLine($"Documentación generada en {new DirectoryInfo(outputFolder).FullName}");
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

    private static void ShowHelp()
    {
        Console.WriteLine("### Ayuda ###");
        Console.WriteLine("Puedes utilizar las siguientes opciones para configurar esta aplicación:");
        foreach (var option in _consoleOptions)
        {
            Console.WriteLine($"\t- Opción => {string.Join(", ", option.GetOptions())}");
            if (!string.IsNullOrWhiteSpace(option.HelpInfo)) Console.WriteLine($"\t\t{option.HelpInfo}");
        }
    }
}