using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecompatible
{
    public interface IValueResolver
    {
        float Resolve(IValueModificationContext context);
        int ResolveInt(IValueModificationContext context);
    }
    public interface IPriorityValueResolver : IValueResolver
    {
        void Add(float priority, IValueModifier handler);
        void Remove(float priority, IValueModifier handler);
        void Clear();
        IEnumerable<(float, IValueModifier)> Handlers { get; }
    }
    public class PriorityDynamicValueResolver : IPriorityValueResolver
    {
        private readonly object sync = new object();
        private IComparer<(float, IValueModifier)> Comparer { get; } = new NumberComparer<(float, IValueModifier)>(x => x.Item1);
        private ImmutableList<(float, IValueModifier)> RequestHandlers { get; set; } = ImmutableList<(float, IValueModifier)>.Empty;
        public IEnumerable<(float, IValueModifier)> Handlers => RequestHandlers;
        public PriorityDynamicValueResolver(params (float, IValueModifier)[] requestHandlers)
        {
            RequestHandlers = RequestHandlers.AddRange(requestHandlers);
        }
        public PriorityDynamicValueResolver() { }

        public float Resolve(IValueModificationContext context)
        {
            PassThroughHandlers(context);
            return context.FloatValue;
        }
        public int ResolveInt(IValueModificationContext context)
        {
            PassThroughHandlers(context);
            return context.IntValue;
        }
        private void PassThroughHandlers(IValueModificationContext context)
        {
            foreach ((_, IValueModifier handler) in RequestHandlers)
            {
                handler.ModifyValue(context);
            }
        }
        public void Add(float priority, IValueModifier handler)
        {
            RequestHandlers = RequestHandlers.Add((priority, handler)).Sort(Comparer);
        }
        public void Remove(float priority, IValueModifier handler)
        {
            RequestHandlers = RequestHandlers.Remove((priority, handler));
        }
        public void Clear()
        {
            RequestHandlers = RequestHandlers.Clear();
        }
    }
}
