namespace SubastaYa.Application.Common.Exceptions;

// Email o contraseña incorrectos. El middleware la traduce a 401.
// El mensaje es deliberadamente vago: no revelamos si el email existe.
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Email o contraseña incorrectos.")
    {
    }
}
