using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Ecompatible;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcompatibleTools
{
    public interface IOperationDetails
    {
        float InputFloat { get; set; }
        LocString ModificationName { get; }
        float OutputFloat { get; set; }
    }

    public abstract class OperationDetailsBase : IOperationDetails
    {
        public LocString ModificationName { get; set; }
        public float InputFloat { get; set; }
        public float OutputFloat { get; set; }
        public OperationDetailsBase(LocString modificationName)
        {
            ModificationName = modificationName;
        }
    }
    public class BaseLevelOperationDetails : OperationDetailsBase
    {
        public BaseLevelOperationDetails(string name = "Base Level") : base(Localizer.DoStr(name))
        {
        }
    }
    public class NoOperationDetails : OperationDetailsBase
    {
        public NoOperationDetails() : base(LocString.Empty)
        {
        }
    }
    public class MultiplicationOperationDetails : OperationDetailsBase
    {
        public MultiplicationOperationDetails(LocString modificationName, float multiplier) : base(modificationName)
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
        private List<IOperationDetails> SelectAppliedUsedOperations(IOperationDetails[] allSteps)
        {
            Stack<IOperationDetails> stack = new Stack<IOperationDetails>();
            for (int i = allSteps.Length - 1; i >= 0; i--)
            {
                var step = allSteps[i];
                if (step is BaseLevelOperationDetails)
                {
                    stack.Push(step);
                    break;
                }
                else if (step is not NoOperationDetails)
                {
                    stack.Push(step);
                }
            }
            return new List<IOperationDetails>(stack);
        }
        public LocString GenerateDescription(AuxillaryInfo info)
        {
            IOperationDetails[] allSteps = info.StepOutputs;
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
        private bool TableRowContent(IOperationDetails details, out (LocString Name, LocString Effect) content)
        {
            if (details is NoOperationDetails noOperationDetails) return TableRowContent(noOperationDetails, out content);
            if (details is BaseLevelOperationDetails baseLevelOperationDetails) return TableRowContent(baseLevelOperationDetails, out content);
            if (details is MultiplicationOperationDetails multiplicationOperationDetails) return TableRowContent(multiplicationOperationDetails, out content);
            if (details is OperationDetailsBase operationDetailsBase) return TableRowContent(operationDetailsBase, out content);
            content = default;
            return false;
        }
        private bool TableRowContent(OperationDetailsBase details, out (LocString Name, LocString Effect) content)
        {
            content = (details.ModificationName, Localizer.NotLocalizedStr(Text.Num(details.OutputFloat)));
            return true;
        }
        private bool TableRowContent(NoOperationDetails details, out (LocString Name, LocString Effect) content) 
        {
            content = default;
            return false;
        }
        private bool TableRowContent(BaseLevelOperationDetails details, out (LocString Name, LocString Effect) content)
        {
            content = (details.ModificationName, Localizer.NotLocalizedStr(Text.Num(details.OutputFloat)));
            return true;
        }

        private bool TableRowContent(MultiplicationOperationDetails details, out (LocString Name, LocString Effect) content)
        {
            content = (details.ModificationName, Localizer.NotLocalizedStr(details.Multiplier >= 1 ? Text.Positive($"+{Text.Percent(details.Multiplier - 1)}") : Text.Negative(Text.Percent(details.Multiplier - 1))));
            return true;
        }
    }
}
