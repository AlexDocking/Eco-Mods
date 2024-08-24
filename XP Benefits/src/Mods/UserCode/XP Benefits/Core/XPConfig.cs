using Eco.Core.Controller;
using Eco.Gameplay.Systems.Balance;
using Eco.Shared.Localization;
using System.Collections.Generic;
using System.ComponentModel;

namespace XPBenefits
{
    public partial class XPConfig : IController
    {
        [Category("Shared Settings"), LocDescription("Only if the server began before Eco v0.9.6 do you need this setting.\nWhat to subtract from the player's food xp before doing the calculation.\nSince v0.9.6 players get a little food XP regardless of stomach contents.\nIf your server started pre-9.6 you'll see 'Base Multiplier' in the stomach tooltip instead of 'Base Gain', in which case you should set this to zero.")]
        public float DefaultBaseFoodXP { get; set; } = 12;
        [Browsable(false)]
        public virtual float BaseFoodXP => DefaultBaseFoodXP * BalancePlugin.Obj.Config.SkillGainMultiplier;
        [Category("Shared Settings"), LocDescription("Players' food XP is scaled using this when calculating how much reward to give. If players reach this value they will get the full reward. This is the value before the server's skill gain setting is applied.")]
        public float DefaultMaximumFoodXP { get; set; } = 120;
        [Browsable(false)]
        public virtual float AdjustedMaximumFoodXP => DefaultMaximumFoodXP * BalancePlugin.Obj.Config.SkillGainMultiplier - BaseFoodXP;
        [Browsable(false)]
        public virtual float MaximumFoodXP => DefaultMaximumFoodXP * BalancePlugin.Obj.Config.SkillGainMultiplier;
        [Category("Shared Settings"), LocDescription("Players' housing XP is scaled using this when calculating how much reward to give. If players reach this value they will get the full reward. This is the value before the server's skill gain setting is applied.")]
        public float DefaultMaximumHousingXP { get; set; } = 200;
        [Browsable(false)]
        public virtual float AdjustedMaximumHousingXP => DefaultMaximumHousingXP * BalancePlugin.Obj.Config.SkillGainMultiplier;
        [Browsable(false)]
        public virtual float MaximumHousingXP => DefaultMaximumHousingXP * BalancePlugin.Obj.Config.SkillGainMultiplier;
        [Category("Shared Settings"), LocDisplayName("List of Available Benefit Function Types"), LocDescription("The different ways a benefit can be calculated, and each benefit can have a different means of calculation.")]
        [ReadOnly(true)]
        public List<string> AvailableBenefitFunctionTypesDescription { get; set; }
        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
}