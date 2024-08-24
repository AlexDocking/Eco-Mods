// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Ecompatible
{
    internal class ModificationInput<T, TContext> : IModificationInput<T, TContext> where TContext : IContext
    {
        public ModificationInput(IValueResolver<T, TContext> resolver, TContext context, T input)
        {
            Resolver = resolver;
            Input = input;
            Context = context;
        }
        public IValueResolver<T, TContext> Resolver { get; }
        public TContext Context { get; }
        public T Input { get; }
    }
    public interface IModificationInput<T, TContext> where TContext : IContext
    {
        IValueResolver<T, TContext> Resolver { get; }
        TContext Context { get; }
        T Input { get; }
    }
    public interface IModificationOutput<T>
    {
        LocString MarkedUpModificationName { get; }
        string ModificationName { get; }
        T Output { get; }
    }

    public static class OutputFactory
    {
        public static IModificationOutput<float> BaseLevel(float output) => new BaseLevelModificationOutput(output, Localizer.DoStr("Base Level"));
        public static IModificationOutput<float> BaseLevel(float output, LocString modificationName) => new BaseLevelModificationOutput(output, modificationName);
        public static IModificationOutput<float> Multiplier(float output, LocString modificationName, float multiplier) => new MultiplicationModificationOutput(output, modificationName, multiplier);
        public static IModificationOutput<float> Overwrite(float newOutput, LocString modificationName) => new OverwriteModificationOutput(newOutput, modificationName);
    }
    internal abstract class ModificationOutputBase : IModificationOutput<float>
    {
        public LocString MarkedUpModificationName { get; set; }
        public LocString ModificationDescription { get; set; }
        public float Output { get; set; }
        public string ModificationName => MarkedUpModificationName;

        public ModificationOutputBase(float output, LocString modificationName)
        {
            Output = output;
            MarkedUpModificationName = modificationName;
        }
    }

    internal class BaseLevelModificationOutput : ModificationOutputBase
    {
        public BaseLevelModificationOutput(float output, LocString name) : base(output, name)
        {
        }
    }

    internal class OverwriteModificationOutput : ModificationOutputBase
    {
        public OverwriteModificationOutput(float output, LocString modificationName) : base(output, modificationName)
        {
        }
    }
    internal class MultiplicationModificationOutput : ModificationOutputBase
    {
        public MultiplicationModificationOutput(float output, LocString modificationName, float multiplier) : base(output, modificationName)
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
        internal static LocString Strikethrough(this LocString text)
        {
            return text.Wrap("<s>", "</s>");
        }
    }
    internal interface IResolvedOutputRowDescriber<T, TRow>
    {
        TRow DescribeAsRow<TContext>(IResolvedSequence<T, TContext> resolvedSequence, int index) where TContext : IContext;
        TRow DescribeAsResult<TContext>(IResolvedSequence<T, TContext> resolvedSequence, int index) where TContext : IContext;
    }
    internal interface IResolvedSequenceDescriber<T>
    {
        public LocString DescribeSequence<TContext>(IResolvedSequence<T, TContext> resolvedSequence) where TContext : IContext;
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
        public LocString DescribeSequence<TContext>(IResolvedSequence<T, TContext> resolvedSequence) where TContext : IContext
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

        public LocString DescribeSequence<TContext>(IResolvedSequence<float, TContext> resolvedSequence) where TContext : IContext
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
            
            private bool ShouldStrikethrough(IReadOnlyList<IModificationOutput<float>> stepOutputs, int index)
            {
                for (int i = index + 1; i < stepOutputs.Count; i++)
                {
                    if (stepOutputs[i] is OverwriteModificationOutput) return true;
                }
                return false;
            }
            private bool ShouldDisplay(IReadOnlyList<IModificationOutput<float>> stepOutputs, int index)
            {
                for (int i = stepOutputs.Count - 1; i > index; i--)
                {
                    if (stepOutputs[i] is BaseLevelModificationOutput)
                    {
                        return false;
                    }
                }
                return true;
            }
            public TableRowInformation DescribeAsRow<TContext>(IResolvedSequence<float, TContext> resolvedSequence, int index) where TContext : IContext
            {
                TableRowInformation row = DescribeBasic<TContext>(resolvedSequence, index);
                if (row != null && ShouldStrikethrough(resolvedSequence.StepOutputs, index))
                {
                    return new TableRowInformation(row.Name, row.Effect.Strikethrough());
                }
                return row;
            }
            private TableRowInformation DescribeBasic<TContext>(IResolvedSequence<float, TContext> resolvedSequence, int index) where TContext : IContext
            {
                if (!ShouldDisplay(resolvedSequence.StepOutputs, index)) return null;

                IModificationOutput<float> stepOutput = resolvedSequence.StepOutputs[index];
                if (stepOutput is BaseLevelModificationOutput baseLevelOperationDetails) return TableRowContent(baseLevelOperationDetails);
                if (stepOutput is MultiplicationModificationOutput multiplicationOperationDetails) return TableRowContent(multiplicationOperationDetails);
                if (stepOutput is ModificationOutputBase operationDetailsBase) return TableRowContent(operationDetailsBase);
                if (stepOutput is OverwriteModificationOutput overwriteModificationOutput) return TableRowContent(overwriteModificationOutput);
                return null;
            }
            public TableRowInformation DescribeAsResult<TContext>(IResolvedSequence<float, TContext> resolvedSequence, int index) where TContext : IContext
            {
                IModificationOutput<float> stepOutput = resolvedSequence.StepOutputs.Take(index + 1).Reverse().First(step => step != null);
                LocString resultCell = Rounding == Rounding.RoundUp ? Localizer.DoStr("Result (rounded up)") : Localizer.DoStr("Result (rounded down)");
                return new TableRowInformation(resultCell, Localizer.NotLocalizedStr(Text.Num(ResolverExtensions.Round(stepOutput.Output, Rounding))));
            }
            private TableRowInformation TableRowContent(ModificationOutputBase details)
            {
                return new TableRowInformation(details.MarkedUpModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            }
            private TableRowInformation TableRowContent(BaseLevelModificationOutput details)
            {
                return new TableRowInformation(details.MarkedUpModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            }

            private TableRowInformation TableRowContent(MultiplicationModificationOutput details)
            {
                return new TableRowInformation(details.MarkedUpModificationName, Localizer.NotLocalizedStr(details.Multiplier >= 1 ? Text.Positive($"+{Text.Percent(details.Multiplier - 1)}") : Text.Negative(Text.Percent(details.Multiplier - 1))));
            }
            private TableRowInformation TableRowContent(OverwriteModificationOutput details)
            {
                return new TableRowInformation(details.MarkedUpModificationName, Localizer.NotLocalizedStr(Text.Num(details.Output)));
            }
        }
    }
}
