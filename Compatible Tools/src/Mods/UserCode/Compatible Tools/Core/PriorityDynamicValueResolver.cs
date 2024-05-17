using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CompatibleTools
{
    public interface IValueResolver
    {
        float Resolve(IModifyValueInPlaceContext context);
        int ResolveInt(IModifyValueInPlaceContext context);
    }
    public interface IPriorityValueResolver : IValueResolver
    {
        void Add(float priority, IModifyValueInPlaceHandler handler);
        void Remove(float priority, IModifyValueInPlaceHandler handler);
        void Clear();
        IEnumerable<(float, IModifyValueInPlaceHandler)> Handlers { get; }
    }
    public class PriorityDynamicValueResolver : IPriorityValueResolver
    {
        private readonly object sync = new object();
        private IComparer<(float, IModifyValueInPlaceHandler)> Comparer { get; } = new NumberComparer<(float, IModifyValueInPlaceHandler)>(x => x.Item1);
        private ImmutableList<(float, IModifyValueInPlaceHandler)> RequestHandlers { get; set; } = ImmutableList<(float, IModifyValueInPlaceHandler)>.Empty;
        public IEnumerable<(float, IModifyValueInPlaceHandler)> Handlers => RequestHandlers;
        public PriorityDynamicValueResolver(params (float, IModifyValueInPlaceHandler)[] requestHandlers)
        {
            RequestHandlers = RequestHandlers.AddRange(requestHandlers);
        }
        public PriorityDynamicValueResolver() { }

        public float Resolve(IModifyValueInPlaceContext context)
        {
            PassThroughHandlers(context);
            return context.FloatValue;
        }
        public int ResolveInt(IModifyValueInPlaceContext context)
        {
            PassThroughHandlers(context);
            return context.IntValue;
        }
        private void PassThroughHandlers(IModifyValueInPlaceContext context)
        {
            foreach ((_, IModifyValueInPlaceHandler handler) in RequestHandlers)
            {
                handler.ModifyValue(context);
            }
        }
        public void Add(float priority, IModifyValueInPlaceHandler handler)
        {
            Log.WriteLine(Localizer.Do($"Before:{RequestHandlers.Select(x => $"({x.Item1},{x.Item2.GetType()})").NewlineList()}"));
            RequestHandlers = RequestHandlers.Add((priority, handler)).Sort(Comparer);
            Log.WriteLine(Localizer.Do($"After:{RequestHandlers.Select(x => $"({x.Item1},{x.Item2.GetType()})").NewlineList()}"));

        }
        public void Remove(float priority, IModifyValueInPlaceHandler handler)
        {
            RequestHandlers = RequestHandlers.Remove((priority, handler));
        }
        public void Clear()
        {
            RequestHandlers = RequestHandlers.Clear();
        }
    }
}
