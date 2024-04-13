namespace CompatibleTools
{
    public interface IMaxTakeModifier
    {
        float Priority { get; }
        void ModifyMaxTake(ShovelMaxTakeModification modification);
    }
}
