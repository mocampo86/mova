namespace Mova.Application.Abstractions.Policies;

public interface ICancellationPolicy
{
    bool IsWithinCancellationWindow(DateTime startAt, DateTime now);
}
