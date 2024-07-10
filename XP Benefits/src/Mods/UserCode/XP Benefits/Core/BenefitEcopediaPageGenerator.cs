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
using Eco.Core.Systems;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    public static class BenefitEcopediaExtensions
    {
        public static EcopediaPage GetPage(this IBenefitEcopedia ecopedia)
        {
            if (string.IsNullOrEmpty(ecopedia?.PageName)) return null;
            return Ecopedia.Obj.Categories.Values.SelectMany(category => category.Pages.Values).FirstOrDefault(page => page.Name == ecopedia.PageName);
        }
        public static EcopediaPage GetOrCreatePage(this IBenefitEcopediaGenerator ecopediaGenerator)
        {
            return ecopediaGenerator?.GetPage() ?? ecopediaGenerator?.CreateEcopediaPage();
        }
        public static void RemovePage(this IBenefitEcopedia benefitEcopedia)
        {
            EcopediaPage page = benefitEcopedia?.GetPage();
            Ecopedia.Obj.RemovePage(page);
        }
        public static LocString GetPageLink(this IBenefitEcopedia ecopedia, LocString content = default)
        {
            if (content != default) return ecopedia?.GetPage()?.UILink(content) ?? LocString.Empty;
            return ecopedia?.GetPage()?.UILink() ?? LocString.Empty;
        }
        public static void AddPage(this Ecopedia ecopedia, string categoryName, EcopediaPage page)
        {
            ecopedia.Categories[categoryName].Pages.Add(page.Name, page);
        }
        public static void RemovePage(this Ecopedia ecopedia, EcopediaPage page)
        {
            if (page == null) return;
            foreach (EcopediaCategory category in ecopedia.Categories.Values)
            {
                category.Pages.RemoveWhere((_, existingPage) => page == existingPage);
            }
        }
    }
    public interface IBenefitEcopedia
    {
        string PageName { get; }
    }
    public interface IBenefitEcopediaGenerator : IBenefitEcopedia
    {
        BenefitBase Benefit { get; }
        EcopediaPage CreateEcopediaPage();
        /// <summary>
        /// The information about a benefit which is user-specific or is dependent on mod settings. It will be added to the bottom of the Ecopedia page.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        LocString GenerateEcopediaDescription(User user);
    }
    public abstract class BenefitEcopediaGenerator : IBenefitEcopediaGenerator
    {
        public virtual string CategoryName { get; } = "XP Benefits";
        public abstract string PageName { get; }
        public abstract float PagePriority { get; }
        public abstract LocString DisplayName { get; }
        public abstract string Summary { get; }
        public virtual string IconName { get; }
        public virtual IEnumerable<LocString> Sections { get; } = Enumerable.Empty<LocString>();
        public abstract LocString BenefitDescription { get; }
        public BenefitBase Benefit { get; }
        public IBenefitDescriber BenefitDescriber { get; }
        public BenefitEcopediaGenerator(BenefitBase benefit, IBenefitDescriber benefitDescriber)
        {
            Benefit = benefit;
            BenefitDescriber = benefitDescriber;
        }

        private object pageCreationLock = new object();
        public virtual LocString GenerateEcopediaDescription(User user)
        {
            var locStringBuilder = new LocStringBuilder();
            IBenefitDescriber describer = BenefitDescriber;

            locStringBuilder.AppendLineLoc($"{describer.MeansOfImprovingStat(user)}");
            locStringBuilder.AppendLine();
            var note = Benefit.XPLimitEnabled ? "Note that the benefit will be capped at this." : "Note that the benefit will not be capped at this if you can do even better.";
            if (Benefit.XPLimitEnabled)
            {
                locStringBuilder.AppendLineLoc($"With {describer.MaximumInput(user)} you would receive {describer.MaximumBenefit(user)} {BenefitDescription}. {note}");
            }
            else
            {
                locStringBuilder.AppendLineLoc($"With {describer.MaximumInput(user)} you would receive {describer.MaximumBenefit(user)} {BenefitDescription}. {note}");
            }
            locStringBuilder.AppendLine();
            locStringBuilder.AppendLine(TextLoc.HeaderLoc($"Current Status"));
            locStringBuilder.AppendLineLoc($"You have {describer.CurrentInput(user)}, which is providing you with {describer.CurrentBenefitEcopedia(user)} {BenefitDescription}.");

            return locStringBuilder.ToLocString();
        }
        public EcopediaPage CreateEcopediaPage()
        {
            if (Benefit == null) { return null; }
            lock (pageCreationLock)
            {
                Dictionary<string, EcopediaPage> xpBenefitPages = Ecopedia.Obj.Categories["XP Benefits"].Pages;
                if (xpBenefitPages.TryGetValue(PageName, out var existingPage))
                {
                    return existingPage;
                }
                var page = UnserializedNamedEntry<EcopediaPage>.GetByName(PageName);
                if (page == null)
                {
                    page = new EcopediaPage();
                    page.Name = PageName;
                    page.Priority = PagePriority;
                    page.DisplayName = DisplayName;
                    page.Summary = Summary;
                    page.FullName = CategoryName + ";" + PageName;
                    page.IconName = IconName;

                    foreach (LocString sectionText in Sections)
                    {
                        var section = new Eco.Gameplay.EcopediaRoot.EcopediaSection();
                        section.Text = sectionText;
                        page.Sections.Add(section);
                    }
                    page.AddGeneratedData(new BenefitEcopediaGeneratedData(this));
                    page.HasGeneratedData = true;
                    page.ParseTagsInText();
                }
                Ecopedia.Obj.AddPage(CategoryName, page);
                return page;
            }
        }
    }
 
    //Cannot be abstract or generic due to how they are loaded by the game
    public class BenefitEcopediaGeneratedData : IEcopediaGeneratedData
    {
        public IBenefitEcopediaGenerator EcopediaGenerator { get; }
        public BenefitEcopediaGeneratedData(IBenefitEcopediaGenerator ecopediaGenerator)
        {
            EcopediaGenerator = ecopediaGenerator;
        }

        public BenefitEcopediaGeneratedData()
        {
        }

        public virtual LocString GetEcopediaData(Player player, EcopediaPage page)
        {
            if (EcopediaGenerator == null) return LocString.Empty;
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            locStringBuilder.AppendLine(EcopediaGenerator.GenerateEcopediaDescription(player.User));
            return locStringBuilder.ToLocString();
        }
        public IEnumerable<EcopediaPageReference> PagesWeSupplyDataFor()
        {
            return Array.Empty<EcopediaPageReference>();
        }
    }
}
