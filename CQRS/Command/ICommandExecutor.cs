using System.Threading.Tasks;

namespace CQRS.Command
{
    public interface ICommandExecutor
    {
        Task Execute(ICommand command);
        Task<TResult> Execute<TResult>(ICommand<TResult> command);
    }
}
