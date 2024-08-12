using Eco.Core.Tests;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using EcoTestTools;
using System;

namespace Ecompatible.Tests
{
    [ChatCommandHandler]
    public class TestResolverDescriptions
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
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
            Context = new ExampleContext();
            (float, IValueModifier<float>)[] modifiers = new (float, IValueModifier<float>)[]
            {
                (1, new ExampleBaseModifier()),
                (2, new ExampleMultiplierModifier(0.5f)),
                (3, new ExampleBaseModifier()),//Overwrites all previous values, so only display from here onwards. Displays as "Base Level: 5"
                (4, new ExampleNoOpModifier()),//No modification, so ignore
                (5, new ExampleMultiplierModifier(1.3f))//Displays as "Example Multiplier: +30%"
            };
            Resolver = new PriorityValueResolver<float>(modifiers);
        }

        private static ExampleContext Context { get; set; }
        private static PriorityValueResolver<float> Resolver { get; set; }
        private static void ShouldResolveCorrectFloatValue()
        {
            float value = Resolver.Resolve(0, Context, out _);
            Assert.AreEqual(5 * 1.3f, value);
        }
        private static void ShouldRoundDownByDefault()
        {
            int value = Resolver.ResolveInt(0, Context, out _);
            Assert.AreEqual((int)(5 * 1.3f), value);
        }
        private static void ShouldRoundUp()
        {
            int value = Resolver.ResolveInt(0, Context, out _, Rounding.RoundUp);
            Assert.AreEqual((int)Math.Ceiling(5 * 1.3f), value);
        }
        private static void ShouldGenerateDescriptionOfEachStep()
        {
            int intOutput = Resolver.ResolveInt(0, Context, out AuxillaryInfo<float> info, Rounding.RoundDown);
            LocString description = DescriptionGenerator.Obj.BuildModificationListDescriptionInt(intOutput, info);

            string expected = "<table>\r\n<tr><th><![CDATA[Base Level:]]></th><th><![CDATA[<align=\"right\">5</align>]]></th></tr><tr><th><![CDATA[Example Multiplier:]]></th><th><![CDATA[<align=\"right\"><style=\"Positive\">+30%</style></align>]]></th></tr><tr><th><![CDATA[---------------------------]]></th><th><![CDATA[]]></th></tr><tr><th><![CDATA[Result:]]></th><th><![CDATA[<align=\"right\">6</align>]]></th></tr></table>\r\n";
            Assert.AreEqual(expected, description.ToString());
        }
    }

    internal class ExampleContext : IValueModificationContext
    {
    }
    internal class ExampleBaseModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            return new BaseLevelModificationOutput(5);
        }
    }
    internal class ExampleNoOpModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            return null;
        }
    }
    internal class ExampleMultiplierModifier : IValueModifier<float>
    {
        public ExampleMultiplierModifier(float multiplier)
        {
            Multiplier = multiplier;
        }

        public float Multiplier { get; }

        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            float multiplier = Multiplier;
            float output = functionInput.Input * multiplier;
            return new MultiplicationOperationDetails(output, Localizer.DoStr("Example Multiplier"), multiplier);
        }
    }
}
