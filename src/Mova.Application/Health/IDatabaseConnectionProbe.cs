namespace Mova.Application.Health;

public interface IDatabaseConnectionProbe
{
    Task<bool> IsConnectionHealthyAsync(CancellationToken cancellationToken = default);
}
