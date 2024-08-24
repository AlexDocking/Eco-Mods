using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public class StackLimitBenefitInventoryRestriction : InventoryRestriction
    {
        public virtual IBenefitFunction BenefitFunction { get; }
        public StackLimitBenefitInventoryRestriction(User user, IBenefitFunction benefitFunction)
        {
            User = user;
            BenefitFunction = benefitFunction;
        }

        public override int MaxAccepted(Item item, int currentQuantity)
        {
            return (int)(item.MaxStackSize * GetStackLimitMultiplier());
        }

        private float GetStackLimitMultiplier()
        {
            float stackSizeBenefit = BenefitFunction.CalculateBenefit(User);
            return (1 + stackSizeBenefit);
        }

        public User User { get; }
        public BenefitValue MaximumBenefit { get; }

        public override bool SurpassStackSize => true;
        public override LocString Message => new LocString();
    }
}