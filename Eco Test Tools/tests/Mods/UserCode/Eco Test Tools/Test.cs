using Eco.Gameplay.Systems.Balance;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using System;

namespace EcoTestTools
{
    public static class Test
    {
        /// <summary>
        /// Unhandled exceptions in tests will cause the server to shut down and not run
        /// any remaining tests, so we need to catch any exceptions the tests throw
        /// </summary>
        /// <param name="test"></param>
        public static void Run(Action test, string name = null)
        {
            float skillGainMultiplier = BalancePlugin.Obj.Config.SkillGainMultiplier;
            BalancePlugin.Obj.Config.SkillGainMultiplier = 1;
            try
            {
                Log.WriteLine(Localizer.Do($"Running sub-test {name}"));
                test();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
            finally
            {
                BalancePlugin.Obj.Config.SkillGainMultiplier = skillGainMultiplier;
            }
        }
    }
}
