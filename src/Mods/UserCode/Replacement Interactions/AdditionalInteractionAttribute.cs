using System;

namespace ReplacementInteractions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AdditionalInteractionAttribute : Attribute
    {
        public AdditionalInteractionAttribute(Type interactorType, string method)
        {
            InteractorType = interactorType;
            Method = method;
        }

        public Type InteractorType { get; }
        public string Method { get; }
    }
}
