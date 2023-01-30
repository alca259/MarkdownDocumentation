using System.Text.RegularExpressions;

namespace MDLibrary.Extensions;

public static class StringExtensions
{
    private static readonly Regex _removeBreakLineRegex = new(@"\r\n?|\n", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(3));
    private static readonly Regex _removeWhiteSpacesRegex = new(@"[ ]{2,}", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(3));

    /// <summary>
    /// Elimina los espacios entre palabras y los limita a uno
    /// </summary>
    /// <param name="line"></param>
    /// <seealso cref="https://github.com/alca259/Bya-Extensions/blob/main/src/Bya.System/StringExtensions.cs"/>
    /// <returns></returns>
    public static string TrimSpacesBetweenString(this string line)
    {
        if (string.IsNullOrEmpty(line)) return line;
        line = _removeWhiteSpacesRegex.Replace(line, @" ").Trim();
        return line;
    }

    /// <summary>
    /// Elimina todos los saltos de líneas existentes en el string
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static string RemoveAllLineBreak(this string line)
    {
        if (string.IsNullOrEmpty(line)) return line;
        line = _removeBreakLineRegex.Replace(line, @" ");
        return line;
    }
}
