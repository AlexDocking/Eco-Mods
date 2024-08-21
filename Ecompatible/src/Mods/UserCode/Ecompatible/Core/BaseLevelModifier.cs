namespace Ecompatible
{
    public sealed class BaseLevelModifier<TContext> : IValueModifier<float, TContext> where TContext : IContext
    {
        public float BaseValue { get; }

        public BaseLevelModifier(float baseValue)
        {
            BaseValue = baseValue;
        }

        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            return OutputFactory.BaseLevel(BaseValue);
        }
    }
}
