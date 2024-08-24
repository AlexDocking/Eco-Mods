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
using Eco.Shared.Networking;
using System.ComponentModel;

namespace Ecompatible
{
    public partial class EcompatibleAxeConfig : IController
    {
        [Category("Settings"), LocDescription("How much of the tree should be automatically sliced up."), Range(0, 1)]
        public float FractionOfTreeToAutoSlice { get; set; } = 0;
        [Category("Settings"), LocDescription("How much damage should be dealt to the stump."), Range(0, float.MaxValue)]
        public float DamageToStumpWhenFelled { get; set; } = 0;
        [Category("Settings"), LocDescription("What is the maximum number of debris to spawn per tree."), Range(0, float.MaxValue)]
        public int MaxTreeDebrisToSpawn { get; set; } = 20;
        [Category("Settings"), LocDescription("What is the chance for each debris to get automatically cleared."), Range(0, 1)]
        public float ChanceToClearDebrisOnSpawn { get; set; } = 0;

        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
}