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
using System.Linq;

namespace XPBenefits
{
    public partial class ExtraCarryStackLimitBenefit : BenefitBase
    {
        /// <summary>
        /// Whether the shovel size should be affected by the extra carry limit
        /// </summary>
        public static bool IncreaseShovelSize { get; set; } = true;
        /// <summary>
        /// Whether all shovels should by default take max stack size + (optional) benefit
        /// </summary>
        public static bool AllBigShovels { get; set; } = false;
        /// <summary>
        /// Used by shovels to work out how much their size should increase
        /// </summary>
        public static IBenefitFunction ShovelBenefit { get; set; }
        public ExtraCarryStackLimitBenefit()
        {
            XPConfig = XPConfig.Obj;
            //e.g. 0.5 represents 50 % increase in stack limit for the items held in the hands e.g.carry 30 bricks instead of 20
            MaxBenefitValue = 1f;
            ModsPreInitialize();
            BenefitFunction = new GeometricMeanFoodHousingBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
            ShovelBenefit ??= BenefitFunction;
            ModsPostInitialize();
        }
        partial void ModsPreInitialize();
        partial void ModsPostInitialize();

        public override void ApplyBenefitToUser(User user)
        {
            var carryInventory = user.Inventory.Carried;

            if (!carryInventory.Restrictions.Any(restriction => restriction is StackLimitBenefitInventoryRestriction))
            {
                carryInventory.AddInvRestriction(new StackLimitBenefitInventoryRestriction(user, BenefitFunction));
            }
        }
        public override void RemoveBenefitFromUser(User user)
        {
            Inventory carryInventory = user.Inventory.Carried;
            carryInventory.RemoveAllRestrictions(restriction => restriction is StackLimitBenefitInventoryRestriction);
        }
    }
}
