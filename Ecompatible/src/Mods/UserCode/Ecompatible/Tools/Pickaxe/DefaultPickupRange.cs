using Eco.Gameplay.Players;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    internal sealed class DefaultPickupRange<TContext> : IValueModifier<float, TContext> where TContext : IUserContext
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            var context = functionInput.Context;
            User user = context.User;
            if (user.Talentset.GetTalent<MiningSweepingHandsTalent>() is not MiningSweepingHandsTalent sweepingHands) return null;
            return OutputFactory.BaseLevel(sweepingHands.PickUpRange);
        }
    }
}
