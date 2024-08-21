using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecompatible
{
    public static partial class ContextFactory
    {
        public static ITreeFelledContext CreateTreeFelledContext(User user, AxeItem axe, Tree tree)
        {
            return new TreeFelledContext(user, axe, tree);
        }
    }
    public interface ITreeFelledContext : IUserContext
    {
        AxeItem Axe { get; }
        Tree Tree { get; }
        TreeSpecies TreeSpecies { get; }
    }
    internal class TreeFelledContext : ITreeFelledContext
    {
        public TreeFelledContext(User user, AxeItem axe, Tree tree)
        {
            User = user;
            Axe = axe;
            Tree = tree;
            TreeSpecies = tree?.Species;
        }

        public AxeItem Axe { get; set; }

        public Tree Tree { get; set; }

        public TreeSpecies TreeSpecies { get; set; }

        public User User { get; set; }
    }
}
