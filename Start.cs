global using static Global;

public static class Global
{
    public static string StartWorkingDirectory = null;

    public static void StartAsync(string command, Action<string> callback = null) => Task.Run(() => Start(command, callback));

    public static void Start(string command, Action<string> callback = null)
    {
        callback ??= Console.WriteLine;

        var startInfo = new ProcessStartInfo
        {
            FileName               = "cmd.exe",
            Arguments              = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
            WorkingDirectory       = StartWorkingDirectory
        };

        using var process = new Process();

        process.StartInfo = startInfo;

        process.OutputDataReceived  += (sender, args) => callback?.Invoke(args.Data);
        process.ErrorDataReceived   += (sender, args) => callback?.Invoke(args.Data);
        process.EnableRaisingEvents =  true;

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }
}