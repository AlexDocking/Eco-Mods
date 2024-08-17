using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecompatible
{
    public interface IValueResolver<T>
    {
        T Resolve(T startingValue, IContext context);
    }
    public enum Rounding
    {
        RoundDown,
        RoundUp
    }
    public interface ISequentialValueResolver<T> : IValueResolver<T>
    {
        T Resolve(T startingValue, IContext context, out IResolvedSequence<T> resolvedSequence);
        IResolvedSequence<T> ResolveSequence(T startingValue, IContext context);
    }
    public interface IPriorityValueResolver<T> : ISequentialValueResolver<T>
    {
        IEnumerable<IValueModifier<T>> Modifiers { get; }
        void Add(float priority, IValueModifier<T> modifier);
    }
    public static class ResolverExtensions
    {
        public static int ResolveInt(this IValueResolver<float> resolver, float startingValue, IContext context, Rounding rounding = Rounding.RoundDown)
        {
            return Round(resolver.Resolve(startingValue, context), rounding);
        }
        public static int ResolveInt(this ISequentialValueResolver<float> resolver, float startingValue, IContext context, out IResolvedSequence<float> resolvedSequence, Rounding rounding = Rounding.RoundDown)
        {
            return Round(resolver.Resolve(startingValue, context, out resolvedSequence), rounding);
        }
        
        public static int Round(float value, Rounding rounding)
        {
            return rounding == Rounding.RoundDown ? (int)value : (int)Math.Ceiling(value);
        }
    }
    internal sealed class SequentialValueResolver<T> : ISequentialValueResolver<T>
    {
        public SequentialValueResolver(IEnumerable<IValueModifier<T>> modifiers)
        {
            Modifiers = modifiers;
        }

        private IEnumerable<IValueModifier<T>> Modifiers { get; }

        public T Resolve(T startingValue, IContext context) => Resolve(startingValue, context, out _);
        public T Resolve(T startingValue, IContext context, out IResolvedSequence<T> resolvedSequence)
        {
            PassThroughModifiers(startingValue, context, out resolvedSequence);
            return resolvedSequence.Output;
        }

        public IResolvedSequence<T> ResolveSequence(T startingValue, IContext context)
        {
            Resolve(startingValue, context, out IResolvedSequence<T> sequentiallyResolvedOutput);
            return sequentiallyResolvedOutput;
        }

        private void PassThroughModifiers(T startingValue, IContext context, out IResolvedSequence<T> resolvedSequence)
        {
            List<IModificationOutput<T>> outputs = new List<IModificationOutput<T>>();
            List<IModificationInput<T>> inputs = new List<IModificationInput<T>>();
            T previousOutput = startingValue;
            foreach (IValueModifier<T> modifier in Modifiers)
            {
                IModificationInput<T> functionInput = new ModificationInput<T>(this, context, previousOutput);
                IModificationOutput<T> functionOutput = modifier.ModifyValue(functionInput);
                if (functionOutput != null) previousOutput = functionOutput.Output;
                inputs.Add(functionInput);
                outputs.Add(functionOutput);
            }
            resolvedSequence = new ResolvedSequence<T>(this, Modifiers.ToImmutableList(), startingValue, context, inputs.ToImmutableList(), previousOutput, outputs.ToImmutableList());
        }
    }
    internal class PriorityValueResolver<T> : IPriorityValueResolver<T>
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
        public T Resolve(T startingValue, IContext context, out IResolvedSequence<T> resolvedSequence)
        {
            return SequentialValueResolver.Resolve(startingValue, context, out resolvedSequence);
        }

        public T Resolve(T startingValue, IContext context)
        {
            return SequentialValueResolver.Resolve(startingValue, context);
        }

        public IResolvedSequence<T> ResolveSequence(T startingValue, IContext context) => SequentialValueResolver.ResolveSequence(startingValue, context);
    }
    public interface IResolvedSequence<T>
    {
        IContext Context { get; }
        T Output { get; }
        ISequentialValueResolver<T> Resolver { get; }
        T StartingValue { get; }
        IReadOnlyList<IModificationInput<T>> StepInputs { get; }
        IReadOnlyList<IModificationOutput<T>> StepOutputs { get; }
        IReadOnlyList<IValueModifier<T>> Modifiers { get; }
    }
    internal sealed class ResolvedSequence<T> : IResolvedSequence<T>
    {
        public ResolvedSequence(ISequentialValueResolver<T> resolver, IReadOnlyList<IValueModifier<T>> modifiers, T startingValue, IContext context, IReadOnlyList<IModificationInput<T>> stepInputs, T output, IReadOnlyList<IModificationOutput<T>> stepOutputs)
        {
            Resolver = resolver;
            Modifiers = modifiers;
            StartingValue = startingValue;
            Context = context;
            StepInputs = stepInputs;
            StepOutputs = stepOutputs;
            Output = output;
        }
        public IContext Context { get; }

        public T Output { get; }

        public ISequentialValueResolver<T> Resolver { get; }

        public T StartingValue { get; }

        public IReadOnlyList<IModificationInput<T>> StepInputs { get; }

        public IReadOnlyList<IModificationOutput<T>> StepOutputs { get; }

        public IReadOnlyList<IValueModifier<T>> Modifiers { get; }
    }
}