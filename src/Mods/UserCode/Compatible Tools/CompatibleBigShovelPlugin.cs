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
using Eco.Core;
using Eco.Core.Controller;
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CompatibleTools
{
    public partial class CompatibleToolsPlugin : IController
    {
        [Category("Settings"), LocDescription("How many blocks a wooden shovel can dig before any modifiers are applied.")]
        public int WoodenShovelBaseSize { get; set; } = 1;
        [Category("Settings"), LocDescription("How many blocks an iron shovel can dig before any modifiers are applied.")]
        public int IronShovelBaseSize { get; set; } = 3;
        [Category("Settings"), LocDescription("How many blocks a steel shovel can dig before any modifiers are applied.")]
        public int SteelShovelBaseSize { get; set; } = 5;
        [Category("Settings"), LocDescription("How many blocks a modern shovel can dig before any modifiers are applied.")]
        public int ModernShovelBaseSize { get; set; } = 10;
        [Category("Settings"), LocDescription("Should the \"ItemStackSizeMultiplier\" in the Difficulty config affect how much shovels can dig?")]
        public bool ApplyStackSizeModifier { get; set; } = false;

        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
    public partial class CompatibleBigShovelPlugin : Singleton<CompatibleBigShovelPlugin>, IConfigurablePlugin, IModKitPlugin, IModInit
    {
        public CompatibleToolsPlugin Config => Obj.GetEditObject() as CompatibleToolsPlugin;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<CompatibleToolsPlugin> config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public CompatibleBigShovelPlugin()
        {
            this.config = new PluginConfig<CompatibleToolsPlugin>("BigShovel");
        }

        public string GetCategory() => Localizer.DoStr("Mods");
        public override string ToString() => Localizer.DoStr("Shovel Settings");
        public object GetEditObject() => this.config.Config;
        public void OnEditObjectChanged(object o, string param)
        {
            this.SaveConfig();
        }
        public string GetStatus() => "";

        public static void Initialize()
        {
            ShovelItem.MaxTakeModifiers.Add(new InitialShovelSizeModifier());
            ShovelItem.MaxTakeModifiers.Add(new ShovelStackSizeModifierSettingModifier());
        }
    }
    public class InitialShovelSizeModifier : IMaxTakeModifier
    {
        public float Priority { get; } = float.MinValue;
        public void ModifyMaxTake(ShovelMaxTakeModification modification)
        {
            switch (modification.Shovel)
            {
                case WoodenShovelItem:
                    modification.MaxTake = CompatibleBigShovelPlugin.Obj.Config.WoodenShovelBaseSize;
                    break;
                case IronShovelItem:
                    modification.MaxTake = CompatibleBigShovelPlugin.Obj.Config.IronShovelBaseSize;
                    break;
                case SteelShovelItem:
                    modification.MaxTake = CompatibleBigShovelPlugin.Obj.Config.SteelShovelBaseSize;
                    break;
                case ModernShovelItem:
                    modification.MaxTake = CompatibleBigShovelPlugin.Obj.Config.ModernShovelBaseSize;
                    break;
            }
        }
    }
    public class ShovelStackSizeModifierSettingModifier : IMaxTakeModifier
    {
        public float Priority { get; } = -100;
        public void ModifyMaxTake(ShovelMaxTakeModification modification)
        {
            if (CompatibleBigShovelPlugin.Obj.Config.ApplyStackSizeModifier)
            {
                modification.MaxTake *= DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier;
            }
        }
    }
}