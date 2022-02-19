using NSubstitute;
using POP.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace POP.NSubstitute_Builders
{
    public class GenericBuilder<T> : Builder<T>
        where T : class
    {
        protected override T BuildObject()
        {
            var constructors = typeof(T).GetConstructors();
            MethodInfo? method = typeof(Substitute).GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new Exception();

            foreach (var c in constructors.OrderByDescending(c => c.GetParameters().Count()))
            {
                var createdParams = new List<object>();

                foreach(var p in c.GetParameters())
                {
                    try
                    {
                        var genMethod = method.MakeGenericMethod(p.ParameterType);
                        var inited = genMethod.Invoke(null, null);
                        if (inited != null)
                            createdParams.Add(inited);
                    }
                    catch
                    {
                        createdParams = null;
                        break;
                    }
                }

                if (createdParams != null)
                {
                    try
                    {
                        if (c.Invoke(createdParams.ToArray()) is T output) { return output; }
                        throw new Exception();
                    }
                    catch
                    {
                        // Try next constructor
                    }
                }
            }

            throw new Exception("Unable to instantiate");
        }
    }
}
