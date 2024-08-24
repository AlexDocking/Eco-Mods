// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.


using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;

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
