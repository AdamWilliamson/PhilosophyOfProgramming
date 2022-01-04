namespace PhilosophyOfProgramming
{
    public interface IStrategy { }
    public interface IStrategyContext { }
    public interface IStrategyOperator { }

    public interface IService { }

    public interface INotificationSystem { }
    public interface IDomainNotificationSystem { }
    public interface IUINotificationSystem { }

    namespace Threading
    {
        public interface ILock { }
        public interface ITask { }
    }

    //namespace Mocking
    //{
    //    public interface IMock { }
    //    public interface IMockingContext : DataAccess.IContext { }
    //}

    namespace DataAccess
    {
        public interface IRepository { }

        
    }

    namespace Messaging
    {
        public interface IMessage { }

        public interface IMessageHandler {}
        public interface IMessagePublisher {}
    }

    namespace Architecture
    {
        namespace DomainDesign
        {
            namespace Exceptions { }

            public interface IEntity { }

            
        }
    }

    namespace UIBasics
    {

        public interface IView { }

        public interface IController { }

        public interface IViewModel { }

        public interface IPresenter { }
    }

    

    namespace TestingArtifacts
    {
        public interface IContextFor<T> { }

        public interface IUIPage { }

        public interface IUIControl { }
    }
}
