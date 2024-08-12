using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Ecompatible
{
    internal class ModificationInput<T> : IModificationInput<T>
    {
        public ModificationInput(IValueResolver<T> resolver, IValueModificationContext context, T input)
        {
            Resolver = resolver;
            Input = input;
            Context = context;
        }
        public IValueResolver<T> Resolver { get; }
        public IValueModificationContext Context { get; }
        public T Input { get; }
    }
    public interface IModificationInput<T>
    {
        IValueResolver<T> Resolver { get; }
        IValueModificationContext Context { get; }
        T Input { get; }
    }
    public interface IModificationOutput<T>
    {
        LocString ModificationName { get; }
        T Output { get; set; }
    }

    public abstract class ModificationOutputBase : IModificationOutput<float>
    {
        public LocString ModificationName { get; set; }
        public LocString ModificationDescription { get; set; }
        public float Output { get; set; }
        public ModificationOutputBase(float output, LocString modificationName)
        {
            Output = output;
            ModificationName = modificationName;
        }
    }

    public class BaseLevelModificationOutput : ModificationOutputBase
    {
        public BaseLevelModificationOutput(float output, string name = "Base Level") : base(output, Localizer.DoStr(name))
        {
        }
    }

    public class MultiplicationOperationDetails : ModificationOutputBase
    {
        public MultiplicationOperationDetails(float output, LocString modificationName, float multiplier) : base(output, modificationName)
        {
            Multiplier = multiplier;
        }

        public float Multiplier { get; }
    }
    internal static class LocStringExtensions
    {
        internal static LocString Align(this LocString text, string alignment)
        {
            return text.Wrap($"<align=\"{alignment}\">", "</align>");
        }
    }
    public class ResolvedValueDescriber
    {
        private List<IModificationOutput<float>> SelectAppliedUsedOperations(IModificationOutput<float>[] allSteps)
        {
            Stack<IModificationOutput<float>> stack = new Stack<IModificationOutput<float>>();
            for (int i = allSteps.Length - 1; i >= 0; i--)
            {
                var step = allSteps[i];
                if (step is BaseLevelModificationOutput)
                {
                    stack.Push(step);
                    break;
                }
                else if (step != null)
                {
                    stack.Push(step);
                }
            }
            return new List<IModificationOutput<float>>(stack);
        }
        public LocString GenerateDescription(int intOutput, ResolvedSequence<float> info)
        {
            IModificationOutput<float>[] allSteps = info.StepOutputs;
            var steps = SelectAppliedUsedOperations(allSteps);
            if (!steps.Any()) return LocString.Empty;
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            locStringBuilder.StartTable();
            for (int i = 0; i < steps.Count; i++)
            {
                if (TableRowContent(steps[i], out (LocString Name, LocString Effect) contents))
                {
                    locStringBuilder.AddRow((contents.Name + ":", contents.Effect.Align("right")));
                }
            }
            locStringBuilder.AddRow((Localizer.NotLocalizedStr("---------------------------"), LocString.Empty));
            locStringBuilder.AddRow((Localizer.DoStr("Result") + ":", Localizer.NotLocalizedStr(Text.Num(intOutput)).Align("right")));
            locStringBuilder.EndTable();
            return locStringBuilder.ToLocString();
        }
        private bool TableRowContent(IModificationOutput<float> details, out (LocString Name, LocString Effect) content)
        {
            if (details is BaseLevelModificationOutput baseLevelOperationDetails) return TableRowContent(baseLevelOperationDetails, out content);
            if (details is MultiplicationOperationDetails multiplicationOperationDetails) return TableRowContent(multiplicationOperationDetails, out content);
            if (details is ModificationOutputBase operationDetailsBase) return TableRowContent(operationDetailsBase, out content);
            content = default;
            return false;
        }
        private bool TableRowContent(ModificationOutputBase details, out (LocString Name, LocString Effect) content)
        {
            content = (details.ModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            return true;
        }
        private bool TableRowContent(BaseLevelModificationOutput details, out (LocString Name, LocString Effect) content)
        {
            content = (details.ModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            return true;
        }

        private bool TableRowContent(MultiplicationOperationDetails details, out (LocString Name, LocString Effect) content)
        {
            content = (details.ModificationName, Localizer.NotLocalizedStr(details.Multiplier >= 1 ? Text.Positive($"+{Text.Percent(details.Multiplier - 1)}") : Text.Negative(Text.Percent(details.Multiplier - 1))));
            return true;
        }
    }
}
