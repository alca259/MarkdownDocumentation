using MDLibrary.Guards;
using MDLibrary.Models;

namespace MDLibrary;

public static class MetadataResolver
{
    public static void ResolveTypeNames(List<BaseMetadata> elements)
    {
        Ensure.Argument.NotNull(elements, nameof(elements));

        foreach (BaseMetadata element in elements)
        {
            if (element is TypeMetadata typeMetadata)
            {
                var props = elements
                    .Where(w => w is PropertyMetadata)
                    .Cast<PropertyMetadata>()
                    .Where(w => w.FullClassName == typeMetadata.FullName)
                    .ToList();

                var fields = elements
                    .Where(w => w is FieldMetadata)
                    .Cast<FieldMetadata>()
                    .Where(w => w.FullClassName == typeMetadata.FullName)
                    .ToList();

                var methods = elements
                    .Where(w => w is MethodMetadata)
                    .Cast<MethodMetadata>()
                    .Where(w => w.FullClassName == typeMetadata.FullName)
                    .ToList();

                var events = elements
                    .Where(w => w is EventMetadata)
                    .Cast<EventMetadata>()
                    .Where(w => w.FullClassName == typeMetadata.FullName)
                    .ToList();

                typeMetadata.Properties.AddRange(props);
                typeMetadata.Fields.AddRange(fields);
                typeMetadata.Methods.AddRange(methods);
                typeMetadata.Events.AddRange(events);
            }
        }
    }
}
