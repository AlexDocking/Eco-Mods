namespace XPBenefits
{
    public interface IBenefitFunctionFactory
    {
        string Name { get; }
        string Description { get; }
        IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false);
    }
}
