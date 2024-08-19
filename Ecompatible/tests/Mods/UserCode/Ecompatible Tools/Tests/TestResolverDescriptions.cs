using Eco.Core.Tests;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Localization;
using EcoTestTools;
using System;

namespace Ecompatible.Tests
{
    [ChatCommandHandler]
    public class TestResolverDescriptions
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestDescriptions()
        {
            Setup();
            Test.Run(ShouldResolveCorrectFloatValue);
            Test.Run(ShouldRoundDownByDefault);
            Test.Run(ShouldRoundUp);
            Test.Run(ShouldGenerateDescriptionOfEachStep, "Generate ecompatible resolver descriptions");
        }

        private static void Setup()
        {
            Context = Ecompatible.Context.CreateContext((new ContextKey(typeof(float), "ContextMultiplier"), 1.3f));
            (float, IValueModifier<float>)[] modifiers = new (float, IValueModifier<float>)[]
            {
                (0, new ExampleNoOpModifier()),
                (1, new ExampleBaseModifier()),
                (2, new ExampleMultiplierModifier(0.5f)),
                (3, new ExampleBaseModifier()),//Overwrites all previous values, so only display from here onwards. Displays as "Base Level: 5"
                (4, new ExampleNoOpModifier()),//No modification, so ignore
                (5, new EnsureValueIsAtLeast(6)),//Overwrite previous value because the running total is too small, while still displaying the previous values except with lines through them
                (6, new ExampleContextMultiplierModifier()),//Should read the multiplier value from the context. Displays as "Example Context Multiplier: +30%"
                (7, new ExampleNoOpModifier())
            };
            Resolver = ValueResolverFactory.CreatePriorityResolver<float>(modifiers);
        }

        private static IContext Context { get; set; }
        private static IPriorityValueResolver<float> Resolver { get; set; }
        private static void ShouldResolveCorrectFloatValue()
        {
            float value = Resolver.Resolve(0, Context, out _);
            Assert.AreEqual(6 * 1.3f, value);
        }
        private static void ShouldRoundDownByDefault()
        {
            int value = Resolver.ResolveInt(0, Context, out _);
            Assert.AreEqual((int)(6 * 1.3f), value);
        }
        private static void ShouldRoundUp()
        {
            int value = Resolver.ResolveInt(0, Context, out _, Rounding.RoundUp);
            Assert.AreEqual((int)Math.Ceiling(6 * 1.3f), value);
        }
        private static void ShouldGenerateDescriptionOfEachStep()
        {
            IResolvedSequence<float> resolvedSequence = Resolver.ResolveSequence(0, Context);
            LocString description = DescriptionGenerator.Obj.DescribeSequenceAsTableAndRoundDown(resolvedSequence);

            string expected = "<table>\r\n<tr><th><![CDATA[Base Level:]]></th><th><![CDATA[<align=\"right\"><s>5</s></align>]]></th></tr><tr><th><![CDATA[Must be at least 6 (got 5):]]></th><th><![CDATA[<align=\"right\">6</align>]]></th></tr><tr><th><![CDATA[Example Context Multiplier:]]></th><th><![CDATA[<align=\"right\"><style=\"Positive\">+30%</style></align>]]></th></tr><tr><th><![CDATA[---------------------------]]></th><th><![CDATA[]]></th></tr><tr><th><![CDATA[Result (rounded down):]]></th><th><![CDATA[<align=\"right\">7</align>]]></th></tr></table>\r\n";
            Assert.AreEqual(expected, description.ToString());
        }
    }

    internal class ExampleContext : IContext
    {
    }
    internal class ExampleBaseModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            return Output.BaseLevel(5);
        }
    }
    internal class ExampleNoOpModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            return null;
        }
    }
    internal class ExampleContextMultiplierModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (!context.TryGetNonNull(new ContextKey<float>("ContextMultiplier"), out float multiplier)) return null;
            float output = functionInput.Input * multiplier;
            return Output.Multiplier(output, Localizer.DoStr("Example Context Multiplier"), multiplier);
        }
    }
    internal class ExampleMultiplierModifier : IValueModifier<float>
    {
        public float Multiplier { get; }

        public ExampleMultiplierModifier(float multiplier)
        {
            Multiplier = multiplier;
        }

        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            float multiplier = Multiplier;
            float output = functionInput.Input * multiplier;
            return Output.Multiplier(output, Localizer.DoStr("Example Multiplier"), multiplier);
        }
    }
}
