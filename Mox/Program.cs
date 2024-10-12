using System.Text;

namespace Mox;

public static class Program
{
    public static void Main(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: Mox [script]");
                Environment.Exit(0);
                break;
            case 1:
                Mox.RunFile(args[0]);
                break;
            default:
                Mox.RunPrompt();
                break;
        }
    }
}

public static class Mox
{
    private static bool _hasError;

    public static void RunFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Run(Encoding.Default.GetString(bytes));
        if (_hasError)
            Environment.Exit(65);
    }

    public static void RunPrompt()
    {
        while (true)
        {
            var line = Console.ReadLine();
            if (line == null)
                break;
            Run(line);
            _hasError = false;
        }
    }

    public static void Error(int line, string message)
    {
        _hasError = true;
        Report(line, "", message);
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);

        // For now, just print the tokens.
        foreach (var token in scanner.ScanTokens())
        {
            Console.WriteLine($"token: {token}");
        }
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
    }
}
