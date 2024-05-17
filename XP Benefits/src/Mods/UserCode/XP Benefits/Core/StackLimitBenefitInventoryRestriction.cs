//XP Benefits
//Copyright (C) 2023 Alex Docking
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public class StackLimitBenefitInventoryRestriction : InventoryRestriction
    {
        public virtual IBenefitFunction BenefitFunction { get; }
        public StackLimitBenefitInventoryRestriction(User user, IBenefitFunction benefitFunction)
        {
            User = user;
            BenefitFunction = benefitFunction;
        }

        public override int MaxAccepted(Item item, int currentQuantity)
        {
            return (int)(item.MaxStackSize * GetStackLimitMultiplier());
        }

        private float GetStackLimitMultiplier()
        {
            float stackSizeBenefit = BenefitFunction.CalculateBenefit(User);
            return (1 + stackSizeBenefit);
        }

        public User User { get; }
        public BenefitValue MaximumBenefit { get; }

        public override bool SurpassStackSize => true;
        public override LocString Message => new LocString();
    }
}