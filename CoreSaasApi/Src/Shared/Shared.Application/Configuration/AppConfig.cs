namespace Shared.Application.Configuration;

public class AppConfig
{
    public string ApiKey { get; set; } = "";
    public string ApiURL { get; set; } = "";
    public string WebURL { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string FileUploadTarget { get; set; } = "";
    public LifeTime OTPLifeTime { get; set; } = new();
    public LifeTime JWTTokenLifeTime { get; set; } = new();
    public LifeTime PasswordSetTokenLifeTime { get; set; } = new();
    public LifeTime PasswordResetTokenLifeTime { get; set; } = new();
}

public class LifeTime
{
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public LifeTime()
    {
        Day = 0;
        Hour = 0;
        Minute = 0;
    }
}
