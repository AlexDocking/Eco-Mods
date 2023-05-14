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
using Eco.Gameplay.Players;

namespace XPBenefits
{
    public partial class XPConfig
    {
        private static XPConfig singleton;
        public static XPConfig Obj
        {
            get
            {
                singleton ??= new XPConfig();
                return singleton;
            }
        }

        /// <summary>
        /// What to subtract from the player's food xp before doing the calculation
        /// </summary>
        /// <remarks>
        /// Since v9.6 of Eco players get a little food XP regardless of stomach contents.
        /// If your server started pre-9.6 you'll see 'Base Multiplier' in the stomach tooltip
        /// instead of 'Base Gain', in which case you should set this to zero
        /// </remarks>
        public float DefaultBaseFoodXP { get; set; }
        public virtual float BaseFoodXP => DefaultBaseFoodXP * DifficultySettings.SkillGainMultiplier;
        /// <summary>
        /// The value before the server's skill gain setting is applied
        /// </summary>
        public float DefaultMaximumFoodXP { get; set; }
        public virtual float AdjustedMaximumFoodXP => DefaultMaximumFoodXP * DifficultySettings.SkillGainMultiplier - BaseFoodXP;
        /// <summary>
        /// The value before the server's skill gain setting is applied
        /// </summary>
        public float DefaultMaximumHousingXP { get; set; }
        public virtual float AdjustedMaximumHousingXP => DefaultMaximumHousingXP * DifficultySettings.SkillGainMultiplier;

        public virtual IBenefitFunction BenefitFunction { get; protected set; }
        public XPConfig()
        {
            DefaultBaseFoodXP = 12;
            DefaultMaximumFoodXP = 120;
            DefaultMaximumHousingXP = 200;
            ModsOverrideConfig();
        }
        partial void ModsOverrideConfig();
    }
}