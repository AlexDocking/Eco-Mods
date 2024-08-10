using Eco.Core.Controller;
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using EcompatibleTools;

namespace Ecompatible
{
    public partial class EcompatibleShovelPlugin : Singleton<EcompatibleShovelPlugin>, IConfigurablePlugin, IModKitPlugin, IModInit
    {
        public EcompatibleShovelConfig Config => Obj.GetEditObject() as EcompatibleShovelConfig;
        public IPluginConfig PluginConfig => this.config;
        private PluginConfig<EcompatibleShovelConfig> config;
        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public EcompatibleShovelPlugin()
        {
            this.config = new PluginConfig<EcompatibleShovelConfig>("EcompatibleShovel");
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
            ValueResolvers.Tools.Shovel.MaxTakeResolver.Add(float.MinValue, new InitialShovelSizeModifier());
            ValueResolvers.Tools.Shovel.MaxTakeResolver.Add(-100, new ShovelStackSizeModifierSettingModifier());
        }
    }
    public class InitialShovelSizeModifier : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails details)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            switch (shovelContext.Shovel)
            {
                case WoodenShovelItem:
                    shovelContext.FloatValue = EcompatibleShovelPlugin.Obj.Config.WoodenShovelBaseSize;
                    shovelContext.IntValue = EcompatibleShovelPlugin.Obj.Config.WoodenShovelBaseSize;
                    break;
                case IronShovelItem:
                    shovelContext.FloatValue = EcompatibleShovelPlugin.Obj.Config.IronShovelBaseSize;
                    shovelContext.IntValue = EcompatibleShovelPlugin.Obj.Config.IronShovelBaseSize;
                    break;
                case SteelShovelItem:
                    shovelContext.FloatValue = EcompatibleShovelPlugin.Obj.Config.SteelShovelBaseSize;
                    shovelContext.IntValue = EcompatibleShovelPlugin.Obj.Config.SteelShovelBaseSize;
                    break;
                case ModernShovelItem:
                    shovelContext.FloatValue = EcompatibleShovelPlugin.Obj.Config.ModernShovelBaseSize;
                    shovelContext.IntValue = EcompatibleShovelPlugin.Obj.Config.ModernShovelBaseSize;
                    break;
            }
            details = new BaseLevelOperationDetails();
        }
    }
    public class ShovelStackSizeModifierSettingModifier : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails operationDetails)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            if (EcompatibleShovelPlugin.Obj.Config.ApplyStackSizeModifier)
            {
                shovelContext.FloatValue *= DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier;
                shovelContext.IntValue = (int)shovelContext.FloatValue;
                operationDetails = new MultiplicationOperationDetails(Localizer.DoStr("Server Stack Size"), DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier);
            }
        }
    }
}