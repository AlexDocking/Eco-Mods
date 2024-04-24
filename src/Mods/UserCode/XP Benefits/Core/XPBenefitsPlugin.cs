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
using Eco.Core;
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XPBenefits
{
    /// <summary>
    /// Give bonuses to players' stats if they have high food and housing xp.
    /// The benefit amount will reach the target maximum when the skill rate is equal to the maximum skill rate.
    /// They are not strict limits since the actual xp rates a player attains could be greater than you specify as the maximum.
    /// The food xp is adjusted to account for the base rate xp that you get with an empty stomach, so no benefit is given on an empty stomach.
    /// Multipliers from other sources should still apply (if there are any)
    /// </summary>
    public partial class XPBenefitsPlugin : Singleton<XPBenefitsPlugin>, IConfigurablePlugin, IModKitPlugin, IInitializablePlugin
    {
        public XPConfig Config => Obj.GetEditObject() as XPConfig;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<XPConfig> config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public XPBenefitsPlugin()
        {
            this.config = new PluginConfig<XPConfig>("XPBenefits");
        }

        public string GetCategory() => Localizer.DoStr("Mods");
        public override string ToString() => Localizer.DoStr("XP Benefits");
        public object GetEditObject() => this.config.Config;
        public void OnEditObjectChanged(object o, string param)
        {
            this.SaveConfig();
        }
        public string GetStatus() => Benefits.Any() ? "Loaded Benefits:" + string.Concat(Benefits.Select(benefit => " " + benefit.GetType().Name)) : "No benefits loaded";
        /// <summary>
        /// List of loaded benefits. Benefits that are disabled when the plugin loads aren't included.
        /// </summary>
        public IList<ILoggedInBenefit> Benefits { get; } = new List<ILoggedInBenefit>();
        public IEnumerable<ILoggedInBenefit> EnabledBenefits => Benefits.Where(benefit => benefit.Enabled);
        public void Initialize(TimedTask timer)
        {
            DiscoverILoggedInBenefits();
            ModsChangeBenefits();
            foreach (var benefit in Benefits)
            {
                benefit.OnPluginLoaded();
            }
            UserManager.OnUserLoggedIn.Add(OnUserLoggedIn);
            UserManager.OnUserLoggedOut.Add(OnUserLoggedOut);
            Log.WriteLine(Localizer.DoStr("XP Benefits Status:" + GetStatus()));

            PluginManager.Controller.RunIfOrWhenInited(() => OnServerStart(Benefits));
        }
        private EcopediaCategory EcopediaXPBenefitsCategory => Ecopedia.Obj.Chapters["Mods"].Categories.FirstOrDefault(category => category.Name == "XP Benefits");
        private EcopediaPage EcopediaXPBenefitsOverviewPage => EcopediaXPBenefitsCategory.Pages["XP Benefits Overview"];

        private void OnServerStart(IEnumerable<ILoggedInBenefit> benefits)
        {
            foreach(var benefit in benefits)
            {
                benefit.OnServerStart();
            }
            AmendEcopedia(benefits);
        }
        private void AmendEcopedia(IEnumerable<ILoggedInBenefit> benefits)
        {
            StringBuilder ecopediaBenefitsListBuilder = new StringBuilder();
            ecopediaBenefitsListBuilder.AppendLine(Text.Style(Text.Styles.Header, "List of Benefits:"));
            var pages = Benefits.SelectNonNull(benefit => benefit.BenefitEcopedia.GetPage()).OrderBy(page => page.Priority);
            foreach (var page in pages)
            {
                ecopediaBenefitsListBuilder.AppendLine(page.UILink());
            }
            LocString ecopediaPageList = ecopediaBenefitsListBuilder.ToStringLoc();
            if (benefits.Any(benefit => benefit.Enabled))
            {
                var section = new Eco.Gameplay.EcopediaRoot.EcopediaSection();
                section.Text = ecopediaPageList;
                EcopediaXPBenefitsOverviewPage.Sections.Insert(1, section);
                EcopediaXPBenefitsOverviewPage.ParseTagsInText();
            }
            else
            {
                Ecopedia.Obj.Chapters["Mods"].Categories.Remove(EcopediaXPBenefitsCategory);
            }
        }
        partial void ModsChangeBenefits();

        private void DiscoverILoggedInBenefits()
        {
            var types = typeof(ILoggedInBenefit).CreatableTypes();
            foreach (var type in types)
            {
                LoadBenefitType(type);
            }
        }
        private ILoggedInBenefit LoadBenefitType(Type benefitType)
        {
            if (benefitType == null) { return null; }
            TypeInfo typeInfo = benefitType.GetTypeInfo();
            var constructor = typeInfo.DeclaredConstructors.FirstOrDefault(constructor => constructor.GetParameters().Length == 0);
            var benefit = (ILoggedInBenefit)constructor?.Invoke(null);
            Benefits.Add(benefit);
            return benefit;
        }
        public T GetBenefit<T>() where T : ILoggedInBenefit
        {
            return (T)GetBenefit(typeof(T));
        }
        public ILoggedInBenefit GetBenefit(Type benefitType)
        {
            return Benefits.FirstOrDefault(benefit => benefit.GetType() == benefitType);
        }
        private void OnUserLoggedIn(User user)
        {
            foreach(ILoggedInBenefit benefit in EnabledBenefits)
            {
                benefit.ApplyBenefitToUser(user);
            }
        }
        private void OnUserLoggedOut(User user)
        {
            foreach(ILoggedInBenefit benefit in EnabledBenefits)
            {
                benefit.RemoveBenefitFromUser(user);
            }
        }
    }
}