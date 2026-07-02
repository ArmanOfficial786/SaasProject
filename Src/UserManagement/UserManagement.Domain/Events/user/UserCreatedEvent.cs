namespace UserManagement.Domain.Events.user;

public class UserCreatedEvent : BaseEvent
{
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? ResetPasswordUrl { get; set; }

    public UserCreatedEvent(string? fullName, string? userName, string? email, string? resetPasswordUrl)
    {
        FullName = fullName;
        UserName = userName;
        Email = email;
        ResetPasswordUrl = resetPasswordUrl;
    }
}
