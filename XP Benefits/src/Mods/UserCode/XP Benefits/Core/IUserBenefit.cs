using Eco.Gameplay.Players;

namespace XPBenefits
{
    public interface IUserBenefit
    {
        /// <summary>
        /// Whether the benefit is enabled on the server.
        /// It should be determined before the XP Benefits plugin initializes.
        /// Ecopedia pages won't be generated for disabled benefits.
        /// </summary>
        bool Enabled { get; }
        public void ApplyBenefitToUser(User user);
        public void RemoveBenefitFromUser(User user);
        /// <summary>
        /// Called during the XP Benefits plugin Initialize.
        /// Can be used for any setup that can't be done until the XP Benefits config and the available calculation types are loaded.
        /// </summary>
        void Initialize();
    }
}
