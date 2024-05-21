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
using Eco.Gameplay.Players;

namespace XPBenefits
{
    public interface IUserBenefit
    {
        /// <summary>
        /// Whether the benefit is enabled on the server.
        /// It should be determined before the XP Benefits plugin initializes.
        /// Ecopedia pages won't be generated for disabled benefits.
        /// </summary>
        bool Enabled { get; }
        public void ApplyBenefitToUser(User user);
        public void RemoveBenefitFromUser(User user);
        /// <summary>
        /// Called during the XP Benefits plugin Initialize.
        /// Can be used for any setup that can't be done until the XP Benefits config and the available calculation types are loaded.
        /// </summary>
        void OnPluginLoaded();
    }
}
