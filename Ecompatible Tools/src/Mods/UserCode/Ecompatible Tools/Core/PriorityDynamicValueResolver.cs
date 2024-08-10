using Eco.Shared.Localization;
using EcompatibleTools;
using System.Collections.Generic;
using System.Collections.Immutable;

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
        float Resolve(IValueModificationContext context, out AuxillaryInfo auxillaryInfo);
        int ResolveInt(IValueModificationContext context, out AuxillaryInfo auxillaryInfo);
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
        public int ResolveInt(IValueModificationContext context) => ResolveInt(context, out _);
        public int ResolveInt(IValueModificationContext context, out AuxillaryInfo auxillaryInfo)
        {
            PassThroughHandlers(context, out auxillaryInfo);
            return context.IntValue;
        }
        private void PassThroughHandlers(IValueModificationContext context, out AuxillaryInfo auxillaryInfo)
        {
            List<IOperationDetails> steps = new List<IOperationDetails>();
            foreach ((_, IValueModifier handler) in RequestHandlers)
            {
                IOperationDetails operationDetails = new NoOperationDetails();
                operationDetails.InputFloat = context.IntValue;
                handler.ModifyValue(context, ref operationDetails);
                operationDetails.OutputFloat = context.IntValue;
                steps.Add(operationDetails);
            }
            auxillaryInfo = new AuxillaryInfo(steps.ToArray());
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
    public class AuxillaryInfo
    {
        public IOperationDetails[] StepOutputs { get; }

        public AuxillaryInfo(IOperationDetails[] stepOutputs)
        {
            StepOutputs = stepOutputs;
        }
    }
}