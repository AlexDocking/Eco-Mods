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
using Eco.Core.Controller;
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.ComponentModel;

namespace CompatibleTools
{
    public partial class CompatibleToolsConfig : IController
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
    
    public partial class CompatibleToolsPlugin : Singleton<CompatibleToolsPlugin>, IConfigurablePlugin, IModKitPlugin, IModInit
    {
        public CompatibleToolsConfig Config => Obj.GetEditObject() as CompatibleToolsConfig;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<CompatibleToolsConfig> config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public CompatibleToolsPlugin()
        {
            this.config = new PluginConfig<CompatibleToolsConfig>("BigShovel");
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
            ShovelItem.MaxTakeResolver.Add(new InitialShovelSizeModifier());
            ShovelItem.MaxTakeResolver.Add(new ShovelStackSizeModifierSettingModifier());
        }
    }
    public class InitialShovelSizeModifier : IPriorityModifyInPlaceDynamicValueHandler
    {
        public float Priority { get; } = float.MinValue;
        public void ModifyValue(IModifyInPlaceDynamicValueContext context)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            switch (shovelContext.Shovel)
            {
                case WoodenShovelItem:
                    shovelContext.FloatValue = CompatibleToolsPlugin.Obj.Config.WoodenShovelBaseSize;
                    shovelContext.IntValue = CompatibleToolsPlugin.Obj.Config.WoodenShovelBaseSize;
                    break;
                case IronShovelItem:
                    shovelContext.FloatValue = CompatibleToolsPlugin.Obj.Config.IronShovelBaseSize;
                    shovelContext.IntValue = CompatibleToolsPlugin.Obj.Config.IronShovelBaseSize;
                    break;
                case SteelShovelItem:
                    shovelContext.FloatValue = CompatibleToolsPlugin.Obj.Config.SteelShovelBaseSize;
                    shovelContext.IntValue = CompatibleToolsPlugin.Obj.Config.SteelShovelBaseSize;
                    break;
                case ModernShovelItem:
                    shovelContext.FloatValue = CompatibleToolsPlugin.Obj.Config.ModernShovelBaseSize;
                    shovelContext.IntValue = CompatibleToolsPlugin.Obj.Config.ModernShovelBaseSize;
                    break;
            }
        }
    }
    public class ShovelStackSizeModifierSettingModifier : IPriorityModifyInPlaceDynamicValueHandler
    {
        public float Priority { get; } = -100;
        public void ModifyValue(IModifyInPlaceDynamicValueContext context)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            if (CompatibleToolsPlugin.Obj.Config.ApplyStackSizeModifier)
            {
                shovelContext.FloatValue *= DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier;
                shovelContext.IntValue = (int)shovelContext.FloatValue;
            }
        }
    }
}