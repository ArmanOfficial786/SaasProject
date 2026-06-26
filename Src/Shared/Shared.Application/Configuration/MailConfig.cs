namespace Shared.Application.Configuration;

public class MailConfig
{
    public string EmailHost { get; set; } = "";
    public int EmailPort { get; set; } = 0;
    public string EmailUserName { get; set; } = "";
    public string EmailPassword { get; set; } = "";
    public string TemplatePath { get; set; } = "";
    public string ClientURL { get; set; } = "";
    public string OfficeURL { get; set; } = "";
    public string ClientNewUserUrl { get; set; } = "";
    public string ClientResetPasswordUrl { get; set; } = "";
    public string OfficeNewUserUrl { get; set; } = "";
    public string OfficeResetPasswordUrl { get; set; } = "";
}
