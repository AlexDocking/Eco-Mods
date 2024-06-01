using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            float skillGainMultiplier = DifficultySettings.SkillGainMultiplier;
            DifficultySettings.SkillGainMultiplier = 1;
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
                DifficultySettings.SkillGainMultiplier = skillGainMultiplier;
            }
        }
    }
}
