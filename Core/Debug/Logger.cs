namespace ECS.Core.Debug;
public static class Logger
{
    // A list to store all debug messages
    private static readonly List<string> messages = new List<string>();

    // Expose a read-only version of the messages
    public static IReadOnlyList<string> Messages => messages;

    // Log a message and optionally output it to the standard debug console
    public static void Log(string message)
    {
        messages.Add(message);
        System.Diagnostics.Debug.WriteLine(message);
    }

    // Clear the log: this method just clears the in-memory list of messages
    public static void Clear()
    {
        messages.Clear();
    }

    // Save the current log messages to a file.
    // If no file path is provided, a unique file name is generated automatically.
    public static string Save(string fileName = null)
    {
        try
        {
            string currentDir = Directory.GetCurrentDirectory();
            string logDirectory = currentDir + "/log/";
            string fileExt = ".log";
            string filePath;

            // Ensure the log directory exists
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // If no file path is provided, generate a unique file name using the current date and time.
            if (string.IsNullOrEmpty(fileName))
            {
                // Generate a unique file name using date, time, and milliseconds to avoid collisions.
                string uniqueFileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt";
                filePath = logDirectory + uniqueFileName;
            }
            else
            {
               filePath = logDirectory + fileName + fileExt;

               for (int i = 1; ;++i) {
                    if (!File.Exists(filePath))
                        break;

                    filePath = logDirectory + fileName + " " + i + fileExt;
                }
            }

            // Write all messages to the file, each message on a new line
            File.WriteAllLines(filePath, messages);
            return $"<color=yellow>Log saved to</color>: {filePath}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error writing log to file: {ex.Message}");
            return $"<color=lightcoral>Error writing log to file: {ex.Message}</color>";
        }
    }
}