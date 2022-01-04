namespace CQRS.Command
{
    public interface ICommand { }
    public interface ICommand<out T> { }

    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand { }
    public interface ICommandHandler<in TCommand, out TResult>
        where TCommand : ICommand<TResult> { }
}
