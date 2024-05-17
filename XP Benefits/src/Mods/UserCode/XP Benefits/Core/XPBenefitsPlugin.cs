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
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    /// <summary>
    /// Give bonuses to players' stats if they have high food and housing xp.
    /// The benefit amount will reach the target maximum when the skill rate is equal to the maximum skill rate.
    /// They are not strict limits since the actual xp rates a player attains could be greater than you specify as the maximum.
    /// The food xp is adjusted to account for the base rate xp that you get with an empty stomach, so no benefit is given on an empty stomach.
    /// Multipliers from other sources should still apply (if there are any)
    /// </summary>
    [Priority(PriorityAttribute.Normal + 2)]
    public partial class XPBenefitsPlugin : Singleton<XPBenefitsPlugin>, IConfigurablePlugin, IModKitPlugin, IInitializablePlugin
    {
        public XPConfig Config => Obj.GetEditObject() as XPConfig;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<XPConfig> config;

        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();
 
        public XPBenefitsPlugin()
        {
            this.config = new PluginConfig<XPConfig>("XPBenefits");
            Log.WriteLine(Localizer.Do($"Config:{Config.BaseFoodXP}"));
        }

        public string GetCategory() => Localizer.DoStr("Mods");
        public override string ToString() => Localizer.DoStr("XP Benefits");
        public object GetEditObject() => this.config.Config;
        public void OnEditObjectChanged(object o, string param)
        {
            this.SaveConfig();
        }
        public string GetStatus() => Benefits.Any() ? "Loaded Benefits:" + string.Concat(EnabledBenefits.Select(benefit => " " + benefit.GetType().Name)) : "No benefits loaded";
        private static IList<IUserBenefit> LoadedBenefits { get; } = new List<IUserBenefit>();
        /// <summary>
        /// List of loaded benefits. Benefits that are disabled when the plugin loads aren't included.
        /// </summary>
        public IList<IUserBenefit> Benefits => LoadedBenefits;
        public IEnumerable<IUserBenefit> EnabledBenefits => Benefits.Where(benefit => benefit.Enabled);
        private static IList<IBenefitFunctionFactory> LoadedBenefitFunctionFactories { get; } = new List<IBenefitFunctionFactory>();
        public IEnumerable<IBenefitFunctionFactory> CreatableBenefitFunctions => LoadedBenefitFunctionFactories;
        public void Initialize(TimedTask timer)
        {
            Config.AvailableBenefitFunctionTypesDescription = CreatableBenefitFunctions.Select(factory => factory.Name + ": " + Localizer.LocalizeString(factory.Description)).Order().ToList();
            foreach (var benefit in Benefits)
            {
                benefit.OnPluginLoaded();
            }
            ModsChangeBenefits();
            Log.WriteLine(Localizer.DoStr("XP Benefits Status:" + GetStatus()));
        }
        partial void ModsChangeBenefits();
        public static void RegisterBenefit(IUserBenefit benefit) => LoadedBenefits.Add(benefit);
        public static void RegisterBenefitFunctionFactory(IBenefitFunctionFactory benefitFunctionFactory) => LoadedBenefitFunctionFactories.Add(benefitFunctionFactory);
        public T GetBenefit<T>() where T : IUserBenefit
        {
            return (T)GetBenefit(typeof(T));
        }
        public IUserBenefit GetBenefit(Type benefitType)
        {
            return Benefits.FirstOrDefault(benefit => benefit.GetType() == benefitType);
        }
        public string ValidateBenefitFunctionType(string value)
        {
            var validOptions = CreatableBenefitFunctions.Select(factory => factory.Name);
            return validOptions.FirstOrDefault(option => value == option) ?? Localizer.LocalizeString("Invalid");
        }
    }
}