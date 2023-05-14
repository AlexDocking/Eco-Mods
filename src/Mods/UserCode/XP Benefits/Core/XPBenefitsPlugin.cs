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
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XPBenefits
{
    /// <summary>
    /// Give bonuses to players' stats if they have high food and housing xp.
    /// The benefit amount will reach the target maximum when the skill rate is equal to the maximum skill rate.
    /// They are not strict limits since the actual xp rates a player attains could be greater than you specify as the maximum.
    /// The food xp is adjusted to account for the base rate xp that you get with an empty stomach, so no benefit is given on an empty stomach.
    /// Multipliers from other sources should still apply (if there are any)
    /// </summary>
    public partial class XPBenefitsPlugin : IModKitPlugin, IInitializablePlugin
    {
        public string GetCategory() => "Mod";

        public string GetStatus() => (Benefits.Any() ? "Loaded Benefits:" + string.Concat(Benefits.Select(benefit => " " + benefit.GetType().Name)) : "No benefits loaded");

        public List<ILoggedInBenefit> Benefits { get; } = new List<ILoggedInBenefit>();
        public void Initialize(TimedTask timer)
        {
            Benefits.AddRange(DiscoverILoggedInBenefits());
            foreach(ILoggedInBenefit benefit in Benefits)
            {
                Log.WriteLine(Localizer.DoStr(benefit.GetType().Name));
            }
            ModsChangeBenefits();
            UserManager.OnUserLoggedIn.Add(OnUserLoggedIn);
            UserManager.OnUserLoggedOut.Add(OnUserLoggedOut);
        }
        partial void ModsChangeBenefits();

        private IEnumerable<ILoggedInBenefit> DiscoverILoggedInBenefits()
        {
            var types = Assembly.GetExecutingAssembly().DefinedTypes.Where(type => type.IsAssignableTo(typeof(ILoggedInBenefit)) && Attribute.GetCustomAttributes(type).Any(attribute => attribute is BenefitAttribute));
            var constructors = types.Select(type => type.DeclaredConstructors.FirstOrDefault(constructor => constructor.GetParameters().Length == 0)).NonNull();
            var classes = constructors.Select(c => c.Invoke(null)).NonNull();
            return classes.OfType<ILoggedInBenefit>();
        }
        private void OnUserLoggedIn(User user)
        {
            foreach(ILoggedInBenefit benefit in Benefits)
            {
                benefit.ApplyBenefitToUser(user);
            }
        }
        private void OnUserLoggedOut(User user)
        {
            foreach(ILoggedInBenefit benefit in Benefits)
            {
                benefit.RemoveBenefitFromUser(user);
            }
        }
    }
}