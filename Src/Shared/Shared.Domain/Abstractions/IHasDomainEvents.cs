namespace Shared.Domain.Abstractions;

public interface IHasDomainEvents
{
    IReadOnlyCollection<BaseEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

//why we need it 
//WITHOUT IHasDomainEvents:

//  Shared.Infrastructure.UnitOfWork
//      needs to find entities with domain events
//      only option → .OfType<BaseEntity>()
//      but BaseEntity is in UserManagement.Domain
//      so Shared.Infrastructure → UserManagement.Domain  ❌ circular


//WITH IHasDomainEvents:

//  Shared.Domain
//    └── IHasDomainEvents          ← interface defined here

//  UserManagement.Domain → Shared.Domain
//    └── BaseEntity : IHasDomainEvents   ← implements the interface

//  Shared.Infrastructure → Shared.Domain
//    └── UnitOfWork uses.OfType<IHasDomainEvents>()  ✅ no UserManagement reference needed
