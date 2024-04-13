// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace CompatibleTools
{
    using System;

    public class MaxTakeModifier : IMaxTakeModifier
    {
        public MaxTakeModifier(float priority, Action<ShovelMaxTakeModification> modifierFunction)
        {
            Priority = priority;
            ModifierFunction = modifierFunction ?? throw new ArgumentNullException(nameof(modifierFunction));
        }

        public float Priority { get; }
        private Action<ShovelMaxTakeModification> ModifierFunction { get; }
        public void ModifyMaxTake(ShovelMaxTakeModification modification)
        {
            ModifierFunction(modification);
        }
    }

}
