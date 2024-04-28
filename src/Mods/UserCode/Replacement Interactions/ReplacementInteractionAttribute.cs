using System;

namespace Eco.Gameplay.Interactions.Interactors
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReplacementInteractionAttribute : InteractionAttribute
    {
        /// <summary>
        /// Change an existing interaction to use this method, keeping the original interaction parameters
        /// </summary>
        public ReplacementInteractionAttribute(string methodName) : base(default)
        {
            MethodName = methodName;
            CopyParameters = true;
        }
        public string MethodName { get; }
        public bool CopyParameters { get; }
    }
}
