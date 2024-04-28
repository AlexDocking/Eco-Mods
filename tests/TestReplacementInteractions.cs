using Eco.Core.Tests;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        private static void ShouldReplaceSingleInteraction()
        {
            var interactionList = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
            }; 
            Check(interactionList, nameof(ExampleClass.ReplacementMethod1));
        }
        private static void ShouldReplaceChainedInteractions()
        {
            var interactionList = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.ReplacementMethod1)) { RPCName = nameof(ExampleClass.ReplacementMethod2) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
            };

            var reversedInteractionList = new List<InteractionAttribute>()
            {
                new InteractionAttribute(InteractionTrigger.LeftClick) { RPCName = nameof(ExampleClass.OriginalMethod) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.ReplacementMethod1)) { RPCName = nameof(ExampleClass.ReplacementMethod2) },
                new ReplacementInteractionAttribute(nameof(ExampleClass.OriginalMethod)) { RPCName = nameof(ExampleClass.ReplacementMethod1) },
            };

            reversedInteractionList.Reverse();

            Log.WriteLine(Localizer.Do($"Check forward"));
            Check(interactionList, nameof(ExampleClass.ReplacementMethod2));
            Log.WriteLine(Localizer.Do($"Check backward"));
            Check(reversedInteractionList, nameof(ExampleClass.ReplacementMethod2));
            
        }
        static void Check(List<InteractionAttribute> list, string expectedMethodName)
        {
            ReplacementInteractionsPlugin.ReplaceInteractions(list.SingleItemAsEnumerable());
            Assert.AreEqual(1, list.Count);
            var interaction = list[0];
            Assert.AreEqual(InteractionTrigger.LeftClick, interaction.TriggerInfo.Trigger);
            Assert.AreEqual(expectedMethodName, interaction.RPCName);
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
