using System;
using Autofac;
using IntegrationBase;

namespace CQRS_Autofac
{
    public class IOCResolver : IIOCResolver
    {
        ILifetimeScope scope;

        public IOCResolver(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public dynamic Resolve(Type t)
        {
            return scope.Resolve(t);
        }
    }
}
