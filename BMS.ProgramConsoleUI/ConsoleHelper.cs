using System.Text;

namespace UI;

public static class ConsoleHelper
{
    static ConsoleHelper()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

    public static void Header(string title)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine($"║ {title.PadRight(44)} ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    public static void MenuItem(string key, string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($" [{key}] ");
        Console.ResetColor();
        Console.WriteLine(text);
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Xəta: " + message);
        Console.ResetColor();
    }

    public static void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✔ " + message);
        Console.ResetColor();
    }

    public static void Pause()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\nDavam etmək üçün istənilən düyməni basın...");
        Console.ResetColor();
        Console.ReadKey();
    }
}

