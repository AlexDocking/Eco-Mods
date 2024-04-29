using Eco.Core.Tests;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplacementInteractions.Tests
{
    using Eco.Shared.Localization;
    using EcoTests;
    [ChatCommandHandler]
    public static class TestReplacementInteractions
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestReplacementInteractionPlugin()
        {
            Test.Run(ShouldReplaceSingleInteraction, nameof(ShouldReplaceSingleInteraction));
            Test.Run(ShouldReplaceChainedInteractions, nameof(ShouldReplaceChainedInteractions));
            Test.Run(ShouldIgnoreMissingInteractions, nameof(ShouldIgnoreMissingInteractions));
            Test.Run(ShouldThrowExceptionIfThereIsACycle, nameof(ShouldThrowExceptionIfThereIsACycle));
            Test.Run(ShouldTrackMultipleInteractionsOnSameMethod, nameof(ShouldTrackMultipleInteractionsOnSameMethod));
        }
        private static void ShouldReplaceSingleInteraction()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
            };
            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleClass.ReplacementMethod1));
            }
        }
        private static void ShouldReplaceChainedInteractions()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.ReplacementMethod1)) { RPCName = nameof(ExampleClass.ReplacementMethod2) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleClass.ReplacementMethod2));
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
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
                new ReplacementInteractionAttribute("MissingInteraction") { RPCName = nameof(ExampleClass.ReplacementMethod1) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.ReplacementMethod1)) { RPCName = nameof(ExampleClass.ReplacementMethod2) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList, nameof(ExampleClass.ReplacementMethod2));
            }
        }
        private static void ShouldThrowExceptionIfThereIsACycle()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.ReplacementMethod1)) { RPCName = nameof(ExampleClass.OriginalMethod) },
            };
            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Assert.Throws<InvalidOperationException>(() => ReplacementInteractionsPlugin.ReplaceInteractions(interactionList.SingleItemAsEnumerable()));
            }
        }
        private static void ShouldTrackMultipleInteractionsOnSameMethod()
        {
            List<InteractionAttribute> CreateList() => new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new InteractionAttribute(InteractionTrigger.RightClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
            };

            foreach (List<InteractionAttribute> interactionList in AllPermutationsOfAttributeList(CreateList))
            {
                Check(interactionList);
            }
            void Check(List<InteractionAttribute> list)
            {
                ReplacementInteractionsPlugin.ReplaceInteractions(list.SingleItemAsEnumerable());
                Log.WriteLine(Localizer.Do($"Multiple. {list.Select(x => x.RPCName).CommaList()}"));
                Assert.AreEqual(2, list.Count);
                var leftClickInteraction = list.FirstOrDefault(interaction => interaction.TriggerInfo.Trigger == InteractionTrigger.LeftClick);
                Assert.IsNotNull(leftClickInteraction);
                Assert.AreEqual(nameof(ExampleClass.ReplacementMethod1), leftClickInteraction.RPCName);
                var rightClickInteraction = list.FirstOrDefault(interaction => interaction.TriggerInfo.Trigger == InteractionTrigger.RightClick);
                Assert.IsNotNull(rightClickInteraction);
                Assert.AreEqual(nameof(ExampleClass.ReplacementMethod1), rightClickInteraction.RPCName);
            }
        }
        static void Check(List<InteractionAttribute> list, string expectedMethodName)
        {
            Log.WriteLine(Localizer.Do($"Check {list.Select(x => x is ReplacementInteractionAttribute r ? $"[{r.MethodName}->{x.RPCName}]" : $"<{x.RPCName}>").CommaList()}"));

            ReplacementInteractionsPlugin.ReplaceInteractions(list.SingleItemAsEnumerable());
            Assert.AreEqual(1, list.Count);
            var interaction = list[0];
            Assert.AreEqual(InteractionTrigger.LeftClick, interaction.TriggerInfo.Trigger);
            Assert.AreEqual(expectedMethodName, interaction.RPCName);
            Log.WriteLine(Localizer.Do($"Result:{interaction.RPCName} named {interaction.Description}"));
        }
        private class ExampleClass
        {
            public void OriginalMethod() { }
            public void ReplacementMethod1() { }
            public void ReplacementMethod2() { }
        }
    }
    namespace EcoTests
    {
        public class Assert
        {
            public static void AreEqual(object expected, object actual)
            {
                if (!Equals(expected, actual))
                {
                    throw new Exception($"AreEqual failed.\nExpected={expected}\nActual={actual}");
                }
            }
            public static void AreEqual(float expected, float actual, float delta = 0.0001f)
            {
                if (Math.Abs(expected - actual) > delta)
                {
                    throw new Exception($"AreEqual failed.\nExpected={expected}\nActual={actual}\nwith difference no greater than {delta}");
                }
            }
            public static void AreNotEqual(object notExpected, object actual)
            {
                if (Equals(notExpected, actual))
                {
                    throw new Exception($"AreNotEqual failed.\nNot Expected={notExpected}\nActual={actual}");
                }
            }
            public static void IsNull(object obj)
            {
                if (obj is not null)
                {
                    throw new Exception($"IsNull failed.\nGot={obj}");
                }
            }
            public static void IsNotNull(object obj)
            {
                if (obj is null)
                {
                    throw new Exception($"IsNotNull failed.");
                }
            }
            public static void IsTrue(bool value)
            {
                if (!value)
                {
                    throw new Exception($"IsTrue failed.");
                }
            }
            public static void IsFalse(bool value)
            {
                if (value)
                {
                    throw new Exception($"IsFalse failed.");
                }
            }
            public static void Throws<T>(Action action) where T : Exception
            {
                bool threwException = false;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (ex is not T) throw new Exception($"Action threw wrong exception. Expected {typeof(T).Name}, got {ex}");
                    threwException = true;
                }
                if (!threwException) throw new Exception($"Action did not throw exception. Expected {typeof(T).Name}");
            }
        }
        public static class Test
        {
            /// <summary>
            /// Unhandled exceptions in tests will cause the server to shut down and not run
            /// any remaining tests, so we need to catch any exceptions the tests throw
            /// </summary>
            /// <param name="test"></param>
            public static void Run(Action test, string name = null)
            {
                float skillGainMultiplier = DifficultySettings.SkillGainMultiplier;
                DifficultySettings.SkillGainMultiplier = 1;
                try
                {
                    Log.WriteLine(Localizer.Do($"Running sub-test {name}"));
                    test();
                }
                catch (Exception ex)
                {
                    Log.WriteException(ex);
                }
                finally
                {
                    DifficultySettings.SkillGainMultiplier = skillGainMultiplier;
                }
            }
        }
    }

}
