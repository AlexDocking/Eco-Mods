using Eco.Core.Tests;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Ecompatible;
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
            User user = TestUtils.TestUser;
            Context = new ExampleContext() { User = user };
            (float, IValueModifier)[] modifiers = new (float, IValueModifier)[]
            {
                (1, new ExampleBaseModifier()),
                (2, new ExampleMultiplierModifier(0.5f)),
                (3, new ExampleBaseModifier()),//Overwrites all previous values, so only display from here onwards. Displays as "Base Level: 5"
                (4, new ExampleNoOpModifier()),//No modification, so ignore
                (5, new ExampleMultiplierModifier(1.3f))//Displays as "Example Multiplier: +30%"
            };
            Resolver = new PriorityDynamicValueResolver(modifiers);
        }

        private static ExampleContext Context { get; set; }
        private static PriorityDynamicValueResolver Resolver { get; set; }
        private static void ShouldResolveCorrectFloatValue()
        {
            float value = Resolver.Resolve(Context, out _);
            Assert.AreEqual(5 * 1.3f, value);
        }
        private static void ShouldRoundDownByDefault()
        {
            int value = Resolver.ResolveInt(Context, out _);
            Assert.AreEqual((int)(5 * 1.3f), value);
        }
        private static void ShouldRoundUp()
        {
            int value = Resolver.ResolveInt(Context, out _, Rounding.RoundUp);
            Assert.AreEqual((int)Math.Ceiling(5 * 1.3f), value);
        }
        private static void ShouldGenerateDescriptionOfEachStep()
        {
            Resolver.Resolve(Context, out AuxillaryInfo info);
            LocString description = DescriptionGenerator.Obj.BuildModificationListDescriptionInt(info);

            string expected = "<table>\r\n<tr><th><![CDATA[Base Level:]]></th><th><![CDATA[<align=\"right\">5</align>]]></th></tr><tr><th><![CDATA[Example Multiplier:]]></th><th><![CDATA[<align=\"right\"><style=\"Positive\">+30%</style></align>]]></th></tr><tr><th><![CDATA[---------------------------]]></th><th><![CDATA[]]></th></tr><tr><th><![CDATA[Result:]]></th><th><![CDATA[<align=\"right\">6</align>]]></th></tr></table>\r\n";
            Assert.AreEqual(expected, description.ToString());
        }
    }

    internal class ExampleContext : IValueModificationContext
    {
        public User User { get; set; }

        public float FloatValue { get; set; }
    }
    internal class ExampleBaseModifier : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails modificationDetails)
        {
            context.FloatValue = 5;
            modificationDetails = new BaseLevelOperationDetails();
        }
    }
    internal class ExampleNoOpModifier : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails modificationDetails)
        {
        }
    }
    internal class ExampleMultiplierModifier : IValueModifier
    {
        public ExampleMultiplierModifier(float multiplier)
        {
            Multiplier = multiplier;
        }

        public float Multiplier { get; }

        public void ModifyValue(IValueModificationContext context, ref IOperationDetails modificationDetails)
        {
            float multiplier = Multiplier;
            context.FloatValue *= multiplier;
            modificationDetails = new MultiplicationOperationDetails(Localizer.DoStr("Example Multiplier"), multiplier);
        }
    }
}
