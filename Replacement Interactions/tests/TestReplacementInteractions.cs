﻿using Eco.Core.Tests;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using EcoTestTools;
using Eco.Shared.Logging;

namespace ReplacementInteractions.Tests
{
    [ChatCommandHandler]
    public static class TestReplacementInteractions
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestReplacementInteractionPlugin()
        {
            Test.Run(ShouldReplaceSingleInteraction, nameof(ShouldReplaceSingleInteraction));
            Test.Run(ShouldReplaceChainedInteractions, nameof(ShouldReplaceChainedInteractions));
            Test.Run(ShouldIgnoreMissingInteractions, nameof(ShouldIgnoreMissingInteractions));
            Test.Run(ShouldThrowExceptionIfThereIsACycle, nameof(ShouldThrowExceptionIfThereIsACycle));
            Test.Run(ShouldTrackMultipleInteractionsOnSameMethod, nameof(ShouldTrackMultipleInteractionsOnSameMethod));
            Test.Run(ShouldUseCustomInteractionIfProvided, nameof(ShouldUseCustomInteractionIfProvided));
            Test.Run(ShouldThrowExceptionIfCustomInteractionGetterDoesNotExist, nameof(ShouldThrowExceptionIfCustomInteractionGetterDoesNotExist));
            Test.Run(ShouldThrowExceptionIfCustomInteractionGetterIsIncorrect, nameof(ShouldThrowExceptionIfCustomInteractionGetterIsIncorrect));
            Test.Run(ShouldModifyInteraction, nameof(ShouldModifyInteraction));
            Test.Run(ShouldModifyInteractionUsingAttributesOnClass, nameof(ShouldModifyInteractionUsingAttributesOnClass));
            Test.Run(ShouldAddNewInteractionUsingAttributesOnClass, nameof(ShouldAddNewInteractionUsingAttributesOnClass));
            Test.Run(ShouldAddNewInteractionBasedOnType, nameof(ShouldAddNewInteractionBasedOnType));
            Test.Run(ShouldNotAddInteractionForMissingMethod, nameof(ShouldNotAddInteractionForMissingMethod));
            Test.Run(ShouldRemoveInteraction, nameof(ShouldRemoveInteraction));
            Test.Run(ShouldNotRedirectInteractionUsingModifyInteractionAttribute, nameof(ShouldNotRedirectInteractionUsingModifyInteractionAttribute));
            Test.Run(ShouldReplaceRPCMethodFunc, nameof(ShouldReplaceRPCMethodFunc));
        }
        private static void ShouldReplaceSingleInteraction()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
            };
            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleInteractor.ReplacementMethod1));
            }
        }
        private static void ShouldReplaceChainedInteractions()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.ReplacementMethod1)) { RPCName = nameof(ExampleInteractor.ReplacementMethod2) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleInteractor.ReplacementMethod2));
            }
        }
        static IEnumerable<List<InteractionAttribute>> AllPermutationsOfAttributeList(Func<List<InteractionAttribute>> listGenerator)
        {
            List<InteractionAttribute> firstList = listGenerator();
            List<int> indices = new List<int>();
            for (int i = 0; i < firstList.Count; i++)
            {
                indices.Add(i);
            }

            IEnumerable<IEnumerable<int>> indicesPermutations = AllPermutations<int>(indices);
            foreach(var indicesPermutation in indicesPermutations)
            {
                var copy = listGenerator();
                List<InteractionAttribute> permutation = indicesPermutation.Select(i => copy[i]).ToList();
                yield return permutation;
            }
        }
        static IEnumerable<List<T>> AllPermutations<T>(List<T> list)
        {
            if (!list.Any()) return Array.Empty<List<T>>();
            List<List<T>> permutations = new List<List<T>>();
            if (list.Count == 1) permutations.Add(list);
            for (int i = 0; i < list.Count; i++)
            {
                var allExceptChosen = list.Take(i).ToList();
                allExceptChosen.AddRange(list.Skip(i + 1));
                foreach (var remainder in AllPermutations(allExceptChosen))
                {
                    var permutation = list[i].SingleItemAsEnumerable().ToList();
                    permutation.AddRange(remainder);
                    permutations.Add(permutation);
                }
            }
            return permutations;
        }
        private static void ShouldIgnoreMissingInteractions()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
                new ReplacementInteractionAttribute("MissingInteraction") { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.ReplacementMethod1)) { RPCName = nameof(ExampleInteractor.ReplacementMethod2) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleInteractor.ReplacementMethod2));
            }
        }
        private static void ShouldThrowExceptionIfThereIsACycle()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.ReplacementMethod1)) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
            };
            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Assert.Throws<InvalidOperationException>(() => ReplacementInteractionsPlugin.ReplaceInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), interactionList } }));
            }
        }
        private static void ShouldTrackMultipleInteractionsOnSameMethod()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new InteractionAttribute(InteractionTrigger.RightClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList);
            }
            void Check(List<InteractionAttribute> list)
            {
                ReplacementInteractionsPlugin.ReplaceInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), list } });
                Log.WriteLine(Localizer.Do($"Multiple. {list.Select(x => x.RPCName).CommaList()}"));
                Assert.AreEqual(2, list.Count);
                var leftClickInteraction = list.FirstOrDefault(interaction => interaction.TriggerInfo.Trigger == InteractionTrigger.LeftClick);
                Assert.IsNotNull(leftClickInteraction);
                Assert.AreEqual(nameof(ExampleInteractor.ReplacementMethod1), leftClickInteraction.RPCName);
                var rightClickInteraction = list.FirstOrDefault(interaction => interaction.TriggerInfo.Trigger == InteractionTrigger.RightClick);
                Assert.IsNotNull(rightClickInteraction);
                Assert.AreEqual(nameof(ExampleInteractor.ReplacementMethod1), rightClickInteraction.RPCName);
            }
        }
        private static void ShouldUseCustomInteractionIfProvided()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.RightClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod), nameof(ExampleInteractor.GetReplacementInteraction)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleInteractor.ReplacementMethod1));
            }
            void Check(List<InteractionAttribute> list, string expectedMethodName)
            {
                Log.WriteLine(Localizer.Do($"Check {list.Select(x => x is ReplacementInteractionAttribute r ? $"[{r.MethodName}->{x.RPCName}]" : $"<{x.RPCName}>").CommaList()}"));

                ReplacementInteractionsPlugin.ReplaceInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), list } });
                Assert.AreEqual(1, list.Count);
                var interaction = list[0];
                Assert.AreEqual(InteractionTrigger.RightClick, interaction.TriggerInfo.Trigger);
                Assert.AreEqual(expectedMethodName, interaction.RPCName);
                Log.WriteLine(Localizer.Do($"Result:{interaction.RPCName} named {interaction.Description}"));
            }
        }
        private static void ShouldThrowExceptionIfCustomInteractionGetterDoesNotExist()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod), "MethodDoesNotExist") { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },
            };
            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Assert.Throws<MissingMethodException>(() => ReplacementInteractionsPlugin.ReplaceInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), interactionList } }));
            }
        }
        private static void ShouldThrowExceptionIfCustomInteractionGetterIsIncorrect()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleInteractor.OriginalMethod), nameof(ExampleInteractor.IncorrectReplacementInteraction)) { RPCName = nameof(ExampleInteractor.ReplacementMethod1) },            
            };
            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Assert.Throws<MissingMethodException>(() => ReplacementInteractionsPlugin.ReplaceInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), interactionList } }));
            }
        }
        private static void ShouldModifyInteraction()
        {
            List<InteractionAttribute> originalInteractions = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
            };
            List<InteractionParametersModification> interactionModifications = new List<InteractionParametersModification>()
            {
                new InteractionParametersModification()
                {
                    InteractorType = typeof(ExampleInteractor),
                    InteractorMethodName = nameof(ExampleInteractor.OriginalMethod),
                    ModificationMethod = (type, interaction) => { ExampleInteractionReplacer.ModifyInteractionOnOriginalMethod(type, ref interaction); return interaction; }
                }
            };
            ReplacementInteractionsPlugin.ModifyInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), originalInteractions } }, interactionModifications);
            Assert.AreEqual(1, originalInteractions.Count);
            var interaction = originalInteractions[0];
            Assert.AreEqual(InteractionTrigger.RightClick, interaction.TriggerInfo.Trigger);
            Assert.AreEqual(nameof(ExampleInteractor.OriginalMethod), interaction.RPCName);
        }
        private static void ShouldModifyInteractionUsingAttributesOnClass()
        {
            List<InteractionAttribute> originalInteractions = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
            };
            ReplacementInteractionsPlugin.ModifyInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), originalInteractions } });
            Assert.AreEqual(1, originalInteractions.Count);
            var interaction = originalInteractions[0];
            Assert.AreEqual(InteractionTrigger.RightClick, interaction.TriggerInfo.Trigger);
            Assert.AreEqual(nameof(ExampleInteractor.OriginalMethod), interaction.RPCName);
        }
        private static void ShouldAddNewInteractionUsingAttributesOnClass()
        {
            List<InteractionAttribute> originalInteractions = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod) },
            };
            ReplacementInteractionsPlugin.AddInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), originalInteractions } });
            Assert.AreEqual(2, originalInteractions.Count);
            var newInteraction = originalInteractions[1];
            Assert.AreEqual(InteractionTrigger.InteractKey, newInteraction.TriggerInfo.Trigger);
            Assert.AreEqual(nameof(ExampleInteractor.OriginalMethod), newInteraction.RPCName);
        }
        private static void ShouldAddNewInteractionBasedOnType()
        {
            List<InteractionAttribute> baseClassInteractions = new List<InteractionAttribute>();
            List<InteractionAttribute> childClassInteractions = new List<InteractionAttribute>();
            ReplacementInteractionsPlugin.AddInteractions(new Dictionary<Type, List<InteractionAttribute>>()
            {
                { typeof(ExampleInteractor), baseClassInteractions },
                { typeof(ExampleInteractorChildClass), childClassInteractions },
            });
            Assert.AreEqual(0, baseClassInteractions.Count(interaction => interaction.RPCName == nameof(ExampleInteractor.OriginalMethod2)));
            Assert.AreEqual(1, childClassInteractions.Count(interaction => interaction.RPCName == nameof(ExampleInteractor.OriginalMethod2)));

            var newInteraction = childClassInteractions.Single(interaction => interaction.RPCName == nameof(ExampleInteractor.OriginalMethod2));
            Assert.AreEqual(InteractionTrigger.InteractKey, newInteraction.TriggerInfo.Trigger);
            Assert.AreEqual(nameof(ExampleInteractor.OriginalMethod2), newInteraction.RPCName);
        }
        private static void ShouldNotAddInteractionForMissingMethod()
        {
            List<InteractionAttribute> originalInteractions = new List<InteractionAttribute>();
            ReplacementInteractionsPlugin.AddInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), originalInteractions } });
            Assert.IsFalse(originalInteractions.Any(interaction => interaction.RPCName == "MissingMethod"));
        }
        private static void ShouldRemoveInteraction()
        {
            List<InteractionAttribute> originalInteractions = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod3) },
            };
            Dictionary<Type, List<InteractionAttribute>> interactionDict = new Dictionary<Type, List<InteractionAttribute>>() {
                { typeof(ExampleInteractor), originalInteractions.ToList() },
                { typeof(ExampleInteractorChildClass), originalInteractions.ToList() }
            };
            ReplacementInteractionsPlugin.ModifyInteractions(interactionDict);
            //Should remove the interaction on the child class because of RemoveInteractionOnOriginalMethod2
            Assert.AreEqual(1, interactionDict[typeof(ExampleInteractor)].Count);
            Assert.AreEqual(0, interactionDict[typeof(ExampleInteractorChildClass)].Count);
        }
        private static void ShouldNotRedirectInteractionUsingModifyInteractionAttribute()
        {
            List<InteractionAttribute> originalInteractions = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleInteractor.OriginalMethod3) },
            };
            ReplacementInteractionsPlugin.ModifyInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), originalInteractions } });
            Assert.AreEqual(1, originalInteractions.Count);
            var interaction = originalInteractions[0];
            Assert.AreEqual(nameof(ExampleInteractor.OriginalMethod3), interaction.RPCName);
        }
        /// <summary>
        /// Test replacing an RPC with a static method in another class
        /// </summary>
        private class StaticRPCTest
        {
            private RPCMethod RPCMethod { get; }
            private ExampleInteractor Interactor { get; }
            public StaticRPCTest()
            {
                Dictionary<string, RPCMethod[]> rpcDict = RPCManager.GetOrBuildLookup(typeof(ExampleInteractor));
                var rpcMethods = rpcDict[nameof(ExampleInteractor.OriginalMethodRPC)];
                Assert.AreEqual(1, rpcMethods.Length);
                RPCMethod = rpcMethods.First();
                Interactor = new ExampleInteractor();
            }
            public void Setup()
            {
                Assert.AreEqual(0, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.OriginalMethodRPC)));
                RPCMethod.Func(Interactor, new object[] { default, default, default });
                Assert.AreEqual(1, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.OriginalMethodRPC)));
                Assert.AreEqual(0, ExampleRPCReplacer.Calls.GetValueOrDefault(nameof(ExampleRPCReplacer.ReplacementRPCPatch)));
                Interactor.Calls.Clear();
            }
            public void Test()
            {
                RPCMethod.Func(Interactor, new object[] { default, default, default });
                Assert.AreEqual(0, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.OriginalMethodRPC)));
                Assert.AreEqual(1, ExampleRPCReplacer.Calls.GetValueOrDefault(nameof(ExampleRPCReplacer.ReplacementRPCPatch)));
                ExampleRPCReplacer.Calls.Clear();
            }
        }
        /// <summary>
        /// Test replacing an RPC with a method in the same class
        /// </summary>
        private class LocalRPCTest
        {
            private RPCMethod RPCMethod { get; }
            private ExampleInteractor Interactor { get; }
            public LocalRPCTest()
            {
                Dictionary<string, RPCMethod[]> rpcDict = RPCManager.GetOrBuildLookup(typeof(ExampleInteractor));
                var rpcMethods = rpcDict[nameof(ExampleInteractor.SecondMethodRPC)];
                Assert.AreEqual(1, rpcMethods.Length);
                RPCMethod = rpcMethods.First();
                Interactor = new ExampleInteractor();
            }
            public void Setup()
            {
                Assert.AreEqual(0, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.SecondMethodRPC)));
                RPCMethod.Func(Interactor, new object[0]);
                Assert.AreEqual(1, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.SecondMethodRPC)));
                Assert.AreEqual(0, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.ReplacementSecondMethodRPC)));
                Interactor.Calls.Clear();
            }
            public void Test()
            {
                RPCMethod.Func(Interactor, new object[0]);
                Assert.AreEqual(0, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.SecondMethodRPC)));
                Assert.AreEqual(1, Interactor.Calls.GetValueOrDefault(nameof(ExampleInteractor.ReplacementSecondMethodRPC)));
                Interactor.Calls.Clear();
            }
        }
        private static void ShouldReplaceRPCMethodFunc()
        {
            StaticRPCTest staticRPCTest = new StaticRPCTest();
            LocalRPCTest localRPCTest = new LocalRPCTest();
            staticRPCTest.Setup();
            localRPCTest.Setup();
            ReplacementInteractionsPlugin.ReplaceRPCs();
            staticRPCTest.Test();
            localRPCTest.Test();
        }
        private static void Check(List<InteractionAttribute> list, string expectedMethodName)
        {
            Log.WriteLine(Localizer.Do($"Check {list.Select(x => x is ReplacementInteractionAttribute r ? $"[{r.MethodName}->{x.RPCName}]" : $"<{x.RPCName}>").CommaList()}"));

            ReplacementInteractionsPlugin.ReplaceInteractions(new Dictionary<Type, List<InteractionAttribute>>() { { typeof(ExampleInteractor), list } });
            Assert.AreEqual(1, list.Count);
            var interaction = list[0];
            Assert.AreEqual(InteractionTrigger.LeftClick, interaction.TriggerInfo.Trigger);
            Assert.AreEqual(expectedMethodName, interaction.RPCName);
            Log.WriteLine(Localizer.Do($"Result:{interaction.RPCName} named {interaction.Description}"));
        }
        public partial class ExampleInteractor
        {
            public Dictionary<string, int> Calls { get; set; } = new Dictionary<string, int>();
            public void OriginalMethod(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(OriginalMethod), 1, (num, i) => num + i); }
            public void OriginalMethod2(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(OriginalMethod2), 1, (num, i) => num + i); }
            public void OriginalMethod3(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(OriginalMethod3), 1, (num, i) => num + i); }
            public void ReplacementMethod1(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(ReplacementMethod1), 1, (num, i) => num + i); }
            public void ReplacementMethod2(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(ReplacementMethod2), 1, (num, i) => num + i); }
            public static InteractionAttribute GetReplacementInteraction() => new InteractionAttribute(InteractionTrigger.RightClick);
            public static InteractionAttribute IncorrectReplacementInteraction(object obj) => new InteractionAttribute(InteractionTrigger.RightClick);
            public static void IncorrectReplacementInteraction() { }
            [RPC]
            public void OriginalMethodRPC(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(OriginalMethodRPC), 1, (num, i) => num + i); }
        }
        public class ExampleInteractorChildClass : ExampleInteractor
        {
        }
        [DefinesInteractions]
        public static class ExampleInteractionReplacer
        {
            [AdditionalInteraction(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethod))]
            public static InteractionAttribute GetAdditionalInteraction(Type interactorType) => new InteractionAttribute(InteractionTrigger.InteractKey);

            [AdditionalInteraction(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethod2))]
            public static InteractionAttribute GetAdditionalInteractionOnDerivedClass(Type interactorType) => interactorType == typeof(ExampleInteractorChildClass) ? new InteractionAttribute(InteractionTrigger.InteractKey) : null;

            [AdditionalInteraction(typeof(ExampleInteractor), "MissingMethod")]
            public static InteractionAttribute GetAdditionalInteractionForMissingMethod(Type interactorType) => new InteractionAttribute(InteractionTrigger.RightClick);

            [ModifyInteraction(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethod))]
            public static void ModifyInteractionOnOriginalMethod(Type interactorType, ref InteractionAttribute interaction)
            {
                if (interaction.TriggerInfo.Trigger == InteractionTrigger.LeftClick)
                {
                    interaction.TriggerInfo = new InteractionTriggerInfo(InteractionTrigger.RightClick, interaction.TriggerInfo.Modifier);
                }
            }
            [ModifyInteraction(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethod3))]
            public static void RemoveInteractionOnOriginalMethod3(Type interactorType, ref InteractionAttribute interaction)
            {
                if (interactorType == typeof(ExampleInteractorChildClass))
                {
                    interaction = null;
                }
            }
            [ModifyInteraction(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethod3))]
            public static void RedirectInteractionOnOriginalMethod3(Type interactorType, ref InteractionAttribute interaction)
            {
                interaction.RPCName = nameof(ExampleInteractor.OriginalMethod);
            }
        }
        [DefinesInteractions]
        public partial class ExampleInteractor
        {
            [ReplacementInteraction(nameof(OriginalMethod))]
            public void ReplacesOriginalMethod(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { }
            [RPC]
            public void SecondMethodRPC() { Calls.AddOrUpdate(nameof(SecondMethodRPC), 1, (num, i) => num + i); }
            [ReplacementRPC(nameof(SecondMethodRPC))]
            public void ReplacementSecondMethodRPC() { Calls.AddOrUpdate(nameof(ReplacementSecondMethodRPC), 1, (num, i) => num + i); }
        }
        [DefinesInteractions]
        public class ExampleRPCReplacer
        {
            public static Dictionary<string, int> Calls { get; set; } = new Dictionary<string, int>();
            [ReplacementRPC(typeof(ExampleInteractor), nameof(ExampleRPCReplacer.ReplacementRPC))]
            [ReplacementRPC(typeof(ExampleInteractor), nameof(ExampleRPCReplacer.ReplacementRPCAlternative))]
            public static void ReplacementRPCPatch(ExampleInteractor interactor, Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(ReplacementRPCPatch), 1, (num, i) => num + i); }
            [ReplacementRPC(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethodRPC))]
            public static void ReplacementRPCAlternative(ExampleInteractor interactor, Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(ReplacementRPCAlternative), 1, (num, i) => num + i); }
            [ReplacementRPC(typeof(ExampleInteractor), nameof(ExampleInteractor.OriginalMethodRPC))]
            public static void ReplacementRPC(ExampleInteractor interactor, Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target) { Calls.AddOrUpdate(nameof(ReplacementRPC), 1, (num, i) => num + i); }
        }
    }
}
