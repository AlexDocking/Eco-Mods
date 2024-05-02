using Eco.Core.Controller;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems;
using Eco.Shared.Localization;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            ReplaceInteractions(interactionDict);
            sim.Changed(nameof(ServerInteractionManager.InteractorToInteractions));
            GlobalData.Obj.Changed(nameof(GlobalData.ServerInteractionManager));
        }

        public static void AddInteractions(IDictionary<Type, List<InteractionAttribute>> interactionDict, IEnumerable<AddInteractionModification> additions)
        {
            foreach(var addition in additions)
            {
                foreach(var type in addition.InteractorType.ConcreteTypes(includeSelf: true))
                {
                    var m = type.GetMethod(addition.InteractorMethodName);
                    if (!type.GetMethods().Any(method => method.Name == addition.InteractorMethodName && method.VerifySignature(typeof(Player), typeof(InteractionTriggerInfo), typeof(InteractionTarget)))) continue;
                    InteractionAttribute newAttribute = addition?.InteractionCreationMethod(type);
                    if (newAttribute == null) continue;
                    interactionDict.AddToList(type, newAttribute);
                }
            }
        }
        public static void AddInteractions(IDictionary<Type, List<InteractionAttribute>> interactionDict)
        {
            AddInteractions(interactionDict, FindAddModifications());
        }
        public static void ModifyInteractions(IDictionary<Type, List<InteractionAttribute>> interactionDict, IEnumerable<InteractionParametersModification> modifications)
        {
            foreach(var modification in modifications)
            {
                foreach (var type in modification.InteractorType.DerivedTypes(includeSelf: true))
                {
                    if (interactionDict.TryGetValue(type, out var list))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i] = modification.ModificationMethod(type, list[i].Clone());
                        }
                    }
                }
            }
        }
        public static void ModifyInteractions(IDictionary<Type, List<InteractionAttribute>> interactionDict)
        {
            ModifyInteractions(interactionDict, FindInteractionModifications());
        }
        private static IEnumerable<InteractionParametersModification> FindInteractionModifications()
        {
            var classesWithModifiers = ReflectionCache.GetAssemblies().SelectMany(assembly => assembly.DefinedTypes.Where(type => type.HasAttribute<DefinesInteractionsAttribute>()));
            var modificationMethods = classesWithModifiers.SelectMany(type => type.MethodsWithAttribute<ModifyInteractionAttribute>()).Where(method => method.IsPublic && method.IsStatic && !method.IsGenericMethod && method.VerifySignature(typeof(Type), typeof(InteractionAttribute).GetTypeInfo().MakeByRefType()));
            var interactionParameterModifications = modificationMethods.Select(method => {
                var attribute = method.GetCustomAttribute<ModifyInteractionAttribute>();
                return new InteractionParametersModification()
                {
                    InteractorType = attribute.InteractorType,
                    InteractorMethodName = method.Name,
                    ModificationMethod = (type, interaction) => { var parameters = new object[] { type, interaction.Clone() }; method.Invoke(null, parameters); return InteractionExtensions.AreEqual(interaction, parameters[1] as InteractionAttribute) ? interaction : parameters[1] as InteractionAttribute; }
                };
            });
            return interactionParameterModifications;
        }
        private static IEnumerable<AddInteractionModification> FindAddModifications()
        {
            var classesWithModifiers = ReflectionCache.GetAssemblies().SelectMany(assembly => assembly.DefinedTypes.Where(type => type.HasAttribute<DefinesInteractionsAttribute>()));

            var addMethods = classesWithModifiers.SelectMany(type => type.MethodsWithAttribute<AdditionalInteractionAttribute>()).Where(method => method.IsPublic && method.IsStatic && method.VerifySignature(typeof(Type)) && method.ReturnType == typeof(InteractionAttribute));
            var addModifications = addMethods.Select(method => {
                var attribute = method.GetCustomAttribute<AdditionalInteractionAttribute>();
                var modification = new AddInteractionModification()
                {
                    InteractorType = attribute.InteractorType,
                    InteractorMethodName = attribute.MethodName,
                };
                modification.SetCreationMethod(method);
                return modification;
            });
            return addModifications;
        }
        private class Method
        {
            public string RPCName { get; set; }
            public List<InteractionAttribute> Interactions { get; set; } = new();
            public List<Method> ReplacedBy { get; set; } = new();
        }
        public static void ReplaceInteractions(IEnumerable<KeyValuePair<Type, List<InteractionAttribute>>> interactionDict)
        {
            foreach (var interactionsForType in interactionDict)
            {
                Type type = interactionsForType.Key;
                List<InteractionAttribute> list = interactionsForType.Value;
                List<Method> methods = new List<Method>();
                //Find all the interactions for each method
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
                //Find out which methods replace which
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
                //Find paths from the methods where interactions are defined, to the actual methods which those interactions will be attached to
                List<InteractionAttribute> leaves = new List<InteractionAttribute>();
                foreach (var method in methods.Where(m => m.Interactions.Any(interaction => interaction.GetType() == typeof(InteractionAttribute))))
                {
                    Log.WriteLine(Localizer.Do($"Method {method.RPCName} has {method.Interactions.Count} interactions and is replaced by {method.ReplacedBy.Select(m => m.RPCName).CommaList()}"));
                    IEnumerable<List<Method>> routeToLeaves = FindRoutesToLeaves(method, new List<Method>());
                    //Traverse each path and get the parameters of the resulting interaction, either passed along the chain from the original method or by any custom replacements on the way
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
                                //All interactions on the previous method get transferred
                                if (replacement.CopyParameters)
                                {
                                    newResults.AddRange(results.Select(interaction => { var clone = interaction.Clone(); clone.RPCName = replacement.RPCName; return clone; }));
                                }
                                //Or, a replacement interaction is provided and all interactions on the previous method are forgotten
                                else
                                {
                                    var providedReplacement = replacement.GetReplacementInteraction(type);
                                    providedReplacement.RPCName = replacement.RPCName;
                                    Log.WriteLine(Localizer.Do($"{replacement.RPCName} provided custom interaction from {type}.{replacement.InteractionParametersGetter} -> {providedReplacement?.GetType()}"));
                                    newResults.Add(providedReplacement);
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
        public static bool AreEqual(InteractionAttribute first,  InteractionAttribute second)
        {
            return first?.GetType() == second?.GetType() &&
                first.TriggerInfo.Trigger == second.TriggerInfo.Trigger &&
                first.Description == second.Description &&
                first.TriggerInfo.Modifier == second.TriggerInfo.Modifier &&
                first.RequiredEnvVars.SequenceEqualNullSafe(second.RequiredEnvVars) &&
                first.InteractionDistance == second.InteractionDistance &&
                first.Priority == second.Priority &&
                first.PredictedBlockAction == second.PredictedBlockAction &&
                first.MaxTake == second.MaxTake &&
                first.AnimationDriven == second.AnimationDriven &&
                first.CanHoldToTrigger == second.CanHoldToTrigger &&
                first.HighlightColor.HexRGBA == second.HighlightColor.HexRGBA &&
                first.AccessForHighlight == second.AccessForHighlight &&
                first.Flags == second.Flags &&
                first.TagsTargetable.SequenceEqualNullSafe(second.TagsTargetable);
        }
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
