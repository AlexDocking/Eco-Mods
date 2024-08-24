
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;

namespace XPBenefits
{
    public sealed class SkillRateUtil
    {
        public static float FoodXP(User user)
        {
            return user.Stomach.NutrientSkillRate();
        }
        public static float AdjustFoodXP(float foodXP, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            foodXP = Math.Max(0, foodXP - xpConfig.BaseFoodXP);
            if (xpLimitEnabled)
            {
                foodXP = Math.Min(foodXP, xpConfig.AdjustedMaximumFoodXP);
            }
            return foodXP;
        }
        public static float AdjustedFoodXP(User user, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            return AdjustFoodXP(FoodXP(user), xpConfig, xpLimitEnabled);
        }
        public static float HousingXP(User user)
        {
            return (user.ResidencyPropertyValue?.Value).GetValueOrDefault();
        }
        public static float AdjustHousingXP(float housingXP, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            float houseXP = Math.Max(0, housingXP);
            if (xpLimitEnabled)
            {
                houseXP = Math.Min(houseXP, xpConfig.AdjustedMaximumHousingXP);
            }
            return houseXP;
        }
        public static float AdjustedHousingXP(User user, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            return AdjustHousingXP(HousingXP(user), xpConfig, xpLimitEnabled);
        }
        public static float FractionFoodXP(float foodXP, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            float fractionFoodXP = Math.Max(0, AdjustFoodXP(foodXP, xpConfig, xpLimitEnabled) / xpConfig.AdjustedMaximumFoodXP);
            if (xpLimitEnabled)
            {
                fractionFoodXP = Math.Min(1, fractionFoodXP);
            }
            return fractionFoodXP;
        }
        public static float FractionFoodXP(User user, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            return FractionFoodXP(FoodXP(user), xpConfig, xpLimitEnabled);
        }
        public static float FractionHousingXP(float housingXP, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            housingXP = Math.Max(0, AdjustHousingXP(housingXP, xpConfig) / xpConfig.AdjustedMaximumHousingXP);
            if (xpLimitEnabled)
            {
                housingXP = Math.Min(1, housingXP);
            }
            return housingXP;
        }
        public static float FractionHousingXP(User user, XPConfig xpConfig, bool xpLimitEnabled = false)
        {
            return FractionHousingXP(HousingXP(user), xpConfig, xpLimitEnabled);
        }
    }
}
