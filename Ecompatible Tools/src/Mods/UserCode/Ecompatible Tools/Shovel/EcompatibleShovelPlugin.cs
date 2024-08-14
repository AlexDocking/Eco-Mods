using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;

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
    public class InitialShovelSizeModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (context.HasProperty(ContextProperties.Shovel, out ShovelItem shovel)) return null;
            int output;
            switch (shovel)
            {
                case WoodenShovelItem:
                    output = EcompatibleShovelPlugin.Obj.Config.WoodenShovelBaseSize;
                    break;
                case IronShovelItem:
                    output = EcompatibleShovelPlugin.Obj.Config.IronShovelBaseSize;
                    break;
                case SteelShovelItem:
                    output = EcompatibleShovelPlugin.Obj.Config.SteelShovelBaseSize;
                    break;
                case ModernShovelItem:
                    output = EcompatibleShovelPlugin.Obj.Config.ModernShovelBaseSize;
                    break;
                default:
                    return null;
            }
            return new BaseLevelModificationOutput(output);
        }
    }
    public class ShovelStackSizeModifierSettingModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            if (EcompatibleShovelPlugin.Obj.Config.ApplyStackSizeModifier)
            {
                float output = functionInput.Input * DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier;
                return new MultiplicationOperationDetails(output, Localizer.DoStr("Server Stack Size"), DifficultySettings.Obj.Config.DifficultyModifiers.StackSizeModifier);
            }
            return null;
        }
    }
}