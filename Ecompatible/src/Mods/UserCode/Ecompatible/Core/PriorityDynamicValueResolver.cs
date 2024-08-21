using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecompatible
{
    public interface IValueResolver<T, TContext> where TContext : IContext
    {
        T Resolve(T startingValue, TContext context);
    }
    public enum Rounding
    {
        RoundDown,
        RoundUp
    }
    public interface ISequentialValueResolver<T, TContext> : IValueResolver<T, TContext> where TContext : IContext
    {
        T Resolve(T startingValue, TContext context, out IResolvedSequence<T, TContext> resolvedSequence);
        IResolvedSequence<T, TContext> ResolveSequence(T startingValue, TContext context);
    }
    public interface IPriorityValueResolver<T, TContext> : ISequentialValueResolver<T, TContext> where TContext : IContext
    {
        IEnumerable<IValueModifier<T, TContext>> Modifiers { get; }
        void Add(float priority, IValueModifier<T, TContext> modifier);
    }
    public static class ResolverExtensions
    {
        public static int ResolveInt<TContext>(this IValueResolver<float, TContext> resolver, float startingValue, TContext context, Rounding rounding = Rounding.RoundDown) where TContext : IContext
        {
            return Round(resolver.Resolve(startingValue, context), rounding);
        }
        public static int ResolveInt<TContext>(this ISequentialValueResolver<float, TContext> resolver, float startingValue, TContext context, out IResolvedSequence<float, TContext> resolvedSequence, Rounding rounding = Rounding.RoundDown) where TContext : IContext
        {
            return Round(resolver.Resolve(startingValue, context, out resolvedSequence), rounding);
        }
        
        public static int Round(float value, Rounding rounding)
        {
            return rounding == Rounding.RoundDown ? (int)value : (int)Math.Ceiling(value);
        }
    }
    internal sealed class SequentialValueResolver<T, TContext> : ISequentialValueResolver<T, TContext> where TContext : IContext
    {
        public SequentialValueResolver(IEnumerable<IValueModifier<T, TContext>> modifiers)
        {
            Modifiers = modifiers;
        }

        private IEnumerable<IValueModifier<T, TContext>> Modifiers { get; }

        public T Resolve(T startingValue, TContext context) => Resolve(startingValue, context, out _);
        public T Resolve(T startingValue, TContext context, out IResolvedSequence<T, TContext> resolvedSequence)
        {
            PassThroughModifiers(startingValue, context, out resolvedSequence);
            return resolvedSequence.Output;
        }

        public IResolvedSequence<T, TContext> ResolveSequence(T startingValue, TContext context)
        {
            Resolve(startingValue, context, out IResolvedSequence<T, TContext> sequentiallyResolvedOutput);
            return sequentiallyResolvedOutput;
        }

        private void PassThroughModifiers(T startingValue, TContext context, out IResolvedSequence<T, TContext> resolvedSequence)
        {
            List<IModificationOutput<T>> outputs = new List<IModificationOutput<T>>();
            List<IModificationInput<T, TContext>> inputs = new List<IModificationInput<T, TContext>>();
            T previousOutput = startingValue;
            foreach (IValueModifier<T, TContext> modifier in Modifiers)
            {
                IModificationInput<T, TContext> functionInput = new ModificationInput<T, TContext>(this, context, previousOutput);
                IModificationOutput<T> functionOutput = modifier.ModifyValue(functionInput);
                if (functionOutput != null) previousOutput = functionOutput.Output;
                inputs.Add(functionInput);
                outputs.Add(functionOutput);
            }
            resolvedSequence = new ResolvedSequence<T, TContext>(this, Modifiers.ToImmutableList(), startingValue, context, inputs.ToImmutableList(), previousOutput, outputs.ToImmutableList());
        }
    }
    internal class PriorityValueResolver<T, TContext> : IPriorityValueResolver<T, TContext> where TContext : IContext
    {
        private ImmutableList<(float, IValueModifier<T, TContext>)> requestHandlers = ImmutableList<(float, IValueModifier<T, TContext>)>.Empty;

        private IComparer<(float, IValueModifier<T, TContext>)> Comparer { get; } = new NumberComparer<(float, IValueModifier<T, TContext>)>(x => x.Item1);
        private ImmutableList<(float, IValueModifier<T, TContext>)> RequestHandlers
        {
            get => requestHandlers; set
            {
                requestHandlers = value;
                SequentialValueResolver = new SequentialValueResolver<T, TContext>(value.Select(pair => pair.Item2));
            }
        }
        public IEnumerable<IValueModifier<T, TContext>> Modifiers => RequestHandlers.Select(pair => pair.Item2);
        private ISequentialValueResolver<T, TContext> SequentialValueResolver { get; set; } = new SequentialValueResolver<T, TContext>(Enumerable.Empty<IValueModifier<T, TContext>>());
        public PriorityValueResolver(params (float, IValueModifier<T, TContext>)[] requestHandlers)
        {
            RequestHandlers = RequestHandlers.AddRange(requestHandlers);
        }
        public PriorityValueResolver() { }

        public void Add(float priority, IValueModifier<T, TContext> modifier)
        {
            RequestHandlers = RequestHandlers.Add((priority, modifier)).Sort(Comparer);
        }
        public T Resolve(T startingValue, TContext context, out IResolvedSequence<T, TContext> resolvedSequence)
        {
            return SequentialValueResolver.Resolve(startingValue, context, out resolvedSequence);
        }

        public T Resolve(T startingValue, TContext context)
        {
            return SequentialValueResolver.Resolve(startingValue, context);
        }

        public IResolvedSequence<T, TContext> ResolveSequence(T startingValue, TContext context) => SequentialValueResolver.ResolveSequence(startingValue, context);
    }
    public interface IResolvedSequence<T, TContext> where TContext : IContext
    {
        TContext Context { get; }
        T Output { get; }
        ISequentialValueResolver<T, TContext> Resolver { get; }
        T StartingValue { get; }
        IReadOnlyList<IModificationInput<T, TContext>> StepInputs { get; }
        IReadOnlyList<IModificationOutput<T>> StepOutputs { get; }
        IReadOnlyList<IValueModifier<T, TContext>> Modifiers { get; }
    }
    internal sealed class ResolvedSequence<T, TContext> : IResolvedSequence<T, TContext> where TContext : IContext
    {
        public ResolvedSequence(ISequentialValueResolver<T, TContext> resolver, IReadOnlyList<IValueModifier<T, TContext>> modifiers, T startingValue, TContext context, IReadOnlyList<IModificationInput<T, TContext>> stepInputs, T output, IReadOnlyList<IModificationOutput<T>> stepOutputs)
        {
            Resolver = resolver;
            Modifiers = modifiers;
            StartingValue = startingValue;
            Context = context;
            StepInputs = stepInputs;
            StepOutputs = stepOutputs;
            Output = output;
        }
        public TContext Context { get; }

        public T Output { get; }

        public ISequentialValueResolver<T, TContext> Resolver { get; }

        public T StartingValue { get; }

        public IReadOnlyList<IModificationInput<T, TContext>> StepInputs { get; }

        public IReadOnlyList<IModificationOutput<T>> StepOutputs { get; }

        public IReadOnlyList<IValueModifier<T, TContext>> Modifiers { get; }
    }
}