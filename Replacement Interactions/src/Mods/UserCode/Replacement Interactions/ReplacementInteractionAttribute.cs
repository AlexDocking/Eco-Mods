using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Linq;
using System.Reflection;

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
        }
        public ReplacementInteractionAttribute(string methodName, string interactionParametersGetter) : base(default)
        {
            MethodName = methodName;
            InteractionParametersGetter = interactionParametersGetter;
        }
        public string MethodName { get; }
        public string InteractionParametersGetter { get; }
        public InteractionAttribute GetReplacementInteraction(Type classType)
        {
            if (CopyParameters) return null;
            var getter = classType.GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(method => method.Name == InteractionParametersGetter && !method.GetParameters().Any() && method.ReturnParameter?.ParameterType == typeof(InteractionAttribute));
            if (getter == null) throw new MissingMethodException($"Could not find static method {InteractionParametersGetter} on type {classType} with no parameters with return type {typeof(InteractionAttribute)}", nameof(InteractionParametersGetter));
            InteractionAttribute interaction = getter.Invoke(null, null) as InteractionAttribute;
            if (interaction == null) throw new NullReferenceException($"Method {InteractionParametersGetter} on {classType} returned null. Expected {typeof(InteractionAttribute)}");
            return interaction;
        }
        public bool CopyParameters => string.IsNullOrEmpty(InteractionParametersGetter);
    }
}
