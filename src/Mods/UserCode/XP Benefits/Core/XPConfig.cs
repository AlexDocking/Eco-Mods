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
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace XPBenefits
{
    public partial class XPConfig : IController
    {
        [Category("Shared Settings"), LocDescription("Only if the server began before Eco v0.9.6 do you need this setting.\nWhat to subtract from the player's food xp before doing the calculation.\nSince v0.9.6 players get a little food XP regardless of stomach contents.\nIf your server started pre-9.6 you'll see 'Base Multiplier' in the stomach tooltip instead of 'Base Gain', in which case you should set this to zero.")]
        public float DefaultBaseFoodXP { get; set; } = 12;
        [Browsable(false)]
        public virtual float BaseFoodXP => DefaultBaseFoodXP * DifficultySettings.SkillGainMultiplier;
        [Category("Shared Settings"), LocDescription("Players' food XP is scaled using this when calculating how much reward to give. If players reach this value they will get the full reward. This is the value before the server's skill gain setting is applied.")]
        public float DefaultMaximumFoodXP { get; set; } = 120;
        [Browsable(false)]
        public virtual float AdjustedMaximumFoodXP => DefaultMaximumFoodXP * DifficultySettings.SkillGainMultiplier - BaseFoodXP;
        [Browsable(false)]
        public virtual float MaximumFoodXP => DefaultMaximumFoodXP * DifficultySettings.SkillGainMultiplier;
        [Category("Shared Settings"), LocDescription("Players' housing XP is scaled using this when calculating how much reward to give. If players reach this value they will get the full reward. This is the value before the server's skill gain setting is applied.")]
        public float DefaultMaximumHousingXP { get; set; } = 200;
        [Browsable(false)]
        public virtual float AdjustedMaximumHousingXP => DefaultMaximumHousingXP * DifficultySettings.SkillGainMultiplier;
        [Browsable(false)]
        public virtual float MaximumHousingXP => DefaultMaximumHousingXP * DifficultySettings.SkillGainMultiplier;
        [Category("Shared Settings"), LocDescription("Available benefit function types.")]
        [JsonIgnore]
        [ReadOnly(true)]
        public List<string> AvailableBenefitFunctionTypesDescription { get; set; }
        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
}