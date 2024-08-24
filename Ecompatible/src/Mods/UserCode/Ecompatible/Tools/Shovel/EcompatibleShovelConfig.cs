// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Core.Controller;
using Eco.Shared.Localization;
using System.ComponentModel;

namespace Ecompatible
{
    public partial class EcompatibleShovelConfig : IController
    {
        [Category("Settings"), LocDescription("How many blocks a wooden shovel can dig before any modifiers are applied.")]
        public int WoodenShovelBaseSize { get; set; } = 1;
        [Category("Settings"), LocDescription("How many blocks an iron shovel can dig before any modifiers are applied.")]
        public int IronShovelBaseSize { get; set; } = 3;
        [Category("Settings"), LocDescription("How many blocks a steel shovel can dig before any modifiers are applied.")]
        public int SteelShovelBaseSize { get; set; } = 5;
        [Category("Settings"), LocDescription("How many blocks a modern shovel can dig before any modifiers are applied.")]
        public int ModernShovelBaseSize { get; set; } = 10;
        [Category("Settings"), LocDescription("Should the \"StackSizeMultiplier\" in the Difficulty config affect how much shovels can dig?")]
        public bool ApplyStackSizeModifier { get; set; } = false;

        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
}