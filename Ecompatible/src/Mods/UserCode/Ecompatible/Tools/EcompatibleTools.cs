// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

namespace Ecompatible
{
    public static partial class ValueResolvers
    {
        public static ToolResolvers Tools { get; } = new ToolResolvers();
    }
    public partial class ToolResolvers
    {
        public ShovelResolvers Shovel { get; } = new ShovelResolvers();
        public PickaxeResolvers Pickaxe { get; } = new PickaxeResolvers();
        public AxeResolvers Axe { get; } = new AxeResolvers();
    }
    public partial class ShovelResolvers
    {
        /// <summary>
        /// List of modifiers that change MaxTake.
        /// </summary>
        public IPriorityValueResolver<float, IShovelPickUpContext> MaxTakeResolver { get; } = ValueResolverFactory.CreatePriorityResolver<float, IShovelPickUpContext>((float.MaxValue, new EnsureValueIsAtLeast<IShovelPickUpContext>(1)));
    }
    public partial class PickaxeResolvers
    {
        public MiningSweepingHandsResolvers MiningSweepingHands { get; } = new MiningSweepingHandsResolvers();
    }
    public partial class MiningSweepingHandsResolvers
    {
        public IPriorityValueResolver<float, IUserPickUpRubbleContext> PickUpRangeResolver { get; } = ValueResolverFactory.CreatePriorityResolver<float, IUserPickUpRubbleContext>((float.MinValue, new DefaultPickupRange<IUserPickUpRubbleContext>()));
    }
    public partial class AxeResolvers
    {
        public IPriorityValueResolver<float, ITreeFelledContext> FractionOfTreeToAutoSlice { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>();
        public IPriorityValueResolver<float, ITreeFelledContext> DamageToStumpWhenFelled { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>();
        public IPriorityValueResolver<float, ITreeFelledContext> MaxTreeDebrisToSpawn { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>();
        public IPriorityValueResolver<float, ITreeFelledContext> ChanceToClearDebrisOnSpawn { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>();
    }
}