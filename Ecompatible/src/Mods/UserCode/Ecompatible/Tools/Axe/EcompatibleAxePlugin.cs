// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    public sealed class EcompatibleAxePlugin : Singleton<EcompatibleAxePlugin>, IConfigurablePlugin, IModKitPlugin, IModInit
    {
        public EcompatibleAxeConfig Config => Obj.GetEditObject() as EcompatibleAxeConfig;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<EcompatibleAxeConfig> config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public EcompatibleAxePlugin()
        {
            this.config = new PluginConfig<EcompatibleAxeConfig>("EcompatibleAxe");
        }

        public string GetCategory() => Localizer.DoStr("Mods");
        public override string ToString() => Localizer.DoStr("Ecompatible Axe");
        public object GetEditObject() => this.config.Config;
        public void OnEditObjectChanged(object o, string param)
        {
            this.SaveConfig();
        }
        public string GetStatus() => "";
        public static void Initialize()
        {
            ValueResolvers.Tools.Axe.FractionOfTreeToAutoSlice.Add(float.MinValue, new ConfigurableFractionOfTreeToAutoSliceModifier());
            ValueResolvers.Tools.Axe.DamageToStumpWhenFelled.Add(float.MinValue, new ConfigurableDamageToStumpWhenFelledModifier());
            ValueResolvers.Tools.Axe.MaxTreeDebrisToSpawn.Add(float.MinValue, new ConfigurableMaxTreeDebrisToSpawnModifier());
            ValueResolvers.Tools.Axe.ChanceToClearDebrisOnSpawn.Add(float.MinValue, new ConfigurableChanceToClearDebrisOnSpawnModifier());
        }
    }
    internal class ConfigurableFractionOfTreeToAutoSliceModifier : IValueModifier<float, ITreeFelledContext>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, ITreeFelledContext> functionInput)
        {
            return OutputFactory.BaseLevel(EcompatibleAxePlugin.Obj.Config.FractionOfTreeToAutoSlice);
        }
    }
    internal class ConfigurableDamageToStumpWhenFelledModifier : IValueModifier<float, ITreeFelledContext>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, ITreeFelledContext> functionInput)
        {
            return OutputFactory.BaseLevel(EcompatibleAxePlugin.Obj.Config.DamageToStumpWhenFelled);
        }
    }
    internal class ConfigurableMaxTreeDebrisToSpawnModifier : IValueModifier<float, ITreeFelledContext>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, ITreeFelledContext> functionInput)
        {
            return OutputFactory.BaseLevel(EcompatibleAxePlugin.Obj.Config.MaxTreeDebrisToSpawn);
        }
    }
    internal class ConfigurableChanceToClearDebrisOnSpawnModifier : IValueModifier<float, ITreeFelledContext>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, ITreeFelledContext> functionInput)
        {
            return OutputFactory.BaseLevel(EcompatibleAxePlugin.Obj.Config.ChanceToClearDebrisOnSpawn);
        }
    }
}