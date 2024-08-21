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
        [Category("Settings"), LocDescription("How much damage should be dealt to the stump."), Range(0, 10000)]
        public int DamageToStumpWhenFelled { get; set; } = 0;
        [Category("Settings"), LocDescription("What is the maximum number of debris to spawn per tree."), Range(0, 100)]
        public int MaxTreeDebrisToSpawn { get; set; } = 20;
        [Category("Settings"), LocDescription("What is the chance for each debris to get automatically cleared."), Range(0, 1)]
        public int ChanceToClearDebrisOnSpawn { get; set; } = 0;

        #region IController
        int controllerID;
        public event PropertyChangedEventHandler PropertyChanged;
        public ref int ControllerID => ref this.controllerID;
        #endregion
    }
}