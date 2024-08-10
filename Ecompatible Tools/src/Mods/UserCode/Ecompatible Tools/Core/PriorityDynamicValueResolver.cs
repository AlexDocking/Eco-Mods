using Eco.Shared.Localization;
using EcompatibleTools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecompatible
{
    public interface IValueResolver
    {
        float Resolve(IValueModificationContext context);
        int ResolveInt(IValueModificationContext context, Rounding rounding = Rounding.RoundDown);
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
        float Resolve(IValueModificationContext context, out AuxillaryInfo auxillaryInfo);
        int ResolveInt(IValueModificationContext context, out AuxillaryInfo auxillaryInfo, Rounding rounding = Rounding.RoundDown);
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

        public float Resolve(IValueModificationContext context) => Resolve(context, out _);
        public float Resolve(IValueModificationContext context, out AuxillaryInfo auxillaryInfo)
        {
            PassThroughHandlers(context, out auxillaryInfo);
            return context.FloatValue;
        }
        public int ResolveInt(IValueModificationContext context, Rounding rounding = Rounding.RoundDown) => ResolveInt(context, out _, rounding);
        public int ResolveInt(IValueModificationContext context, out AuxillaryInfo auxillaryInfo, Rounding rounding = Rounding.RoundDown)
        {
            PassThroughHandlers(context, out auxillaryInfo);
            return Round(context.FloatValue, rounding);
        }
        private void PassThroughHandlers(IValueModificationContext context, out AuxillaryInfo auxillaryInfo, Rounding rounding = Rounding.RoundDown)
        {
            List<IOperationDetails> steps = new List<IOperationDetails>();
            foreach ((_, IValueModifier handler) in RequestHandlers)
            {
                IOperationDetails operationDetails = new NoOperationDetails();
                operationDetails.InputFloat = context.FloatValue;
                handler.ModifyValue(context, ref operationDetails);
                operationDetails.OutputFloat = context.FloatValue;
                steps.Add(operationDetails);
            }
            auxillaryInfo = new AuxillaryInfo(steps.ToArray(), Round(steps[^1].OutputFloat, rounding));
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
        public IOperationDetails[] StepOutputs { get; }
        public float FloatOutput { get; }
        public int IntOutput { get; }

        public AuxillaryInfo(IOperationDetails[] stepOutputs, int intOutput)
        {
            StepOutputs = stepOutputs;
            FloatOutput = stepOutputs[^1].OutputFloat;
            IntOutput = intOutput;
        }
    }
}