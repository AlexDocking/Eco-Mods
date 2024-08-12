using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecompatible
{
    public static class ValueResolverFactory
    {
        public static IPriorityValueResolver<T> CreatePriorityResolver<T>(params (float Priority, IValueModifier<T> Modifier)[] modifiers) => new PriorityValueResolver<T>(modifiers);
    }
}
