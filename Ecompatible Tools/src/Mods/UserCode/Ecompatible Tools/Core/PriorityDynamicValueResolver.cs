using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ecompatible
{
    public interface IValueResolver<T>
    {
        T Resolve(T startingValue, IValueModificationContext context);
    }
    public interface IFloatResolver : IValueResolver<float>
    {
        int ResolveInt(float startingValue, IValueModificationContext context, Rounding rounding = Rounding.RoundDown);
    }
    public enum Rounding
    {
        RoundDown,
        RoundUp
    }
    public interface IPriorityValueResolver<T> : IValueResolver<T>
    {
        void Add(float priority, IValueModifier<T> handler);
        void Remove(float priority, IValueModifier<T> handler);
        IEnumerable<(float, IValueModifier<T>)> Handlers { get; }
        T Resolve(T startingValue, IValueModificationContext context, out AuxillaryInfo<T> auxillaryInfo);
    }
    public static class ResolverExtensions
    {
        public static int ResolveInt(this IValueResolver<float> resolver, float startingValue, IValueModificationContext context, Rounding rounding = Rounding.RoundDown)
        {
            return Round(resolver.Resolve(startingValue, context), rounding);
        }
        public static int ResolveInt(this IPriorityValueResolver<float> resolver, float startingValue, IValueModificationContext context, out AuxillaryInfo<float> auxillaryInfo, Rounding rounding = Rounding.RoundDown)
        {
            return Round(resolver.Resolve(startingValue, context, out auxillaryInfo), rounding);
        }
        
        public static int Round(float value, Rounding rounding)
        {
            return rounding == Rounding.RoundDown ? (int)value : (int)Math.Ceiling(value);
        }
    }
    public class PriorityValueResolver<T> : IPriorityValueResolver<T>
    {
        private IComparer<(float, IValueModifier<T>)> Comparer { get; } = new NumberComparer<(float, IValueModifier<T>)>(x => x.Item1);
        private ImmutableList<(float, IValueModifier<T>)> RequestHandlers { get; set; } = ImmutableList<(float, IValueModifier<T>)>.Empty;
        public IEnumerable<(float, IValueModifier<T>)> Handlers => RequestHandlers;
        public PriorityValueResolver(params (float, IValueModifier<T>)[] requestHandlers)
        {
            RequestHandlers = RequestHandlers.AddRange(requestHandlers);
        }
        public PriorityValueResolver() { }

        public T Resolve(T startingValue, IValueModificationContext context) => Resolve(startingValue, context, out _);
        public T Resolve(T startingValue, IValueModificationContext context, out AuxillaryInfo<T> auxillaryInfo)
        {
            PassThroughHandlers(startingValue, context, out auxillaryInfo);
            return auxillaryInfo.Output;
        }
        protected void PassThroughHandlers(T startingValue, IValueModificationContext context, out AuxillaryInfo<T> auxillaryInfo)
        {
            List<IModificationOutput<T>> steps = new List<IModificationOutput<T>>();
            T previousOutput = startingValue;
            foreach ((_, IValueModifier<T> handler) in RequestHandlers)
            {
                IModificationInput<T> functionInput = new ModificationInput<T>(this, context, previousOutput);
                IModificationOutput<T> functionOutput = handler.ModifyValue(functionInput);
                if (functionOutput != null) previousOutput = functionOutput.Output;
                steps.Add(functionOutput);
            }
            auxillaryInfo = new AuxillaryInfo<T>(steps.ToArray());
        }
        public void Add(float priority, IValueModifier<T> handler)
        {
            RequestHandlers = RequestHandlers.Add((priority, handler)).Sort(Comparer);
        }
        public void Remove(float priority, IValueModifier<T> handler)
        {
            RequestHandlers = RequestHandlers.Remove((priority, handler));
        }
    }
    public class AuxillaryInfo<T>
    {
        public IModificationOutput<T>[] StepOutputs { get; }
        public T Output { get; }

        public AuxillaryInfo(IModificationOutput<T>[] stepOutputs)
        {
            StepOutputs = stepOutputs;
            Output = stepOutputs[^1].Output;
        }
    }
}