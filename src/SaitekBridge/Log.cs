namespace SaitekBridge;

public static class Log
{
    public static bool Verbose { get; set; }

    public static void Info(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} {message}");

    public static void Debug(string message)
    {
        if (Verbose) Info(message);
    }
}
