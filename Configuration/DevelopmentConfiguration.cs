namespace ServiceHubMVC.Configuration;

public static class DevelopmentConfiguration
{
    public const string FileName = "appsettings.Development.local.json";

    public static void AddInternalFile(ConfigurationManager configuration, IHostEnvironment environment, string[] args)
    {
        if (!environment.IsDevelopment()) return;

        // The private file overrides old User Secrets. Machine environment and CLI still win.
        // Do not hot-reload credentials halfway through startup/seeding.
        try
        {
            configuration.AddJsonFile(FileName, optional: true, reloadOnChange: false);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException)
        {
            // JSON parser errors may contain secret values; never include the inner exception.
            throw new InvalidOperationException(
                $"Không đọc được {FileName}. Kiểm tra quyền đọc và cú pháp JSON; không gửi nội dung file lên GitHub.");
        }
        configuration.AddEnvironmentVariables();
        configuration.AddCommandLine(args);
    }
}
