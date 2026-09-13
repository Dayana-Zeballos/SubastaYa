namespace SubastaYa.Application.Common.Exceptions;

// Una regla de negocio rechazó la operación (saldo insuficiente, incremento menor al
// mínimo, fechas incoherentes). El middleware la traduce a 400.
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
