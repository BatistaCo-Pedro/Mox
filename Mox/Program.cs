﻿using System.Linq.Expressions;
using System.Text;

namespace Mox;

public static class Program
{
    public static void Main(string[] args)
    {
        var f = Expression.Multiply(Expression.Negate(Expression.Constant(123)), Expression.Constant(45));
        Console.Write(f.ToString());
        
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

    public static void Error(int line, int pos, string message)
    {
        _hasError = true;
        Report(line, pos, message);
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

    private static void Report(int line, int pos, string message)
    {
        Console.WriteLine($"[at {line}:{pos}] Error: {message}");
    }
}
