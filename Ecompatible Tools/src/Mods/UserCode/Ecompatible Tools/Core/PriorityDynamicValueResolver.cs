using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ecompatible
{
    public interface IValueResolver
    {
        float Resolve(float startingValue, IValueModificationContext context);
        int ResolveInt(float startingValue, IValueModificationContext context, Rounding rounding = Rounding.RoundDown);
    }
    public enum Rounding
    {
        RoundDown,
        RoundUp
    }
    public interface IPriorityValueResolver : IValueResolver
    {
        void Add(float priority, IValueModifier handler);
        void Remove(float priority, IValueModifier handler);
        void Clear();
        IEnumerable<(float, IValueModifier)> Handlers { get; }
        float Resolve(float startingValue, IValueModificationContext context, out AuxillaryInfo auxillaryInfo);
        int ResolveInt(float startingValue, IValueModificationContext context, out AuxillaryInfo auxillaryInfo, Rounding rounding = Rounding.RoundDown);
    }
    public class PriorityDynamicValueResolver : IPriorityValueResolver
    {
        private IComparer<(float, IValueModifier)> Comparer { get; } = new NumberComparer<(float, IValueModifier)>(x => x.Item1);
        private ImmutableList<(float, IValueModifier)> RequestHandlers { get; set; } = ImmutableList<(float, IValueModifier)>.Empty;
        public IEnumerable<(float, IValueModifier)> Handlers => RequestHandlers;
        public PriorityDynamicValueResolver(params (float, IValueModifier)[] requestHandlers)
        {
            RequestHandlers = RequestHandlers.AddRange(requestHandlers);
        }
        public PriorityDynamicValueResolver() { }

        public float Resolve(float startingValue, IValueModificationContext context) => Resolve(startingValue, context, out _);
        public float Resolve(float startingValue, IValueModificationContext context, out AuxillaryInfo auxillaryInfo)
        {
            PassThroughHandlers(startingValue, context, out auxillaryInfo);
            return auxillaryInfo.FloatOutput;
        }
        public int ResolveInt(float startingValue, IValueModificationContext context, Rounding rounding = Rounding.RoundDown) => ResolveInt(startingValue, context, out _, rounding);
        public int ResolveInt(float startingValue, IValueModificationContext context, out AuxillaryInfo auxillaryInfo, Rounding rounding = Rounding.RoundDown)
        {
            PassThroughHandlers(startingValue, context, out auxillaryInfo);
            return Round(auxillaryInfo.FloatOutput, rounding);
        }
        private void PassThroughHandlers(float startingValue, IValueModificationContext context, out AuxillaryInfo auxillaryInfo, Rounding rounding = Rounding.RoundDown)
        {
            List<IModificationOutput> steps = new List<IModificationOutput>();
            float previousOutput = startingValue;
            foreach ((_, IValueModifier handler) in RequestHandlers)
            {
                IModificationInput functionInput = new ModificationInput(this, context, previousOutput);
                IModificationOutput functionOutput = handler.ModifyValue(functionInput) ?? new NoOperationDetails(previousOutput);
                previousOutput = functionOutput.Output;
                steps.Add(functionOutput);
            }
            auxillaryInfo = new AuxillaryInfo(steps.ToArray(), Round(steps[^1].Output, rounding));
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
        public static int Round(float value, Rounding rounding)
        {
            return rounding == Rounding.RoundDown ? (int)value : (int)Math.Ceiling(value);
        }
    }
    public class AuxillaryInfo
    {
        public IModificationOutput[] StepOutputs { get; }
        public float FloatOutput { get; }
        public int IntOutput { get; }

        public AuxillaryInfo(IModificationOutput[] stepOutputs, int intOutput)
        {
            StepOutputs = stepOutputs;
            FloatOutput = stepOutputs[^1].Output;
            IntOutput = intOutput;
        }
    }
}