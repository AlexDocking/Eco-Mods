using Eco.Gameplay.Interactions.Interactors;
using System;

namespace ReplacementInteractions
{
    public class InteractionParametersModification
    {
        public Type InteractorType { get; set; }
        public string InteractorMethodName { get; set; }
        public Func<Type, InteractionAttribute, InteractionAttribute> ModificationMethod { get; set; }
    }
}
