using Eco.Core.Systems;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;

namespace XPBenefits
{
    //Cannot be abstract or generic due to how they are loaded by the game
    public class BenefitEcopediaGenerator : IEcopediaGeneratedData
    {
        public virtual string Category { get; } = "XP Benefits";
        public string PageName => Benefit.EcopediaPageName;
        public float PagePriority => Benefit.EcopediaPagePriority;
        public virtual LocString DisplayName { get; }
        public virtual string Summary { get; }
        public virtual string IconName { get; }
        public virtual IEnumerable<LocString> Sections { get; }
        private BenefitBase Benefit { get; }
        protected virtual Type BenefitType { get; }
        public BenefitEcopediaGenerator()
        {
            Benefit = XPBenefitsPlugin.Obj.GetBenefit(BenefitType) as BenefitBase;
        }
        private EcopediaPage CreateEcopediaPage()
        {
            if (Benefit == null) { return null; }
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
                page.FullName = Category + ";" + PageName;
                page.IconName = IconName;

                foreach (LocString sectionText in Sections)
                {
                    var section = new Eco.Gameplay.EcopediaRoot.EcopediaSection();
                    section.Text = sectionText;
                    page.Sections.Add(section);
                }
                page.ParseTagsInText();
            }
            xpBenefitPages.Add(PageName, page);
            return page;
        }
        public virtual LocString GetEcopediaData(Player player, EcopediaPage page)
        {
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            locStringBuilder.AppendLine(Benefit.GenerateEcopediaDescription(player.User));
            return locStringBuilder.ToLocString();
        }
        public virtual IEnumerable<EcopediaPageReference> PagesWeSupplyDataFor()
        {
            EcopediaPage page = CreateEcopediaPage();
            if (page == null) return Array.Empty<EcopediaPageReference>();
            return new EcopediaPageReference(null, "XP Benefits", page.Name, page.DisplayName).SingleItemAsEnumerable();
        }
    }
}
