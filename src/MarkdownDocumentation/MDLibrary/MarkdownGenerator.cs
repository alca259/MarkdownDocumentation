using MDLibrary.Extensions;
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
        sb.AppendLine($"# {item.Name}");
        sb.AppendLine($"- **Ruta completa**: {item.FullName}");

        if (!string.IsNullOrWhiteSpace(item.Summary))
            sb.AppendLine($"- **Resumen**: {item.Summary.Trim().TrimSpacesBetweenString()}");

        if (!string.IsNullOrWhiteSpace(item.Remarks))
            sb.AppendLine($"- **Descripción**:{Environment.NewLine}{item.Remarks.Trim().TrimSpacesBetweenString()}");

        DrawProperties(ref sb, item.Properties);
        DrawFields(ref sb, item.Fields);
        DrawEvents(ref sb, item.Events);
        DrawMethods(ref sb, item.Methods);

        if (File.Exists(fileName)) File.Delete(fileName);
        await File.WriteAllTextAsync(fileName, sb.ToString());
    }

    private static void DrawProperties(ref StringBuilder sb, List<PropertyMetadata> elements)
    {
        if (elements.Count <= 0) return;

        sb.AppendLine();
        sb.AppendLine("## Propiedades");
        // Header
        sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
        sb.AppendLine("| ----------- | ----------- | ----------- |");
        foreach (var element in elements)
        {
            sb.AppendLine($"| {element.Name} | {element.Summary?.Trim()?.TrimSpacesBetweenString()} | {(!string.IsNullOrWhiteSpace(element.TypeName) ? element.TypeName.Trim() : string.Empty)} |");
        }
    }

    private static void DrawFields(ref StringBuilder sb, List<FieldMetadata> elements)
    {
        if (elements.Count <= 0) return;

        sb.AppendLine();
        sb.AppendLine("## Campos");
        // Header
        sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
        sb.AppendLine("| ----------- | ----------- | ----------- |");
        foreach (var element in elements)
        {
            sb.AppendLine($"| {element.Name} | {element.Summary?.Trim()?.TrimSpacesBetweenString()} | {(!string.IsNullOrWhiteSpace(element.TypeName) ? element.TypeName.Trim() : string.Empty)} |");
        }
    }

    private static void DrawEvents(ref StringBuilder sb, List<EventMetadata> elements)
    {
        if (elements.Count <= 0) return;

        sb.AppendLine();
        sb.AppendLine("## Eventos");
        // Header
        sb.AppendLine("| Nombre | Resumen |");
        sb.AppendLine("| ----------- | ----------- | ----------- |");
        foreach (var element in elements)
        {
            sb.AppendLine($"| {element.Name} | {element.Summary?.Trim()?.TrimSpacesBetweenString()} |");
        }
    }

    private static void DrawMethods(ref StringBuilder sb, List<MethodMetadata> elements)
    {
        if (elements.Count <= 0) return;

        sb.AppendLine();
        sb.AppendLine("## Métodos");

        #region Drawing templates
        static void DrawEndMethod(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------");
            sb.AppendLine();
        }

        static void DrawUri(StringBuilder sb, UriMetadata uri)
        {
            if (uri == null) return;

            sb.AppendLine();
            sb.AppendLine($"**URI**: `{uri}`");
        }

        static void DrawExample(StringBuilder sb, string example)
        {
            if (string.IsNullOrWhiteSpace(example)) return;

            sb.AppendLine();
            sb.AppendLine("**Ejemplo**:");
            sb.AppendLine($"```{Environment.NewLine}{example.Trim().TrimSpacesBetweenString()}{Environment.NewLine}```");
        }

        static void DrawReturn(StringBuilder sb, ReturnMetadata returns)
        {
            if (returns == null) return;

            sb.AppendLine();
            sb.AppendLine("**Retorna**:");
            sb.AppendLine($"- Resumen: {returns.Summary?.Trim()?.TrimSpacesBetweenString()}");
            sb.AppendLine($"- Tipo de dato: {returns.FullName}");
        }

        static void DrawParameters(StringBuilder sb, List<ParameterMetadata> parameters)
        {
            if (parameters.Count <= 0) return;

            sb.AppendLine();
            sb.AppendLine("**Parámetros**:");
            sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
            sb.AppendLine("| ----------- | ----------- | ----------- |");
            foreach (var par in parameters)
            {
                sb.AppendLine($"| {par.Name} | {par.Summary?.Trim()?.TrimSpacesBetweenString()} | {(!string.IsNullOrWhiteSpace(par.TypeName) ? par.TypeName.Trim() : string.Empty)} |");
            }
        }

        static void DrawExceptions(StringBuilder sb, List<ExceptionMetadata> exceptions)
        {
            if (exceptions.Count <= 0) return;

            sb.AppendLine();
            sb.AppendLine("**Excepciones**:");
            sb.AppendLine("| Nombre | Resumen | Tipo de dato |");
            sb.AppendLine("| ----------- | ----------- | ----------- |");
            foreach (var ex in exceptions)
            {
                sb.AppendLine($"| {ex.Name} | {ex.Summary?.Trim()?.TrimSpacesBetweenString()} | {(!string.IsNullOrWhiteSpace(ex.FullName) ? ex.FullName.Trim() : string.Empty)} |");
            }
        }

        static void DrawResponses(StringBuilder sb, List<ResponseMetadata> responses)
        {
            if (responses.Count <= 0) return;

            sb.AppendLine();
            sb.AppendLine("**Respuestas**:");
            sb.AppendLine("| Código | Resumen |");
            sb.AppendLine("| ----------- | ----------- |");
            foreach (var par in responses)
            {
                sb.AppendLine($"| {par.Code} | {par.Summary?.Trim()?.TrimSpacesBetweenString()} |");
            }
        }
        #endregion

        foreach (var element in elements)
        {
            // Header
            sb.AppendLine($"### {element.Name}");
            if (element.IsConstructor)
                sb.AppendLine("- Constructor");

            if (!string.IsNullOrWhiteSpace(element.Summary))
                sb.AppendLine($"- **Resumen**: {element.Summary.Trim().TrimSpacesBetweenString()}");
            if (!string.IsNullOrWhiteSpace(element.Remarks))
                sb.AppendLine($"- **Descripción**:{Environment.NewLine}{element.Remarks.Trim().TrimSpacesBetweenString()}");

            if (!element.HasDetails)
            {
                DrawEndMethod(sb);
                return;
            }

            sb.AppendLine();
            sb.AppendLine("<details>");

            DrawUri(sb, element.Uri);
            DrawExample(sb, element.Example);
            DrawParameters(sb, element.Parameters);
            DrawExceptions(sb, element.Exceptions);
            DrawResponses(sb, element.Responses);
            DrawReturn(sb, element.Returns);

            sb.AppendLine();
            sb.AppendLine("</details>");

            DrawEndMethod(sb);
        }
    }
}
