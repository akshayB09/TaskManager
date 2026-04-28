namespace TaskManager.Commands;

public static class ArgParser
{
    /// <summary>
    /// Splits args into positional values and --key value pairs.
    /// e.g. ["abc123", "--title", "Buy milk", "--priority", "High"]
    ///   => positional: ["abc123"]
    ///      named:      { "title": "Buy milk", "priority": "High" }
    /// Bare flags (--all) are stored as { "all": "true" }.
    /// </summary>
    public static (List<string> Positional, Dictionary<string, string> Named) Parse(string[] args)
    {
        var positional = new List<string>();
        var named = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i][2..];
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    named[key] = args[i + 1];
                    i++;
                }
                else
                {
                    named[key] = "true";
                }
            }
            else
            {
                positional.Add(args[i]);
            }
        }

        return (positional, named);
    }
}
