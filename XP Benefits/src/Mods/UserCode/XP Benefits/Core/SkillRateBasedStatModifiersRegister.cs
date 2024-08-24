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
            //Rather than add and remove the modifier every log in/out, set it once when they first log in and it will last until the server shuts down
            if (!SkillRateListeners.ContainsKey((user, statType)))
            {
                UserStat stat = user.ModifiedStats.GetStat(statType);
                stat.ModifierSkill = new MultiDynamicValue(MultiDynamicOps.Sum, stat.ModifierSkill, benefit);

                //Eco will wait for the skill rate to change, then the callback will force the game to recalculate the stat
                user.UserXP.Subscribe("SkillRate", callbackToUpdateStat);
                SkillRateListeners.Add((user, statType), callbackToUpdateStat);
                callbackToUpdateStat();
            }
        }

        public virtual void RemoveStatModifiersFromUser(User user, UserStatType statType)
        {
        }
    }
}