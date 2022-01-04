using IntegrationBase;
using System.Threading.Tasks;

namespace CQRS.Command
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IIOCResolver resolver;

        public CommandExecutor(IIOCResolver resolver)
        {
            this.resolver = resolver;
        }

        public Task Execute(ICommand command)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var handler = (dynamic)resolver.Resolve(handlerType);
            return handler.Execute((dynamic)command);
        }

        public Task<TResult> Execute<TResult>(ICommand<TResult> command)
        {
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            var handler = (dynamic)resolver.Resolve(handlerType);
            return handler.Execute((dynamic)command);
        }
    }
}
