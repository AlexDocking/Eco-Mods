using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecompatible
{
    public interface IValueResolver<T>
    {
        T Resolve(T startingValue, IValueModificationContext context);
    }
    public enum Rounding
    {
        RoundDown,
        RoundUp
    }
    public interface ISequentialValueResolver<T> : IValueResolver<T>
    {
        T Resolve(T startingValue, IValueModificationContext context, out ResolvedSequence<T> resolvedSequence);
        ResolvedSequence<T> ResolveSequence(T startingValue, IValueModificationContext context);
    }
    public interface IPriorityValueResolver<T> : ISequentialValueResolver<T>
    {
        IEnumerable<IValueModifier<T>> Modifiers { get; }
        void Add(float priority, IValueModifier<T> modifier);
    }
    public static class ResolverExtensions
    {
        public static int ResolveInt(this IValueResolver<float> resolver, float startingValue, IValueModificationContext context, Rounding rounding = Rounding.RoundDown)
        {
            return Round(resolver.Resolve(startingValue, context), rounding);
        }
        public static int ResolveInt(this ISequentialValueResolver<float> resolver, float startingValue, IValueModificationContext context, out ResolvedSequence<float> resolvedSequence, Rounding rounding = Rounding.RoundDown)
        {
            return Round(resolver.Resolve(startingValue, context, out resolvedSequence), rounding);
        }
        
        public static int Round(float value, Rounding rounding)
        {
            return rounding == Rounding.RoundDown ? (int)value : (int)Math.Ceiling(value);
        }
    }
    public class SequentialValueResolver<T> : ISequentialValueResolver<T>
    {
        public SequentialValueResolver(IEnumerable<IValueModifier<T>> modifiers)
        {
            Modifiers = modifiers;
        }

        private IEnumerable<IValueModifier<T>> Modifiers { get; }

        public T Resolve(T startingValue, IValueModificationContext context) => Resolve(startingValue, context, out _);
        public T Resolve(T startingValue, IValueModificationContext context, out ResolvedSequence<T> resolvedSequence)
        {
            PassThrougModifiers(startingValue, context, out resolvedSequence);
            return resolvedSequence.Output;
        }

        public ResolvedSequence<T> ResolveSequence(T startingValue, IValueModificationContext context)
        {
            Resolve(startingValue, context, out ResolvedSequence<T> sequentiallyResolvedOutput);
            return sequentiallyResolvedOutput;
        }

        protected void PassThrougModifiers(T startingValue, IValueModificationContext context, out ResolvedSequence<T> resolvedSequence)
        {
            List<IModificationOutput<T>> steps = new List<IModificationOutput<T>>();
            T previousOutput = startingValue;
            foreach (IValueModifier<T> modifier in Modifiers)
            {
                IModificationInput<T> functionInput = new ModificationInput<T>(this, context, previousOutput);
                IModificationOutput<T> functionOutput = modifier.ModifyValue(functionInput);
                if (functionOutput != null) previousOutput = functionOutput.Output;
                steps.Add(functionOutput);
            }
            resolvedSequence = new ResolvedSequence<T>(steps.ToArray());
        }
    }
    public class PriorityValueResolver<T> : IPriorityValueResolver<T>
    {
        private ImmutableList<(float, IValueModifier<T>)> requestHandlers = ImmutableList<(float, IValueModifier<T>)>.Empty;

        private IComparer<(float, IValueModifier<T>)> Comparer { get; } = new NumberComparer<(float, IValueModifier<T>)>(x => x.Item1);
        private ImmutableList<(float, IValueModifier<T>)> RequestHandlers
        {
            get => requestHandlers; set
            {
                requestHandlers = value;
                SequentialValueResolver = new SequentialValueResolver<T>(value.Select(pair => pair.Item2));
            }
        }
        public IEnumerable<IValueModifier<T>> Modifiers => RequestHandlers.Select(pair => pair.Item2);
        private ISequentialValueResolver<T> SequentialValueResolver { get; set; } = new SequentialValueResolver<T>(Enumerable.Empty<IValueModifier<T>>());
        public PriorityValueResolver(params (float, IValueModifier<T>)[] requestHandlers)
        {
            RequestHandlers = RequestHandlers.AddRange(requestHandlers);
        }
        public PriorityValueResolver() { }

        public void Add(float priority, IValueModifier<T> modifier)
        {
            RequestHandlers = RequestHandlers.Add((priority, modifier)).Sort(Comparer);
        }
        public void Remove(float priority, IValueModifier<T> modifier)
        {
            RequestHandlers = RequestHandlers.Remove((priority, modifier));
        }

        public T Resolve(T startingValue, IValueModificationContext context, out ResolvedSequence<T> resolvedSequence)
        {
            return SequentialValueResolver.Resolve(startingValue, context, out resolvedSequence);
        }

        public T Resolve(T startingValue, IValueModificationContext context)
        {
            return SequentialValueResolver.Resolve(startingValue, context);
        }

        public ResolvedSequence<T> ResolveSequence(T startingValue, IValueModificationContext context) => SequentialValueResolver.ResolveSequence(startingValue, context);
    }
    public class ResolvedSequence<T>
    {
        public IModificationOutput<T>[] StepOutputs { get; }
        public T Output { get; }

        public ResolvedSequence(IModificationOutput<T>[] stepOutputs)
        {
            StepOutputs = stepOutputs;
            Output = stepOutputs[^1].Output;
        }
    }
}