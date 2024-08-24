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
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        private IResolvedSequenceDescriber<float> TableRoundDown { get; } = new ResolvedIntFromFloatTableDescriber();
        public LocString DescribeSequenceAsTableAndRoundDown<TContext>(IResolvedSequence<float, TContext> resolvedSequence) where TContext : IContext
        {
            return TableRoundDown.DescribeSequence(resolvedSequence);
        }
    }
}
