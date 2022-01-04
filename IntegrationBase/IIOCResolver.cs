using System;

namespace IntegrationBase
{
    public interface IIOCResolver
    {
        dynamic Resolve(Type t);
    }
}
