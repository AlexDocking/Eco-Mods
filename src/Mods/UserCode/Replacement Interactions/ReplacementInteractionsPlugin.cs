using Eco.Core;
using Eco.Core.Controller;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Systems;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;

namespace ReplacementInteractions
{
    [Priority(4)] //Execute as early as possible in case later plugins want to access the interactions
    public class ReplacementInteractionsPlugin : IInitializablePlugin, IModKitPlugin
    {
        public string GetCategory() => "Mods";

        public string GetStatus() => "";

        public void Initialize(TimedTask timer)
        {
            ReplaceInteractions();
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
        public static void ReplaceInteraction(List<InteractionAttribute> list, InteractionAttribute oldInteraction, InteractionAttribute replacementInteraction)
        {
            var sim = GlobalData.Obj.ServerInteractionManager;
            var interactionDict = sim.InteractorToInteractions;

            var interactionAttributes = list;
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
            ReplaceInteractions(interactionDict.Values);
            sim.Changed(nameof(ServerInteractionManager.InteractorToInteractions));
            GlobalData.Obj.Changed(nameof(GlobalData.ServerInteractionManager));
        }
        private class Method
        {
            public string RPCName { get; set; }
            public List<InteractionAttribute> Interactions { get; set; } = new();
            public List<Method> ReplacedBy { get; set; } = new();
        }
        public static void ReplaceInteractions(IEnumerable<List<InteractionAttribute>> interactionLists)
        {
            foreach (var list in interactionLists)
            {
                List<Method> methods = new List<Method>();
                foreach(var interaction in list)
                {
                    if (methods.FirstOrDefault(method => method.RPCName == interaction.RPCName) is not Method method)
                    {
                        method = new Method() { RPCName = interaction.RPCName, Interactions = new List<InteractionAttribute>() { interaction } };
                        methods.Add(method);
                        Log.WriteLine(Localizer.Do($"New method {method.RPCName}"));
                    }
                    else
                    {
                        method.Interactions.Add(interaction);
                        Log.WriteLine(Localizer.Do($"Found existing method {method.RPCName}, interactions:{method.Interactions.Select(interaction => $"{interaction.RPCName}").CommaList()}"));

                    }
                }
                foreach (var interaction in list)
                {
                    if (interaction is ReplacementInteractionAttribute replacement)
                    {
                        if (methods.FirstOrDefault(method => method.RPCName == replacement.MethodName) is Method method)
                        {
                            method.ReplacedBy.Add(methods.First(m => m.RPCName == interaction.RPCName));
                            Log.WriteLine(Localizer.Do($"Found replacement method {method.RPCName}->{replacement.RPCName}"));
                        }
                    }
                }
                List<InteractionAttribute> leaves = new List<InteractionAttribute>();
                foreach (var method in methods.Where(m => m.Interactions.Any(interaction => interaction.GetType() == typeof(InteractionAttribute))))
                {
                    Log.WriteLine(Localizer.Do($"Method {method.RPCName} has {method.Interactions.Count} interactions and is replaced by {method.ReplacedBy.Select(m => m.RPCName).CommaList()}"));
                    IEnumerable<List<Method>> routeToLeaves = FindRoutesToLeaves(method, new List<Method>());
                    foreach(var route in routeToLeaves)
                    {
                        List<InteractionAttribute> results = new List<InteractionAttribute>();
                        Log.WriteLine(Localizer.Do($"Found route: {route.Select(m => m.RPCName).TextList("->")}"));
                        results.AddRange(route.First().Interactions);
                        Log.WriteLine(Localizer.Do($"result[0] count:{results.Count}:{results.Select(interaction => interaction.RPCName).SimpleCommaList()}"));
                        for(int i = 1; i < route.Count(); i++)
                        {
                            List<InteractionAttribute> newResults = new List<InteractionAttribute>();
                            Method step = route[i];
                            Log.WriteLine(Localizer.Do($"Step [{i}] has {step.Interactions.Count} interactions of which {step.Interactions.OfType<ReplacementInteractionAttribute>().Count()} are replacements"));
                            List<ReplacementInteractionAttribute> replacementAttributes = step.Interactions.OfType<ReplacementInteractionAttribute>().Where(replacement => replacement.MethodName == route[i-1].RPCName).ToList();
                            foreach(ReplacementInteractionAttribute replacement in replacementAttributes)
                            {
                                Log.WriteLine(Localizer.Do($"Replacement {replacement.RPCName} replaces {replacement.MethodName}"));
                                if (replacement.CopyParameters)
                                {
                                    newResults.AddRange(results.Select(interaction => { var clone = interaction.Clone(); clone.RPCName = replacement.RPCName; return clone; }));
                                }
                                else
                                {
                                    //todo: read new interaction params from replacement attribute
                                }
                            }
                            results = newResults;
                            Log.WriteLine(Localizer.Do($"result[{i}] count:{results.Count}"));

                        }
                        leaves.AddRange(results);
                    }
                }
                list.Clear();
                list.AddRange(leaves);
                Log.WriteLine(Localizer.Do($"Results:{leaves.Select(leaf => $"{leaf.RPCName} ({leaf.GetType()})").SimpleCommaList()}"));
                IEnumerable<List<Method>> FindRoutesToLeaves(Method root, List<Method> visited)
                {
                    visited = new List<Method>(visited);
                    if (visited.Contains(root))
                    {
                        throw new InvalidOperationException($"Interaction cycle detected: {visited.Select(m => m.RPCName).TextList("->")}->{root.RPCName}");
                    }
                    visited.Add(root);
                    ISet<List<Method>> leaves = new HashSet<List<Method>>();
                    foreach(Method replacement in root.ReplacedBy)
                    {
                        var routes = FindRoutesToLeaves(replacement, visited);
                        foreach (var route in routes) { route.Insert(0, root); }
                        leaves.AddRange(routes);
                    }
                    if (!leaves.Any())
                    {
                        leaves.Add(new List<Method>(new Method[] { root }));
                    }
                    return leaves;
                }
                /*foreach (var replacementInteraction in list.OfType<ReplacementInteractionAttribute>().ToList())
                {
                    Log.WriteLine(Localizer.Do($"found override attribute RPC {replacementInteraction.RPCName}, replaces {replacementInteraction.MethodName}"));
                    var oldAttribute = list.FirstOrDefault(attribute => attribute.RPCName == replacementInteraction.MethodName);
                    if (replacementInteraction.CopyParameters)
                    {
                        EditInteraction(oldAttribute, replacementInteraction.RPCName, null);
                        list.Remove(replacementInteraction);
                    }
                    else
                    {
                        ReplaceInteraction(list, oldAttribute, replacementInteraction);
                    }
                }*/
            }
        }
    }
    static class InteractionExtensions
    {
        public static InteractionAttribute Clone(this InteractionAttribute interaction)
        {
            if (interaction == null)
            {
                throw new ArgumentNullException(nameof(interaction));
            }
            var clone =  new InteractionAttribute(
                interaction.TriggerInfo.Trigger,
                interaction.Description,
                interaction.TriggerInfo.Modifier,
                interaction.RequiredEnvVars?.ToArray(),
                interaction.InteractionDistance,
                interaction.Priority,
                interaction.PredictedBlockAction,
                interaction.MaxTake,
                interaction.AnimationDriven,
                interaction.CanHoldToTrigger,
                interaction.HighlightColor.HexRGBA,
                interaction.AccessForHighlight,
                false,
                interaction.Flags,
                interaction.TagsTargetable?.Select(tag => tag.Name).ToArray() ?? Array.Empty<string>());
            clone.Init(interaction.InteractorType, interaction.RPCName);
            return clone;
        }
    }
}
