using Eco.Core.Controller;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems;
using Eco.Shared.Localization;
using Eco.Shared.Localization.ConstLocs;
using Eco.Shared.Networking;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Eco.Simulation.RouteProbing.AStarSearch;

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
                            if (list[i].RPCName != modification.InteractorMethodName) continue;
                            string methodName = list[i].RPCName;
                            list[i] = modification.ModificationMethod(type, list[i].Clone());
                            list[i]?.Init(type, methodName);
                        }
                        list.RemoveNulls();
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
                    InteractorMethodName = attribute.MethodName,
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
        private interface INode
        {
            IEnumerable<INode> Children { get; }
        }
        private class MethodInteraction : INode
        {
            public string RPCName { get; set; }
            public List<InteractionAttribute> Interactions { get; set; } = new();
            public List<MethodInteraction> ReplacedBy { get; set; } = new();
            public IEnumerable<INode> Children => ReplacedBy;
        }
        private class MethodRPC : INode
        {
            public string RPCName { get; set; }
            public ISet<ReplacementRPCAttribute> RPCs { get; set; } = new HashSet<ReplacementRPCAttribute>();
            public List<MethodRPC> ReplacedBy { get; set; } = new();
            public IEnumerable<INode> Children => ReplacedBy;
        }
        public static void ReplaceInteractions(IEnumerable<KeyValuePair<Type, List<InteractionAttribute>>> interactionDict)
        {
            foreach (var interactionsForType in interactionDict)
            {
                Type type = interactionsForType.Key;
                List<InteractionAttribute> list = interactionsForType.Value;
                List<MethodInteraction> methods = new List<MethodInteraction>();
                //Find all the interactions for each method
                foreach(var interaction in list)
                {
                    if (methods.FirstOrDefault(method => method.RPCName == interaction.RPCName) is not MethodInteraction method)
                    {
                        method = new MethodInteraction() { RPCName = interaction.RPCName, Interactions = new List<InteractionAttribute>() { interaction } };
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
                        if (methods.FirstOrDefault(method => method.RPCName == replacement.MethodName) is MethodInteraction method)
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
                    IEnumerable<List<MethodInteraction>> routeToLeaves = FindRoutesToLeaves(method, new List<MethodInteraction>());
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
                            MethodInteraction step = route[i];
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
                IEnumerable<List<MethodInteraction>> FindRoutesToLeaves(MethodInteraction root, List<MethodInteraction> visited)
                {
                    visited = new List<MethodInteraction>(visited);
                    if (visited.Contains(root))
                    {
                        throw new InvalidOperationException($"Interaction cycle detected: {visited.Select(m => m.RPCName).TextList("->")}->{root.RPCName}");
                    }
                    visited.Add(root);
                    ISet<List<MethodInteraction>> leaves = new HashSet<List<MethodInteraction>>();
                    foreach(MethodInteraction replacement in root.ReplacedBy)
                    {
                        var routes = FindRoutesToLeaves(replacement, visited);
                        foreach (var route in routes) { route.Insert(0, root); }
                        leaves.AddRange(routes);
                    }
                    if (!leaves.Any())
                    {
                        leaves.Add(new List<MethodInteraction>(new MethodInteraction[] { root }));
                    }
                    return leaves;
                }
            }
        }

        private static IEnumerable<ReplacementRPCAttribute> FindReplacementRPCs()
        {
            var classesWithReplacementRPCs = ReflectionCache.GetAssemblies().SelectMany(assembly => assembly.DefinedTypes.Where(type => type.HasAttribute<DefinesInteractionsAttribute>()));
            var replacementRPCMethods = classesWithReplacementRPCs.SelectMany(type => type.MethodsWithAttribute<ReplacementRPCAttribute>()).Where(method => method.IsPublic && method.IsStatic && !method.IsGenericMethod);
            var replacementRPCAttributes = replacementRPCMethods.SelectMany(method => {
                var attributes = method.GetCustomAttributes<ReplacementRPCAttribute>();
                foreach (var attribute in attributes)
                {
                    attribute.Initialize(method);
                }
                return attributes;
            });
            return replacementRPCAttributes;
        }
        public static void ReplaceRPCs()
        {
            //Discover replacement RPC attributes
            Dictionary<Type, List<ReplacementRPCAttribute>> replacementRPCs = new Dictionary<Type, List<ReplacementRPCAttribute>>();

            var nodes = new List<MethodRPC>();
            foreach (var replacementRPC in FindReplacementRPCs())
            {
                replacementRPCs.AddToList(replacementRPC.Type, replacementRPC);
            }
            Dictionary<RPCMethod, ReplacementRPCAttribute> rpcMethodReplacements = new Dictionary<RPCMethod, ReplacementRPCAttribute>();
            foreach (var type in replacementRPCs.Keys)
            {
                foreach (var replacementRPC in replacementRPCs[type])
                {
                    MethodRPC rpcNode = nodes.FirstOrDefault(node => node.RPCName == replacementRPC.RPCName);
                    if (rpcNode == null)
                    {
                        rpcNode = new MethodRPC();
                        rpcNode.RPCName = replacementRPC.RPCName;
                        nodes.Add(rpcNode);
                    }
                    MethodRPC replacementNode = nodes.FirstOrDefault(node => node.RPCName == replacementRPC.ReplacementRPCName);
                    if (replacementNode == null)
                    {
                        replacementNode = new MethodRPC();
                        replacementNode.RPCName = replacementRPC.ReplacementRPCName;
                        nodes.Add(replacementNode);
                    }
                    replacementNode.RPCs.Add(replacementRPC);
                    rpcNode.ReplacedBy.Add(replacementNode);
                }
                var rpcDict = RPCManager.GetOrBuildLookup(type);
                foreach (var node in nodes)
                {
                    if (rpcDict.TryGetValue(node.RPCName, out RPCMethod[] rpcMethods))
                    {
                        var routes = FindRoutesToLeaves(node, new List<MethodRPC>());
                        var leaf = routes.First().Last();
                        if (!routes.All(route => route.Last() == leaf))
                        {
                            string leafNodeMethodNames = routes.Select(routes => routes.Last()).Distinct().Select(node =>
                                                            {
                                                                var method = node.RPCs.First().MethodInfo;
                                                                return $"{method.DeclaringType.FullName}.{method.Name}";
                                                            }).NewlineList();
                            throw new Exception($"RPCs can only be replaced by a single implementation. For \"{type.FullName}.{node.RPCName}\", found:\n{leafNodeMethodNames}");
                        }

                        ReplacementRPCAttribute replacementRPC = leaf.RPCs.First();
                        foreach(var rpcMethod in rpcMethods)
                        {
                            rpcMethodReplacements.Add(rpcMethod, replacementRPC);
                        }
                    }
                }
            }
            //Update RPCMethods with their replacement functions
            foreach(RPCMethod rpcMethod in rpcMethodReplacements.Keys)
            {
                ReplacementRPCAttribute replacementRPC = rpcMethodReplacements[rpcMethod];
                rpcMethod.SetPropertyWithBackingFieldByName(nameof(RPCMethod.Func), replacementRPC.Func);
            }
        }
        private static IEnumerable<List<T>> FindRoutesToLeaves<T>(T root, List<T> visited) where T : INode
        {
            visited = new List<T>(visited);
            if (visited.Contains(root))
            {
                throw new InvalidOperationException($"Interaction cycle detected: {visited.Select(m => m.ToString()).TextList("->")}->{root.ToString()}");
            }
            visited.Add(root);
            ISet<List<T>> leaves = new HashSet<List<T>>();
            foreach (T replacement in root.Children)
            {
                var routes = FindRoutesToLeaves<T>(replacement, visited);
                foreach (var route in routes) { route.Insert(0, root); }
                leaves.AddRange(routes);
            }
            if (!leaves.Any())
            {
                leaves.Add(new List<T>(new T[] { root }));
            }
            return leaves;
        }
    }
    static class ExtraReflectionUtils
    {
        public static void SetPropertyWithBackingFieldByName(this object obj, string name, object value)
        {
            var field = obj.GetType().GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(obj, value);
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
