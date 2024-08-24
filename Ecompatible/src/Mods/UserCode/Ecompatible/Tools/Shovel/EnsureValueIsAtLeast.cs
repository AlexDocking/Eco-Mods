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

namespace Ecompatible
{
    internal class EnsureValueIsAtLeast<TContext> : IValueModifier<float, TContext> where TContext : IContext
    {
        public EnsureValueIsAtLeast(float minimum)
        {
            Minimum = minimum;
        }

        public float Minimum { get; }

        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            if (functionInput.Input < Minimum)
            {
                return OutputFactory.Overwrite(Minimum, Localizer.DoStr($"Must be at least {Text.Num(Minimum)} (got {Text.Num(functionInput.Input)})"));
            }
            return null;
        }
    }
}
