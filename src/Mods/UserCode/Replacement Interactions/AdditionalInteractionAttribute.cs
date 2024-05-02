using System;

namespace ReplacementInteractions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AdditionalInteractionAttribute : Attribute
    {
        public AdditionalInteractionAttribute(Type interactorType, string method)
        {
            InteractorType = interactorType;
            MethodName = method;
        }

        public Type InteractorType { get; }
        public string MethodName { get; }
    }
}
