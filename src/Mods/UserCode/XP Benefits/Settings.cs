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
namespace XPBenefits
{
    //Here you can configure how the mod is set up, changing things like how much of each type of benefit players can earn
    //Servers starting pre-9.6 should change the XPConfig by uncommenting the ModsOverrideConfig method and setting the DefaultBaseFoodXP
    //Uncomment the sections you want to change
    //Example:
    /*
    [Benefit] //If you want to disable the benefit remove this attribute
    public partial class ExtraCaloriesBenefit
    {
        partial void ModsPreInitialize()
        {
            //Use a different config just for this benefit, where, when combined with enabling the xp limit, players don't need and won't benefit from housing scores above 50 (times server multiplier)
            XPConfig = new XPConfig();
            XPConfig.DefaultMaximumHousingXP = 50;

            //Set the maximum benefit to 3000 calories
            MaxBenefitValue = 3000;
            //Don't reward players for getting even higher xp scores than what the XPConfig says is maximum
            XPLimitEnabled = true;
        }
        partial void ModsPostInitialize()
        {
            //Change the way the calculation is done to only consider the food score and not include housing
            //You can use any of the functions in Core/Benefit Functions, or write your own
            BenefitFunction = new FoodBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
        }
    }
    */
    public partial class XPConfig
    {
        /*partial void ModsOverrideConfig()
        {
            //Set this to zero if your server has 'Base Multiplier' instead of 'Base Gain' in the stomach tooltip,
            //which is probably the case if it started on version pre-9.6
            DefaultBaseFoodXP = ;
            DefaultMaximumFoodXP = ;
            DefaultMaximumHousingXP = ;
        }*/
    }
    [Benefit]
    public partial class ExtraCaloriesBenefit
    {
        /// <summary>
        /// Uncomment to change how much extra calorie space the player can earn
        /// </summary>
        /*partial void ModsPreInitialize()
        {
            //Calories
            MaxBenefitValue = ;
            XPLimitEnabled = ;
        }*/

        /// <summary>
        /// Uncomment to change how the amount of benefit is calculated from a user
        /// </summary>
        /*partial void ModsPostInitialize()
        {
            BenefitFunction = ;
        }*/
    }
    [Benefit]
    public partial class ExtraWeightLimitBenefit
    {
        /// <summary>
        /// Uncomment to change how much extra backpack/toolbar inventory weight limit the player can earn 
        /// </summary>
        /*partial void ModsPreInitialize()
        {
            //Grams
            MaxBenefitValue = ;
            XPLimitEnabled = ;
        }*/
        /// <summary>
        /// Uncomment to change how the amount of benefit is calculated from a user
        /// </summary>
        /*partial void ModsPostInitialize()
        {
            BenefitFunction = ;
        }*/
    }
    [Benefit]
    public partial class ExtraCarryStackLimitBenefit
    {
        /// <summary>
        /// Uncomment to change how much extra carry stack size (hands slot) the player can earn, or if you need to change how shovels work
        /// </summary>
        /*partial void ModsPreInitialize()
        {
            //e.g. 0.5 represents 50% increase in stack limit for the items held in the hands e.g. carry 30 bricks instead of 20
            MaxBenefitValue = ;
            XPLimitEnabled = ;

            //If you don't want this benefit to increase the shovel sizes uncomment this line
            //IncreaseShovelSize = false;

            //If you use All Big Shovels (the one with a single file) you need to uncomment this line to enable the big shovel behaviour
            //as this mod will overwrite the same ShovelItem.override.cs file
            //AllBigShovels = true;
        }*/
        /// <summary>
        /// Uncomment to change how the amount of benefit is calculated from a user
        /// </summary>
        /*partial void ModsPostInitialize()
        {
            BenefitFunction = ;
        }*/
    }
}
