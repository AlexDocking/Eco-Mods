using Eco.Core.Tests;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Ecompatible;
using EcoTestTools;

namespace EcompatibleTools.Tests
{
    [ChatCommandHandler]
    public class TestResolverDescriptions
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestDescriptions()
        {
            Test.Run(ShouldResolveCorrectValue);
            Test.Run(ShouldGenerateDescriptionOfEachStep, "Generate ecompatible resolver descriptions");
        }
        private static float ExampleResolve(out AuxillaryInfo info)
        {
            User user = TestUtils.TestUser;
            IValueModificationContext context = new ExampleContext() { User = user };
            (float, IValueModifier)[] modifiers = new (float, IValueModifier)[]
            {
                (1, new ExampleBaseModifier()),
                (2, new ExampleMultiplierModifier(0.5f)),
                (3, new ExampleBaseModifier()),//Overwrites all previous values, so only display from here onwards. Displays as "Base Level: 5"
                (4, new ExampleNoOpModifier()),//No modification, so ignore
                (5, new ExampleMultiplierModifier(1.3f))//Displays as "Example Multiplier: +30%"
            };
            PriorityDynamicValueResolver resolver = new PriorityDynamicValueResolver(modifiers);
            return resolver.Resolve(context, out info);
        }
        private static void ShouldResolveCorrectValue()
        {
            float value = ExampleResolve(out _);
            Assert.AreEqual(5 * 1.3f, value);
        }
        private static void ShouldGenerateDescriptionOfEachStep()
        {
            ExampleResolve(out AuxillaryInfo info);
            LocString description = DescriptionGenerator.Obj.BuildModificationListDescriptionInt(info);

            Log.WriteLine(description);
            string expected = "<table>\r\n<tr><th><![CDATA[Base Level:]]></th><th><![CDATA[<align=\"right\">5</align>]]></th></tr><tr><th><![CDATA[Example Multiplier:]]></th><th><![CDATA[<align=\"right\"><style=\"Positive\">+30%</style></align>]]></th></tr><tr><th><![CDATA[---------------------------]]></th><th><![CDATA[]]></th></tr><tr><th><![CDATA[Result:]]></th><th><![CDATA[<align=\"right\">6</align>]]></th></tr></table>\r\n";
            Assert.AreEqual(expected, description.ToString());
        }
    }

    internal class ExampleContext : IValueModificationContext
    {
        public User User { get; set; }

        public float FloatValue { get; set; }
        public int IntValue { get; set; }
    }
    internal class ExampleBaseModifier : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails modificationDetails)
        {
            context.FloatValue = 5;
            context.IntValue = 5;
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
            context.IntValue = (int)context.FloatValue;
            modificationDetails = new MultiplicationOperationDetails(Localizer.DoStr("Example Multiplier"), multiplier);
        }
    }
}
