using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;

namespace XPBenefits
{
    internal static class EcopediaExtensions
    {
        public static LocString UILinkPage(this Ecopedia ecopedia, string pageName)
        {
            EcopediaPage page = ecopedia.GetPage(pageName);
            if (page != null) return page.UILink();
            return Localizer.DoStr($"<Missing Ecopedia page \"{pageName}\">");
        }
        public static LocString UILinkPageWithContent(this Ecopedia ecopedia, string pageName, LocString content)
        {
            EcopediaPage page = ecopedia.GetPage(pageName);
            if (page != null) return page.UILinkContent(content);
            return Localizer.DoStr($"<Missing Ecopedia page \"{pageName}\">");
        }
    }
}