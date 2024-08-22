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
            Context = new ExampleContext(contextMultipler: 1.3f);
            (float, IValueModifier<float, ExampleContext>)[] modifiers = new (float, IValueModifier<float, ExampleContext>)[]
            {
                (0, new ExampleNoOpModifier<ExampleContext>()),
                (1, new ExampleBaseModifier<ExampleContext>()),
                (2, new ExampleMultiplierModifier<ExampleContext>(0.5f)),
                (3, new ExampleBaseModifier<ExampleContext>()),//Overwrites all previous values, so only display from here onwards. Displays as "Base Level: 5"
                (4, new ExampleNoOpModifier<ExampleContext>()),//No modification, so ignore
                (5, new EnsureValueIsAtLeast<ExampleContext>(6)),//Overwrite previous value because the running total is too small, while still displaying the previous values except with lines through them
                (6, new ExampleContextMultiplierModifier<ExampleContext>()),//Should read the multiplier value from the context. Displays as "Example Context Multiplier: +30%"
                (7, new ExampleNoOpModifier<ExampleContext>())
            };
            Resolver = ValueResolverFactory.CreatePriorityResolver<float, ExampleContext>(modifiers);
        }

        private static ExampleContext Context { get; set; }
        private static IPriorityValueResolver<float, ExampleContext> Resolver { get; set; }
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
            IResolvedSequence<float, ExampleContext> resolvedSequence = Resolver.ResolveSequence(0, Context);
            LocString description = DescriptionGenerator.Obj.DescribeSequenceAsTableAndRoundDown(resolvedSequence);

            string expected = "<table>\r\n<tr><th><![CDATA[Base Level:]]></th><th><![CDATA[<align=\"right\"><s>5</s></align>]]></th></tr><tr><th><![CDATA[Must be at least 6 (got 5):]]></th><th><![CDATA[<align=\"right\">6</align>]]></th></tr><tr><th><![CDATA[Example Context Multiplier:]]></th><th><![CDATA[<align=\"right\"><style=\"Positive\">+30%</style></align>]]></th></tr><tr><th><![CDATA[---------------------------]]></th><th><![CDATA[]]></th></tr><tr><th><![CDATA[Result (rounded down):]]></th><th><![CDATA[<align=\"right\">7</align>]]></th></tr></table>\r\n";
            Assert.AreEqual(expected, description.ToString());
        }
    }

    internal class ExampleContext : IContext
    {
        public ExampleContext(float contextMultipler)
        {
            this.ContextMultiplier = contextMultipler;
        }

        public float ContextMultiplier { get; }
    }
    internal class ExampleBaseModifier<TContext> : IValueModifier<float, TContext> where TContext : IContext
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            return OutputFactory.BaseLevel(5);
        }
    }
    internal class ExampleNoOpModifier<TContext> : IValueModifier<float, TContext> where TContext : IContext
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            return null;
        }
    }
    internal class ExampleContextMultiplierModifier<TContext> : IValueModifier<float, TContext> where TContext : ExampleContext
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            var context = functionInput.Context;
            float multiplier = context.ContextMultiplier;
            float output = functionInput.Input * multiplier;
            return OutputFactory.Multiplier(output, Localizer.DoStr("Example Context Multiplier"), multiplier);
        }
    }
    internal class ExampleMultiplierModifier<TContext> : IValueModifier<float, TContext> where TContext : IContext
    {
        public float Multiplier { get; }

        public ExampleMultiplierModifier(float multiplier)
        {
            Multiplier = multiplier;
        }

        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            var context = functionInput.Context;
            float multiplier = Multiplier;
            float output = functionInput.Input * multiplier;
            return OutputFactory.Multiplier(output, Localizer.DoStr("Example Multiplier"), multiplier);
        }
    }
}
