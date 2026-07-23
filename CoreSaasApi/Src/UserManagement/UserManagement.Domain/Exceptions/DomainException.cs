namespace UserManagement.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message, Exception? inner = null) : base(message, inner) { }
}
