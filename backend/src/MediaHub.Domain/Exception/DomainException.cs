namespace MediaHub.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found.") { }
}

public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}