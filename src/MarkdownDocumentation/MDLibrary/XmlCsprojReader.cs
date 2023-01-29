using MDLibrary.Models;
using System;
using System.Text;
using System.Xml.Linq;

namespace MDLibrary;

public static class XmlCsprojReader
{
    private static readonly object _lockFile = new();
    private struct XmlNodeNames
    {
        public const string ASSEMBLY = "assembly";
        public const string MEMBERS = "members";
        public const string MEMBER = "member";

        public const string SUMMARY = "summary";
        public const string PARAM = "param";
        public const string REMARKS = "remarks";
        public const string RESPONSE = "response";
        public const string RETURNS = "returns";
        public const string EXAMPLE = "example";
        public const string EXCEPTION = "exception";
        public const string SEE = "see";
    }

    private struct XmlAttributeNames
    {
        public const string NAME = "name";
        public const string CREF = "cref";
        public const string CODE = "code";
    }

    public const string CONSTRUCTOR_NAME = ".#ctor";

    /// <summary>
    /// The prefix of the name property determines what kind of code element the documentation is for, as follows:
    /// Methods “M:”; Types “T:”; Fields “F:”; Properties “P:”; Constructors “M:”; Events “E:”.
    /// 
    /// The remaining parts of the name property are the fully qualified type and member names,
    /// along with any necessary parameters and/or generic parameters.
    /// Generic parameters from types are represented with a single apostrophe followed by the index “`X,”
    /// while generic parameters from methods are represented with two apostrophes followed by the index “``Y.”
    /// Arrays and unsafe pointers have the same syntax as they have in the source code;
    /// however, multidimensional arrays with a rank greater than one include “0:”
    /// strings separated by commas for each rank.
    ///             
    /// Ref/Out/In parameters are all handled the same with just an at sign (@) appended to the end of the type.
    /// Optional parameters (with default values) don’t have any special formatting.
    /// </summary>
    private struct Prefix
    {
        public const string METHODS = "M:";
        /// <summary>Types (Classes, Structs, Interfaces...)</summary>
        public const string TYPES = "T:";
        public const string FIELDS = "F:";
        public const string PROPERTIES = "P:";
        public const string EVENTS = "E:";
    }

    /// <summary>
    /// Carga un archivo de disco y devuelve todos los elementos que hubiese
    /// </summary>
    /// <typeparam name="T">Tipo a guardar</typeparam>
    /// <param name="filePath">Ruta completa hasta el archivo a leer.</param>
    /// <returns>Los elementos encontrados de tipo T</returns>
    public static List<BaseMetadata> Load(string filePath)
    {
        lock (_lockFile)
        {
            var items = new List<BaseMetadata>();

            try
            {
                if (!File.Exists(filePath)) return items;

                XDocument doc = null;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                {
                    using var sr = new StreamReader(fs, Encoding.Default);
                    doc = XDocument.Load(sr, LoadOptions.SetLineInfo);
                }

                if (doc?.Root == null) return items;

                var assemblyName = doc.Root.Descendants(XmlNodeNames.ASSEMBLY).FirstOrDefault()?.Element(XmlAttributeNames.NAME)?.Value;
                if (string.IsNullOrWhiteSpace(assemblyName)) assemblyName = Path.GetFileNameWithoutExtension(filePath);

                var members = doc.Root.Descendants(XmlNodeNames.MEMBERS).Descendants(XmlNodeNames.MEMBER).ToList();

                foreach (XElement member in members)
                {
                    var name = member.Attribute(XmlAttributeNames.NAME).Value;
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    if (name.StartsWith(Prefix.METHODS))
                    {
                        items.Add(ReadNodeMethod(member, name));
                        continue;
                    }

                    if (name.StartsWith(Prefix.TYPES))
                    {
                        items.Add(ReadNodeType(member, name));
                        continue;
                    }

                    if (name.StartsWith(Prefix.FIELDS))
                    {
                        items.Add(ReadNodeField(member, name));
                        continue;
                    }

                    if (name.StartsWith(Prefix.PROPERTIES))
                    {
                        items.Add(ReadNodeProperty(member, name));
                        continue;
                    }

                    if (name.StartsWith(Prefix.EVENTS))
                    {
                        items.Add(ReadNodeEvent(member, name));
                        continue;
                    }

                    // ?
                    throw new ArgumentException($"Cannot determinate type of {name}", name);
                }

                foreach (var item in items)
                {
                    item.AssemblyName = assemblyName;
                }
            }
            catch (Exception)
            {
                // Rethrow
                throw;
            }

            return items;
        }
    }

