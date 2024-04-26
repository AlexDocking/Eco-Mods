using Eco.Core;
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
    public interface IBenefitEcopediaGenerator : IBenefitEcopedia, IBenefitDescriber
    {
        BenefitBase Benefit { get; }
        EcopediaPage CreateEcopediaPage();
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
        public virtual IEnumerable<LocString> Sections { get; }
        public abstract LocString BenefitDescription { get; }
        public BenefitBase Benefit { get; }

        public BenefitEcopediaGenerator(BenefitBase benefit)
        {
            Benefit = benefit;
        }

        private object pageCreationLock = new object();
        public virtual LocString GenerateEcopediaDescription(User user)
        {
            var locStringBuilder = new LocStringBuilder();
            IBenefitDescriber describer = this;

            locStringBuilder.AppendLineLoc($"{describer.MeansOfImprovingStat(user)}.");
            locStringBuilder.AppendLine();
            if (Benefit.XPLimitEnabled)
            {
                locStringBuilder.AppendLineLoc($"With {describer.MaximumInput(user)} you would receive {describer.MaximumBenefit(user)} {BenefitDescription}. Note that the benefit will be capped at this.");
            }
            else
            {
                locStringBuilder.AppendLineLoc($"With {describer.MaximumInput(user)} you would receive {describer.MaximumBenefit(user)} {BenefitDescription}. Note that the benefit will not be capped at this if you can do even better.");
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

        public abstract LocString CurrentBenefit(User user);
        public abstract LocString CurrentInput(User user);
        public abstract LocString CurrentBenefitEcopedia(User user);
        public abstract LocString InputName(User user);
        public abstract LocString MaximumBenefit(User user);
        public abstract LocString MaximumInput(User user);
        public abstract LocString MeansOfImprovingStat(User user);
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
