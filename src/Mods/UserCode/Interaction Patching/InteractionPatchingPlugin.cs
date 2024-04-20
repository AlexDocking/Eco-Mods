using Eco.Core;
using Eco.Core.Controller;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Systems;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using System;
using System.Linq;

namespace InteractionPatching
{
    public class InteractionPatchingPlugin : IInitializablePlugin, IModKitPlugin
    {
        public string GetCategory() => "Mods";

        public string GetStatus() => "";

        public void Initialize(TimedTask timer)
        {
            PluginManager.Controller.RunIfOrWhenInited(ReplaceInteractions);
        }
        public static void EditInteraction(Type type, string methodName, string replacementMethodName)
        {
            var sim = GlobalData.Obj.ServerInteractionManager;
            var interactionDict = sim.InteractorToInteractions;

            var interactionAttributes = interactionDict[type];
            foreach(var oldInteraction in interactionAttributes.Where(interaction => interaction.RPCName == methodName))
            {
                 EditInteraction(oldInteraction, replacementMethodName, null);
            }
        }

        public static void EditInteraction(InteractionAttribute interaction, string newMethodName, LocString? overrideDescription)
        {
            var sim = GlobalData.Obj.ServerInteractionManager;
            if (interaction != null)
            {
                Log.WriteLine(Localizer.Do($"Transfer {interaction.RPCName} to {newMethodName}"));
                interaction.RPCName = newMethodName;
                interaction.Description = overrideDescription ?? interaction.Description;
            }
            sim.Changed(nameof(ServerInteractionManager.InteractorToInteractions));
            GlobalData.Obj.Changed(nameof(GlobalData.ServerInteractionManager));
        }
        public static void ReplaceInteraction(Type type, InteractionAttribute oldInteraction, InteractionAttribute replacementInteraction)
        {
            var sim = GlobalData.Obj.ServerInteractionManager;
            var interactionDict = sim.InteractorToInteractions;

            var interactionAttributes = interactionDict[type];
            interactionAttributes.Remove(replacementInteraction);
            if (oldInteraction != null)
            {
                replacementInteraction.RPCName = replacementInteraction.RPCName;
                replacementInteraction.Description = oldInteraction.Description;
                Log.WriteLine(Localizer.Do($"Remove {oldInteraction.GetType()} {oldInteraction.RPCName}"));
                interactionAttributes.Remove(oldInteraction);
                Log.WriteLine(Localizer.Do($"Add {replacementInteraction.GetType()} {replacementInteraction.RPCName}"));

                interactionAttributes.AddUnique(replacementInteraction);
            }
            sim.Changed(nameof(ServerInteractionManager.InteractorToInteractions));
            GlobalData.Obj.Changed(nameof(GlobalData.ServerInteractionManager));
        }
        private static void ReplaceInteractions()
        {
            var sim = GlobalData.Obj.ServerInteractionManager;
            var interactionDict = sim.InteractorToInteractions;

            foreach (var classType in interactionDict.Keys)
            {
                var interactionAttributes = interactionDict[classType];
                foreach (var replacementInteraction in interactionAttributes.OfType<ReplacementInteractionAttribute>().ToList())
                {
                    Log.WriteLine(Localizer.Do($"found override attribute RPC {replacementInteraction.RPCName}, replaces {replacementInteraction.MethodName} from {classType}"));
                    var oldAttribute = interactionAttributes.FirstOrDefault(attribute => attribute.RPCName == replacementInteraction.MethodName);
                    if (replacementInteraction.CopyParameters)
                    {
                        EditInteraction(oldAttribute, replacementInteraction.RPCName, null);
                        interactionAttributes.Remove(replacementInteraction);
                    }
                    else
                    {
                        ReplaceInteraction(classType, oldAttribute, replacementInteraction);
                    }
                }
            }
            sim.Changed(nameof(ServerInteractionManager.InteractorToInteractions));
            GlobalData.Obj.Changed(nameof(GlobalData.ServerInteractionManager));
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReplacementInteractionAttribute : InteractionAttribute
    {
        public ReplacementInteractionAttribute(string methodName, InteractionTrigger trigger, string overrideDescription = null, InteractionModifier modifier = InteractionModifier.None, string[] requiredEnvVars = null, float interactionDistance = 0, float priority = 0, ClientPredictedBlockAction predictedBlockAction = ClientPredictedBlockAction.None, int maxTake = 0, bool animationDriven = false, TriBool canHoldToTrigger = TriBool.None, string highlightColorHex = null, AccessType authRequired = AccessType.FullAccess, bool enforceAuth = false, InteractionFlags flags = 0, params string[] tags) : base(trigger, overrideDescription, modifier, requiredEnvVars, interactionDistance, priority, predictedBlockAction, maxTake, animationDriven, canHoldToTrigger, highlightColorHex, authRequired, enforceAuth, flags, tags)
        {
            MethodName = methodName;
            CopyParameters = false;
        }

        /// <summary>
        /// Change an existing interaction to use this method, keeping the original interaction parameters
        /// </summary>
        public ReplacementInteractionAttribute(string methodName) : base(default)
        {
            MethodName = methodName;
            CopyParameters = true;
        }
        public string MethodName { get; }
        public bool CopyParameters { get; }
    }
}
