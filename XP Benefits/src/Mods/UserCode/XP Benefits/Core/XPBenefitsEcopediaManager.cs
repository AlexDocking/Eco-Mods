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
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPBenefits
{
    public class XPBenefitsEcopediaManager : AutoSingleton<XPBenefitsEcopediaManager>
    {
        public XPBenefitsEcopediaManager()
        {
            PluginManager.Controller.RunIfOrWhenInited(AmendEcopedia);
        }
        private Dictionary<IUserBenefit, IBenefitEcopediaGenerator> EcopediaGenerators { get; } = new Dictionary<IUserBenefit, IBenefitEcopediaGenerator>();
        public IBenefitEcopediaGenerator GetEcopedia(IUserBenefit userBenefit) => EcopediaGenerators.TryGetValue(userBenefit, out var generator) ? generator : null;
        public void RegisterEcopediaPageGenerator(IUserBenefit benefit, IBenefitEcopediaGenerator ecopediaGenerator)
        {
            EcopediaGenerators.Add(benefit, ecopediaGenerator);
        }
        private void AmendEcopedia()
        {
            foreach(IUserBenefit benefit in EcopediaGenerators.Keys)
            {
                if (!benefit.Enabled)
                {
                    EcopediaGenerators[benefit].RemovePage();
                }
                else
                {
                    EcopediaGenerators[benefit].GetOrCreatePage();
                }
            }
            var benefits = XPBenefitsPlugin.Obj.Benefits;
            StringBuilder ecopediaBenefitsListBuilder = new StringBuilder();
            ecopediaBenefitsListBuilder.AppendLine(Text.Style(Text.Styles.Header, "List of Benefits:"));
            
            var pages = EcopediaGenerators.Values.Select(generator => generator.GetPage());
            foreach (var benefit in XPBenefitsPlugin.Obj.EnabledBenefits)
            {
                string pageLink = EcopediaGenerators.TryGetValue(benefit, out var generator) ? generator.GetPage().UILink() : benefit.GetType().Name.AddSpacesBetweenCapitals();
                ecopediaBenefitsListBuilder.AppendLine(pageLink);
            }
            LocString ecopediaPageList = ecopediaBenefitsListBuilder.ToStringLoc();
            
            if (XPBenefitsPlugin.Obj.EnabledBenefits.Any())
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

        private EcopediaCategory EcopediaXPBenefitsCategory => Ecopedia.Obj.Chapters["Mods"].Categories.FirstOrDefault(category => category.Name == "XP Benefits");
        private EcopediaPage EcopediaXPBenefitsOverviewPage => EcopediaXPBenefitsCategory.Pages["XP Benefits Overview"];
    }
}
