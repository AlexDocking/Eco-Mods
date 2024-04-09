//XP Benefits
//Copyright (C) 2023 Alex Docking
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace XPBenefits
{
    internal static class DisplayUtils
    {
        public static string GradientNum(float v, object text, Eco.Shared.Math.Range range)
        {
            float percent = range.PercentThrough(v);
            if (percent < 0.5)
            {
                return Text.Color(Color.Red.Lerp(Color.Yellow, percent * 2), text);
            }
            else
            {
                return Text.Color(Color.Yellow.Lerp(Color.Green, percent / 2), text);
            }
        }
        public static LocString GradientNumLoc(float v, object text, Eco.Shared.Math.Range range) => Localizer.DoStr(GradientNum(v, text, range));

    }
}