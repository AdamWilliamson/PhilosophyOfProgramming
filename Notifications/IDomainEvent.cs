namespace Notifications
{
    public interface IDomainEvent { }

    public class ItemAddedEvent<T> : IDomainEvent { }
    public class ItemRemovedEvent<T> : IDomainEvent { }
    public class ItemModifiedEvent<T> : IDomainEvent { }
}
