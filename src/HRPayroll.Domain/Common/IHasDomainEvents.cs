using MediatR;

namespace HRPayroll.Domain.Common;

public interface IHasDomainEvents
{
    IReadOnlyCollection<INotification> DomainEvents { get; }
    void ClearDomainEvents();
}
