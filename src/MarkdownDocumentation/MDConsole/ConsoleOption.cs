namespace MDConsole;

public sealed class ConsoleOption
{
    private const string COMMAND_PREFIX = "-";
    private const string COMMAND_FULL_PREFIX = "--";

    public ConsoleOption(string command, string commandFull, string helpInfo = null)
	{
        Command = command;
        CommandFull = commandFull;
        HelpInfo = helpInfo;
    }

    public string Command { get; }
    public string CommandFull { get; }
    public string HelpInfo { get; }
    public string Value { get; private set; }

    public List<string> GetOptions()
    {
        var result = new List<string>();
        if (!string.IsNullOrWhiteSpace(Command))
            result.Add($"{COMMAND_PREFIX}{Command.Trim().ToLower()}");
        if (!string.IsNullOrWhiteSpace(CommandFull))
            result.Add($"{COMMAND_FULL_PREFIX}{CommandFull.Trim().ToLower()}");
        return result;
    }

    public ConsoleOption SetValue(string value)
    {
        Value = !string.IsNullOrWhiteSpace(value) ? value : null;
        return this;
    }
}
