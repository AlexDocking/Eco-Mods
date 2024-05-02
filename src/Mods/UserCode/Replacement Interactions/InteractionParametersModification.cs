using Eco.Gameplay.Interactions.Interactors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplacementInteractions
{
    public class InteractionParametersModification
    {
        public Type InteractorType { get; set; }
        public string InteractorMethodName { get; set; }
        public Func<Type, InteractionAttribute, InteractionAttribute> ModificationMethod { get; set; }
    }
}