    private static EventMetadata ReadNodeEvent(XElement member, string name)
    {
        (string fullClassName, string className, string fullElementName, string elementName) = GetPropertyAndFieldNames(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;

        var result = new EventMetadata
        {
            Name = elementName,
            FullName = fullElementName,
            Summary = summary,
            FullClassName = fullClassName,
            ClassName = className
        };

        return result;
    }

    private static PropertyMetadata ReadNodeProperty(XElement member, string name)
    {
        (string fullClassName, string className, string fullElementName, string elementName) = GetPropertyAndFieldNames(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;
        var cref = GetCRefValue(member.Element(XmlNodeNames.SEE));

        var result = new PropertyMetadata
        {
            Name = elementName,
            FullName = fullElementName,
            Summary = summary,
            TypeName = cref,
            FullClassName = fullClassName,
            ClassName = className
        };

        return result;
    }

    private static FieldMetadata ReadNodeField(XElement member, string name)
    {
        (string fullClassName, string className, string fullElementName, string elementName) = GetPropertyAndFieldNames(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;
        var cref = GetCRefValue(member.Element(XmlNodeNames.SEE));

        var result = new FieldMetadata
        {
            Name = elementName,
            FullName = fullElementName,
            Summary = summary,
            TypeName = cref,
            FullClassName = fullClassName,
            ClassName = className
        };

        return result;
    }

    private static TypeMetadata ReadNodeType(XElement member, string name)
    {
        (_, _, string fullElementName, string elementName) = GetPropertyAndFieldNames(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;
        var remarks = member.Element(XmlNodeNames.REMARKS)?.Value;

        var result = new TypeMetadata
        {
            Name = elementName,
            FullName = fullElementName,
            Summary = summary,
            Remarks = remarks
        };

        return result;
    }

    private static MethodMetadata ReadNodeMethod(XElement member, string name)
    {
        (string fullClassName, string className, string fullElementName, string elementName) = GetPropertyAndFieldNames(name);
        List<string> parameterNames = GetMethodParams(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;
        var parameters = member.Elements(XmlNodeNames.PARAM).ToList();

        var remarks = member.Element(XmlNodeNames.REMARKS)?.Value;
        var responses = member.Elements(XmlNodeNames.RESPONSE).ToList();
        var returns = member.Element(XmlNodeNames.RETURNS);
        var example = member.Element(XmlNodeNames.EXAMPLE)?.Value;
        var exceptions = member.Elements(XmlNodeNames.EXCEPTION).ToList();

        var result = new MethodMetadata
        {
            IsConstructor = name.Contains(CONSTRUCTOR_NAME),
            Name = elementName,
            ClassName = className,
            FullClassName = fullClassName,
            FullName = fullElementName,
            Summary = summary,
            Parameters = ReadNodeMethodParameters(parameters, parameterNames),
            Example = example,
            Remarks = remarks,
            Returns = ReadNodeMethodReturns(returns),
            Exceptions = ReadNodeExceptions(exceptions),
            Responses = ReadNodeResponses(responses)
        };

        return result;
    }

    private static List<ResponseMetadata> ReadNodeResponses(List<XElement> responses)
    {
        var result = new List<ResponseMetadata>();
        foreach (XElement response in responses)
        {
            var codeValid = int.TryParse(response.Attribute(XmlAttributeNames.CODE)?.Value, out int code);
            result.Add(new ResponseMetadata
            {
                Code = codeValid ? code : -1,
                Summary = response.Value
            });
        }
        return result;
    }

    private static List<ExceptionMetadata> ReadNodeExceptions(List<XElement> exceptions)
    {
        var result = new List<ExceptionMetadata>();
        foreach (XElement exception in exceptions)
        {
            var fullName = GetCRefValue(exception);
            var name = fullName?.Split('.')?.LastOrDefault();

            result.Add(new ExceptionMetadata
            {
                Name = name,
                FullName = fullName,
                Summary = exception.Value
            });
        }
        return result;
    }

    private static ReturnMetadata ReadNodeMethodReturns(XElement returns)
    {
        if (returns == null) return null;
        if (string.IsNullOrWhiteSpace(returns.Value) && string.IsNullOrWhiteSpace(GetCRefValue(returns))) return null;
        return new ReturnMetadata
        {
            Summary = returns.Value,
            FullName = GetCRefValue(returns)
        };
    }

    private static List<ParameterMetadata> ReadNodeMethodParameters(List<XElement> parameters, List<string> parameterNames)
    {
        var result = new List<ParameterMetadata>();

        var ix = 0;
        foreach (var typeName in parameterNames)
        {
            // We will assume they will come in order..
            XElement element = parameters.Count > ix ? parameters[ix] : null;

            result.Add(new ParameterMetadata
            {
                TypeName = typeName,
                Name = element?.Attribute(XmlAttributeNames.NAME)?.Value,
                Summary = element?.Value,
                FullName = GetCRefValue(element)
            });

            ix++;
        }

        return result;
    }

    private static string GetCRefValue(XElement element)
    {
        var value = element?.Attribute(XmlAttributeNames.CREF)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value[2..];
    }

    private static (string fullClassName, string className, string fullElementName, string elementName) GetPropertyAndFieldNames(string name)
    {
        string fullClassName = string.Empty;
        string className = string.Empty;
        string fullElementName = name;
        string elementName = string.Empty;

        if (string.IsNullOrWhiteSpace(name)) return (fullClassName, className, fullElementName, elementName);

        fullElementName = (!name.Contains('(') || !name.Contains(')'))
            ? name[2..]
            : name[2..(name.IndexOf('('))];

        var splittedName = fullElementName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (splittedName.Length >= 2)
        {
            elementName = splittedName[^1];
            className = splittedName[^2];
        }
        else if (splittedName.Length == 1)
        {
            elementName = splittedName[0];
        }

        var length = fullElementName.LastIndexOf(elementName) - 1;
        fullClassName = name.StartsWith(Prefix.TYPES) ? fullElementName : fullElementName[0..length];

        return (fullClassName, className, fullElementName, elementName);
    }

    private static List<string> GetMethodParams(string name)
    {
        List<string> parameterNames = new();
        if (string.IsNullOrWhiteSpace(name)) return parameterNames;
        if (!name.Contains('(') || !name.Contains(')')) return parameterNames;

        parameterNames = name[(name.IndexOf('(') + 1)..name.IndexOf(')')].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        return parameterNames;
    }
}
