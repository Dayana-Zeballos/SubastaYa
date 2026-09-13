using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SubastaYa.Infrastructure.Persistence.Converters;

// El tipo datetime2 de SQL Server no guarda la zona horaria, así que al leer las fechas
// volvían con Kind sin especificar y el JSON salía sin la "Z" del final. El frontend las
// tomaba como hora local y el contador de cierre quedaba corrido según el huso del visitante.
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
