using Eco.Shared.Localization;
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
            List<StepOutput> steps = new List<StepOutput>();
            int stepCount = 0;
            foreach ((_, IValueModifier handler) in RequestHandlers)
            {
                handler.ModifyValue(context, out LocString description, out ModificationType modificationType);
                steps.Add(new StepOutput()
                {
                    Step = stepCount++,
                    Modifier = handler,
                    FloatOutput = context.FloatValue,
                    IntOutput = context.IntValue,
                    Description = description,
                    ModificationType = modificationType
                });
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
        public StepOutput[] StepOutputs { get; }

        public AuxillaryInfo(StepOutput[] stepOutputs)
        {
            StepOutputs = stepOutputs;
        }
    }
    public class StepOutput
    {
        public int Step { get; init; }
        public IValueModifier Modifier { get; init; }
        public float FloatOutput { get; init; }
        public int IntOutput { get; init; }
        public LocString Description { get; init; }
        public ModificationType ModificationType { get; init; }
    }
}