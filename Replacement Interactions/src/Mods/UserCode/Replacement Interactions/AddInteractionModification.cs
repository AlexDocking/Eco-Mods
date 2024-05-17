using Eco.Gameplay.Interactions.Interactors;
using Eco.Shared.Utils;
using System;
using System.Reflection;

namespace ReplacementInteractions
{
    public class AddInteractionModification
    {
        public Type InteractorType { get; set; }
        public string InteractorMethodName { get; set; }
        public Func<Type, InteractionAttribute> InteractionCreationMethod { get; set; }
    }
    public static class AddInteractionModificationExtensions
    {
        public static void SetCreationMethod(this AddInteractionModification addModification, MethodInfo interactionCreationMethod)
        {
            if (interactionCreationMethod.IsStatic && interactionCreationMethod.VerifySignature(typeof(Type)) && interactionCreationMethod.ReturnType == typeof(InteractionAttribute))
            {
                addModification.InteractionCreationMethod = type =>
                {
                    var parameters = new object[] { type };
                    var ret = interactionCreationMethod.Invoke(null, parameters);
                    var interaction = ret as InteractionAttribute;
                    interaction?.Init(addModification.InteractorType, addModification.InteractorMethodName);
                    return interaction;
                };
            }
        }
    }
}
