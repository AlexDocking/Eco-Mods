using Eco.Core.Plugins.Interfaces;

namespace XPBenefits
{
    public class XPBenefitsModRegistration : IModInit
    {
       public static ModRegistration Register() => new() 
       { 
           ModName = "XPBenefits",
           ModDescription = "Be rewarded for maintaining a high xp rate with various quality-of-life perks",
           ModDisplayName = "XP Benefits",
       };
    }
}
