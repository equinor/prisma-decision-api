namespace PrismaApi.Application.Exceptions;

public class NotFoundException : Exception
{
    public int StatusCode { get; } = 404;
    public NotFoundException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}            