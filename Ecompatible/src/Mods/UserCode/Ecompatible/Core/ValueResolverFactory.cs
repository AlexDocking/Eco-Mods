using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecompatible
{
    public static class ValueResolverFactory
    {
        public static IPriorityValueResolver<T, TContext> CreatePriorityResolver<T, TContext>(params (float Priority, IValueModifier<T, TContext> Modifier)[] modifiers) where TContext : IContext
        {
            return new PriorityValueResolver<T, TContext>(modifiers);
        }
    }
}
