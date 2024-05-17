using Eco.Core.Controller;
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace CompatibleTools
{
    public partial class CompatibleShovelPlugin : Singleton<CompatibleShovelPlugin>, IConfigurablePlugin, IModKitPlugin, IModInit
    {
        public CompatibleShovelConfig Config => Obj.GetEditObject() as CompatibleShovelConfig;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<CompatibleShovelConfig> config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public CompatibleShovelPlugin()
        {
            this.config = new PluginConfig<CompatibleShovelConfig>("CompatibleShovel");
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
            ShovelItem.MaxTakeResolver.Add(float.MinValue, new InitialShovelSizeModifier());
            ShovelItem.MaxTakeResolver.Add(-100, new ShovelStackSizeModifierSettingModifier());
        }
    }
    public class InitialShovelSizeModifier : IModifyValueInPlaceHandler
    {
        public void ModifyValue(IModifyValueInPlaceContext context)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            switch (shovelContext.Shovel)
            {
                case WoodenShovelItem:
                    shovelContext.FloatValue = CompatibleShovelPlugin.Obj.Config.WoodenShovelBaseSize;
                    shovelContext.IntValue = CompatibleShovelPlugin.Obj.Config.WoodenShovelBaseSize;
                    break;
                case IronShovelItem:
                    shovelContext.FloatValue = CompatibleShovelPlugin.Obj.Config.IronShovelBaseSize;
                    shovelContext.IntValue = CompatibleShovelPlugin.Obj.Config.IronShovelBaseSize;
                    break;
                case SteelShovelItem:
                    shovelContext.FloatValue = CompatibleShovelPlugin.Obj.Config.SteelShovelBaseSize;
                    shovelContext.IntValue = CompatibleShovelPlugin.Obj.Config.SteelShovelBaseSize;
                    break;
                case ModernShovelItem:
                    shovelContext.FloatValue = CompatibleShovelPlugin.Obj.Config.ModernShovelBaseSize;
                    shovelContext.IntValue = CompatibleShovelPlugin.Obj.Config.ModernShovelBaseSize;
                    break;
            }
        }
    }
    public class ShovelStackSizeModifierSettingModifier : IModifyValueInPlaceHandler
    {
        public void ModifyValue(IModifyValueInPlaceContext context)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            if (CompatibleShovelPlugin.Obj.Config.ApplyStackSizeModifier)
            {
                shovelContext.FloatValue *= DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier;
                shovelContext.IntValue = (int)shovelContext.FloatValue;
            }
        }
    }
}