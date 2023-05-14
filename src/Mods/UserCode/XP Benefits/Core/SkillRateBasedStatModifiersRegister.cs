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
using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using Eco.Shared.View;
using System;
using System.Collections.Generic;

namespace XPBenefits
{
    public class SkillRateBasedStatModifiersRegister
    {
        /// <summary>
        /// Stores the skill rate change listeners
        /// </summary>;
        private IDictionary<(User, UserStatType), Action> SkillRateListeners { get; } = new ThreadSafeDictionary<(User, UserStatType), Action>();

        /// <summary>
        /// Add the benefit amount to the current stat in the form of an IDynamicValue
        /// </summary>
        /// <param name="user"></param>
        /// <param name="statType"></param>
        /// <param name="benefit"></param>
        /// <param name="callbackToUpdateStat">If not null, watch the skill rate property and call the callback to update the stat when the skill rate changes</param>
        public virtual void AddModifierToUser(User user, UserStatType statType, IDynamicValue benefit, Action callbackToUpdateStat)
        {
            UserStat stat = user.ModifiedStats.GetStat(statType);
            stat.ModifierSkill = new MultiDynamicValue(MultiDynamicOps.Sum, stat.ModifierSkill, benefit);

            //Eco will wait for the skill rate to change, then the callback will force the game to recalculate the stat
            user.UserXP.Subscribe("SkillRate", callbackToUpdateStat);
            SkillRateListeners.Add((user, statType), callbackToUpdateStat);
            callbackToUpdateStat();
        }

        public virtual void RemoveStatModifiersFromUser(User user, UserStatType statType)
        {
            UserStat stat = user.ModifiedStats.GetStat(statType);
            MultiDynamicValue modification = stat.ModifierSkill as MultiDynamicValue;
            if (modification == null)
            {
                Log.WriteErrorLineLocStr($"Could not remove {statType} modifier from player: {user.Name}");
                return;
            }
            stat.ModifierSkill = modification.Values[0];
            if (SkillRateListeners.Remove((user, statType), out Action callbackToUpdateStat))
            {
                user.UserXP.Unsubscribe("SkillRate", callbackToUpdateStat);
            }
        }
    }
}