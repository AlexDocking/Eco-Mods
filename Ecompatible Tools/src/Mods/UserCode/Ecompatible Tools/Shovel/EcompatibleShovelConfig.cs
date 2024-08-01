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
        [Category("Settings"), LocDescription("Should the \"ItemStackSizeMultiplier\" in the Difficulty config affect how much shovels can dig?")]
        public bool ApplyStackSizeModifier { get; set; } = false;

        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
}