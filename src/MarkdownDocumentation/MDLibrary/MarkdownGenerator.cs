using MDLibrary.Guards;
using MDLibrary.Models;
using System.Text;

namespace MDLibrary;

public sealed class MarkdownGenerator
{
    public List<TypeMetadata> Items { get; set; }
    public string OutputFolder { get; set; }

    public MarkdownGenerator(List<TypeMetadata> items, string outputFolder)
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
            await PageType(item);
        }
    }

    private async Task PageType(TypeMetadata item)
    {
        var sb = new StringBuilder();

        var path = Path.Combine(OutputFolder, item.AssemblyName);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        var fileName = Path.Combine(path, item.Name + ".md");

        // Header
        sb.AppendLine($@"# {item.Name}");
        sb.AppendLine($@"- **Ruta completa**: {item.FullName}");

        if (!string.IsNullOrWhiteSpace(item.Summary))
            sb.AppendLine($@"- **Resumen**: {item.Summary.Trim()}");

        if (!string.IsNullOrWhiteSpace(item.Remarks))
            sb.AppendLine($@"- **Descripción**:{Environment.NewLine}{item.Remarks.Trim()}");

        sb.Append(GetTableOfContents(item));

        if (item.Properties.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Propiedades");
            // Header
            sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
            sb.AppendLine("| ----------- | ----------- | ----------- |");
            foreach (var prop in item.Properties)
            {
                sb.AppendLine($"| {prop.Name} | {prop.Summary?.Trim()} | {(!string.IsNullOrWhiteSpace(prop.TypeName) ? prop.TypeName.Trim() : string.Empty)} |");
            }
        }

        if (item.Fields.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Campos");
            // Header
            sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
            sb.AppendLine("| ----------- | ----------- | ----------- |");
            foreach (var field in item.Fields)
            {
                sb.AppendLine($"| {field.Name} | {field.Summary?.Trim()} | {(!string.IsNullOrWhiteSpace(field.TypeName) ? field.TypeName.Trim() : string.Empty)} |");
            }
        }

        if (item.Events.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Eventos");
            // Header
            sb.AppendLine("| Nombre | Resumen |");
            sb.AppendLine("| ----------- | ----------- | ----------- |");
            foreach (var @event in item.Events)
            {
                sb.AppendLine($"| {@event.Name} | {@event.Summary?.Trim()} |");
            }
        }

        if (item.Methods.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Métodos");

            foreach (var method in item.Methods)
            {
                // Header
                sb.AppendLine($@"### {method.Name}");
                if (method.IsConstructor)
                    sb.AppendLine("- Constructor");

                if (!string.IsNullOrWhiteSpace(method.Summary))
                    sb.AppendLine($@"- **Resumen**: {method.Summary.Trim()}");
                if (!string.IsNullOrWhiteSpace(method.Remarks))
                    sb.AppendLine($@"- **Descripción**:{Environment.NewLine}{method.Remarks.Trim()}");

                if (!string.IsNullOrWhiteSpace(method.Example))
                {
                    sb.AppendLine();
                    sb.AppendLine("**Ejemplo**:");
                    sb.AppendLine($@"
```
{method.Example.Trim()}
```");
                }

                if (method.Parameters.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("**Parámetros**:");
                    sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
                    sb.AppendLine("| ----------- | ----------- | ----------- |");
                    foreach (var par in method.Parameters)
                    {
                        sb.AppendLine($"| {par.Name} | {par.Summary?.Trim()} | {(!string.IsNullOrWhiteSpace(par.TypeName) ? par.TypeName.Trim() : string.Empty)} |");
                    }
                }

                if (method.Exceptions.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("**Excepciones**:");
                    sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
                    sb.AppendLine("| ----------- | ----------- | ----------- |");
                    foreach (var ex in method.Exceptions)
                    {
                        sb.AppendLine($"| {ex.Name} | {ex.Summary?.Trim()} | {(!string.IsNullOrWhiteSpace(ex.FullName) ? ex.FullName.Trim() : string.Empty)} |");
                    }
                }

                if (method.Responses.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("**Respuestas**:");
                    sb.AppendLine("| Código | Resumen |");
                    sb.AppendLine("| ----------- | ----------- |");
                    foreach (var par in method.Responses)
                    {
                        sb.AppendLine($"| {par.Code} | {par.Summary?.Trim()} |");
                    }
                }

                if (method.Returns != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("**Retorna**:");
                    sb.AppendLine($"- Resumen: {method.Returns.Summary?.Trim()}");
                    sb.AppendLine($"- Tipo de dato: {method.Returns.FullName}");
                }

                sb.AppendLine();
                sb.AppendLine("-------------------------------------------------------");
                sb.AppendLine();
            }
        }

        if (File.Exists(fileName)) File.Delete(fileName);
        await File.WriteAllTextAsync(fileName, sb.ToString());
    }

    private static StringBuilder GetTableOfContents(TypeMetadata item)
    {
        var sb = new StringBuilder();

        if (item.Properties.Count <= 0 && item.Fields.Count <= 0 && item.Events.Count <= 0 && item.Methods.Count <= 0)
            return sb;

        sb.AppendLine("## Tabla de contenido:");

        if (item.Properties.Count > 0)
        {
            sb.AppendLine("- Propiedades");
            foreach (var prop in item.Properties)
            {
                sb.AppendLine($"    - {prop.Name}{(!string.IsNullOrWhiteSpace(prop.Summary) ? $" `{prop.Summary.Trim()}`" : string.Empty)}");
            }
        }

        if (item.Fields.Count > 0)
        {
            sb.AppendLine("- Campos");
            foreach (var field in item.Fields)
            {
                sb.AppendLine($"    - {field.Name}{(!string.IsNullOrWhiteSpace(field.Summary) ? $" `{field.Summary.Trim()}`" : string.Empty)}");
            }
        }

        if (item.Events.Count > 0)
        {
            sb.AppendLine("- Eventos");
            foreach (var @event in item.Events)
            {
                sb.AppendLine($"    - {@event.Name}{(!string.IsNullOrWhiteSpace(@event.Summary) ? $" `{@event.Summary.Trim()}`" : string.Empty)}");
            }
        }

        if (item.Methods.Count > 0)
        {
            sb.AppendLine("- Métodos");
            foreach (var method in item.Methods)
            {
                // Header
                sb.AppendLine($@"    - {method.Name}{(!string.IsNullOrWhiteSpace(method.Summary) ? $" `{method.Summary.Trim()}`" : string.Empty)}");
            }
        }

        return sb;
    }
}
