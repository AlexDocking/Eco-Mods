using System;

namespace ReplacementInteractions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ModifyInteractionAttribute : Attribute
    {
        public ModifyInteractionAttribute(Type interactorType, string method)
        {
            InteractorType = interactorType;
            Method = method;
        }

        public Type InteractorType { get; }
        public string Method { get; }
    }
}
