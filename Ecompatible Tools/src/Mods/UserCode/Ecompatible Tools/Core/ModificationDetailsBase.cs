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
    internal interface IResolvedOutputRowDescriber<U, V>
    {
        V DescribeAsRow(IResolvedSequence<U> resolvedSequence, int index);
        V DescribeAsResult(IResolvedSequence<U> resolvedSequence, int index);
    }
    internal interface IResolvedSequenceDescriber<T>
    {
        public LocString DescribeSequence(IResolvedSequence<T> resolvedSequence);
    }
    internal class TableRowInformation
    {
        public TableRowInformation(LocString name, LocString effect)
        {
            Name = name;
            Effect = effect;
        }

        public LocString Name { get; }
        public LocString Effect { get; }
    }
    internal class ResolvedSequenceTableDescriber<T> : IResolvedSequenceDescriber<T>
    {
        public IResolvedOutputRowDescriber<T, TableRowInformation> StepDescriber { get; }

        public ResolvedSequenceTableDescriber(IResolvedOutputRowDescriber<T, TableRowInformation> stepDescriber)
        {
            StepDescriber = stepDescriber;
        }
        private void AppendRow(LocStringBuilder locStringBuilder, TableRowInformation tableRowInformation)
        {
            if (tableRowInformation != null)
            {
                locStringBuilder.AddRow((tableRowInformation.Name + ":", tableRowInformation.Effect.Align("right")));
            }
        }
        public LocString DescribeSequence(IResolvedSequence<T> resolvedSequence)
        {
            IReadOnlyList<IModificationOutput<T>> stepOutputs = resolvedSequence.StepOutputs;
            if (!stepOutputs.Any()) return LocString.Empty;
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            locStringBuilder.StartTable();
            for (int i = 0; i < stepOutputs.Count; i++)
            {
                AppendRow(locStringBuilder, StepDescriber.DescribeAsRow(resolvedSequence, i));
            }
            locStringBuilder.AddRow((Localizer.NotLocalizedStr("---------------------------"), LocString.Empty));
            AppendRow(locStringBuilder, StepDescriber.DescribeAsResult(resolvedSequence, resolvedSequence.StepOutputs.Count - 1));
            locStringBuilder.EndTable();
            return locStringBuilder.ToLocString();
        }
    }
    internal class ResolvedIntFromFloatTableDescriber : IResolvedSequenceDescriber<float>
    {
        private ResolvedSequenceTableDescriber<float> TableRowDescriber { get; }
        public ResolvedIntFromFloatTableDescriber(Rounding rounding = Rounding.RoundDown)
        {
            TableRowDescriber = new ResolvedSequenceTableDescriber<float>(new TableRowInformationPopulator(rounding));
        }

        public LocString DescribeSequence(IResolvedSequence<float> resolvedSequence)
        {
            return TableRowDescriber.DescribeSequence(resolvedSequence);
        }

        private class TableRowInformationPopulator : IResolvedOutputRowDescriber<float, TableRowInformation>
        {
            public TableRowInformationPopulator(Rounding rounding)
            {
                Rounding = rounding;
            }

            private Rounding Rounding { get; }

            private int IndexOfLastBaseLevelModification(IReadOnlyList<IModificationOutput<float>> stepOutputs)
            {
                for (int i = stepOutputs.Count - 1; i >= 0; i--)
                {
                    if (stepOutputs[i] is BaseLevelModificationOutput)
                    {
                        return i;
                    }
                }
                return 0;
            }
            public TableRowInformation DescribeAsRow(IResolvedSequence<float> resolvedSequence, int index)
            {
                if (index < IndexOfLastBaseLevelModification(resolvedSequence.StepOutputs)) return null;

                IModificationOutput<float> stepOutput = resolvedSequence.StepOutputs[index];
                if (stepOutput is BaseLevelModificationOutput baseLevelOperationDetails) TableRowContent(baseLevelOperationDetails);
                if (stepOutput is MultiplicationOperationDetails multiplicationOperationDetails) return TableRowContent(multiplicationOperationDetails);
                if (stepOutput is ModificationOutputBase operationDetailsBase) return TableRowContent(operationDetailsBase);
                return null;
            }
            public TableRowInformation DescribeAsResult(IResolvedSequence<float> resolvedSequence, int index)
            {
                IModificationOutput<float> stepOutput = resolvedSequence.StepOutputs[index];
                LocString resultCell = Rounding == Rounding.RoundUp ? Localizer.DoStr("Result (rounded up)") : Localizer.DoStr("Result (rounded down)");
                return new TableRowInformation(resultCell, Localizer.NotLocalizedStr(Text.Num(ResolverExtensions.Round(stepOutput.Output, Rounding))));
            }
            private TableRowInformation TableRowContent(ModificationOutputBase details)
            {
                return new TableRowInformation(details.ModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            }
            private TableRowInformation TableRowContent(BaseLevelModificationOutput details)
            {
                return new TableRowInformation(details.ModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            }

            private TableRowInformation TableRowContent(MultiplicationOperationDetails details)
            {
                return new TableRowInformation(details.ModificationName, Localizer.NotLocalizedStr(details.Multiplier >= 1 ? Text.Positive($"+{Text.Percent(details.Multiplier - 1)}") : Text.Negative(Text.Percent(details.Multiplier - 1))));
            }
        }
    }
}
