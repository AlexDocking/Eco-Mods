using System;

namespace XPBenefits
{
    internal static class FuncExtensions
    {
        public static T GetOrDefault<T>(this Func<T> getterMaybeNull) where T : struct
        {
            return getterMaybeNull?.Invoke() ?? default;
        }
    }
}