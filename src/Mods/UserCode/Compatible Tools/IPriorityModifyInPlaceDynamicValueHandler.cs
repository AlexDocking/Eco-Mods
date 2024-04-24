namespace CompatibleTools
{
    public interface IPriorityModifyInPlaceDynamicValueHandler
    {
        float Priority { get; }
        void ModifyValue(IModifyInPlaceDynamicValueContext context);
    }
}
