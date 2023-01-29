using MDLibrary.Guards;
using MDLibrary.Models;
using System.Text;

namespace MDLibrary;

public sealed class MarkdownGenerator
{
    public List<BaseMetadata> Items { get; set; }
    public string OutputFolder { get; set; }

    public MarkdownGenerator(List<BaseMetadata> items, string outputFolder)
    {
        Ensure.Argument.NotNull(items, nameof(items));
        Ensure.Argument.NotNullOrEmpty(outputFolder, nameof(outputFolder));
        Items = items.OrderBy(o => o.Order).ToList();
        OutputFolder = outputFolder;
        if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
    }

    public async Task DoIt()
    {
        foreach (var item in Items)
        {
            if (item is PropertyMetadata || item is FieldMetadata || item is MethodMetadata || item is EventMetadata) continue;

            if (item is TypeMetadata t)
            {
                await PageType(t);
                continue;
            }

            await PageDefault(item);
        }
    }

    private async Task PageDefault(BaseMetadata item)
    {
        // TODO: 1
        await Task.Delay(100);
    }

    private async Task PageType(TypeMetadata item)
    {
        var sb = new StringBuilder();
        var fileName = Path.Combine(OutputFolder, item.Name + ".md");

        // Header
        sb.AppendLine($@"# {item.Name} ({item.FullName})");
        sb.AppendLine($@"_{item.Summary}_");
        sb.AppendLine($@"```{item.Remarks}```");

        sb.AppendLine("## Properties");
        foreach (var prop in item.Properties)
        {
            // Header
            sb.AppendLine($@"### {prop.Name} ({prop.FullName})");
            sb.AppendLine($@"_{prop.Summary}_ Type: `{prop.TypeName}`");
        }

        sb.AppendLine("## Fields");
        foreach (var field in item.Fields)
        {
            // Header
            sb.AppendLine($@"### {field.Name} ({field.FullName})");
            sb.AppendLine($@"_{field.Summary}_ Type: `{field.TypeName}`");
        }

        sb.AppendLine("## Events");
        foreach (var @event in item.Events)
        {
            // Header
            sb.AppendLine($@"### {@event.Name} ({@event.FullName})");
            sb.AppendLine($@"_{@event.Summary}_");
        }

        sb.AppendLine("## Methods");
        foreach (var method in item.Methods)
        {
            // Header
            sb.AppendLine($@"### {method.Name} ({method.FullName})");
            sb.AppendLine($@"_{method.Summary}_");
            sb.AppendLine($@"```{method.Remarks}```");
            sb.AppendLine($@"```{method.Example}```");
            sb.AppendLine($@"```{method.Exceptions}```");
            sb.AppendLine($@"```{method.IsConstructor}```");
            sb.AppendLine($@"```{method.Parameters}```");
            sb.AppendLine($@"```{method.Responses}```");
            sb.AppendLine($@"```{method.Returns}```");
        }

        await File.WriteAllTextAsync(fileName, sb.ToString());
    }
}
