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
    }

    private struct XmlAttributeNames
    {
        public const string NAME = "name";
        public const string CREF = "cref";
        public const string CODE = "code";
    }

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
        (string namespaceName, _, string typeName, _) = GetNamesAndParams(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;

        var result = new EventMetadata
        {
            Name = typeName,
            FullName = namespaceName,
            Summary = summary
        };

        return result;
    }

    private static PropertyMetadata ReadNodeProperty(XElement member, string name)
    {
        (string namespaceName, _, string propertyName, _) = GetNamesAndParams(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;

        var result = new PropertyMetadata
        {
            Name = propertyName,
            FullName = namespaceName,
            Summary = summary
        };

        return result;
    }

    private static FieldMetadata ReadNodeField(XElement member, string name)
    {
        (string namespaceName, _, string fieldName, _) = GetNamesAndParams(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;

        var result = new FieldMetadata
        {
            Name = fieldName,
            FullName = namespaceName,
            Summary = summary
        };

        return result;
    }

    private static TypeMetadata ReadNodeType(XElement member, string name)
    {
        (string namespaceName, _, string typeName, _) = GetNamesAndParams(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;
        var remarks = member.Element(XmlNodeNames.REMARKS)?.Value;

        var result = new TypeMetadata
        {
            Name = typeName,
            FullName = namespaceName,
            Summary = summary,
            Remarks = remarks
        };

        return result;
    }

    private static MethodMetadata ReadNodeMethod(XElement member, string name)
    {
        (string namespaceName, string className, string methodName, List<string> parameterNames) = GetNamesAndParams(name);

        var summary = member.Element(XmlNodeNames.SUMMARY)?.Value;
        var parameters = member.Elements(XmlNodeNames.PARAM).ToList();
        
        var remarks = member.Element(XmlNodeNames.REMARKS)?.Value;
        var responses = member.Elements(XmlNodeNames.RESPONSE).ToList();
        var returns = member.Element(XmlNodeNames.RETURNS)?.Value;
        var example = member.Element(XmlNodeNames.EXAMPLE)?.Value;
        var exceptions = member.Elements(XmlNodeNames.EXCEPTION).ToList();

        var result = new MethodMetadata
        {
            Name = methodName,
            ClassName = className,
            FullName = namespaceName,
            Summary = summary,
            Parameters = ReadNodeParameters(parameters, parameterNames),
            Example = example,
            Remarks = remarks,
            Returns = returns,
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
            var fullName = exception.Attribute(XmlAttributeNames.CREF)?.Value;
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

    private static List<ParameterMetadata> ReadNodeParameters(List<XElement> parameters, List<string> parameterNames)
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
                Summary = element?.Value
            });

            ix++;
        }

        return result;
    }

    private static (string namespaceName, string className, string methodName, List<string> parameterNames) GetNamesAndParams(string name)
    {
        string fullName = name;
        string className = string.Empty;
        string methodName = string.Empty;
        List<string> parameterNames = new();

        if (string.IsNullOrWhiteSpace(name)) return (fullName, className, methodName, parameterNames);

        fullName = name.Contains('(') && name.Contains(')')
            ? name[2..name.IndexOf('(')]
            : name[2..];

        var splittedName = fullName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (splittedName.Length >= 2)
        {
            methodName = splittedName[^1];
            className = splittedName[^2];
        }
        else if (splittedName.Length == 1)
        {
            methodName = splittedName[0];
        }

        if (!name.Contains('(') || !name.Contains(')'))
        {
            return (fullName, className, methodName, parameterNames);
        }

        parameterNames = name[(name.IndexOf('(') + 1)..name.IndexOf(')')].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        return (fullName, className, methodName, parameterNames);
    }
}
