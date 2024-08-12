using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Ecompatible
{
    internal class ModificationInput : IModificationInput
    {
        public ModificationInput(IValueResolver resolver, IValueModificationContext context, float input)
        {
            Resolver = resolver;
            Input = input;
            Context = context;
        }

        public IValueResolver Resolver { get; }
        public IValueModificationContext Context { get; }
        public float Input { get; }
    }
    public interface IModificationInput
    {
        IValueResolver Resolver { get; }
        IValueModificationContext Context { get; }
        float Input { get; }
    }
    public interface IModificationOutput
    {
        LocString ModificationName { get; }
        float Output { get; set; }
    }

    public abstract class ModificationOutputBase : IModificationOutput
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

    public class NoOperationDetails : ModificationOutputBase
    {
        public NoOperationDetails(float output) : base(output, LocString.Empty)
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
    public enum Alignment
    {
        Left,
        Right,
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
        private List<IModificationOutput> SelectAppliedUsedOperations(IModificationOutput[] allSteps)
        {
            Stack<IModificationOutput> stack = new Stack<IModificationOutput>();
            for (int i = allSteps.Length - 1; i >= 0; i--)
            {
                var step = allSteps[i];
                if (step is BaseLevelModificationOutput)
                {
                    stack.Push(step);
                    break;
                }
                else if (step is not NoOperationDetails)
                {
                    stack.Push(step);
                }
            }
            return new List<IModificationOutput>(stack);
        }
        public LocString GenerateDescription(AuxillaryInfo info)
        {
            IModificationOutput[] allSteps = info.StepOutputs;
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
            locStringBuilder.AddRow((Localizer.DoStr("Result") + ":", Localizer.NotLocalizedStr(Text.Num(info.IntOutput)).Align("right")));
            locStringBuilder.EndTable();
            return locStringBuilder.ToLocString();
        }
        private bool TableRowContent(IModificationOutput details, out (LocString Name, LocString Effect) content)
        {
            if (details is NoOperationDetails noOperationDetails) return TableRowContent(noOperationDetails, out content);
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
        private bool TableRowContent(NoOperationDetails details, out (LocString Name, LocString Effect) content)
        {
            content = default;
            return false;
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
