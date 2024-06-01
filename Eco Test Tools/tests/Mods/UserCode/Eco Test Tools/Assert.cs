using System;

namespace EcoTestTools
{
    public class Assert
    {
        public static void AreEqual(object expected, object actual)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception($"AreEqual failed.\nExpected={expected}\nActual={actual}");
            }
        }

        public static void AreEqual(float expected, float actual, float delta = 0.0001f)
        {
            if (Math.Abs(expected - actual) > delta)
            {
                throw new Exception($"AreEqual failed.\nExpected={expected}\nActual={actual}\nwith difference no greater than {delta}");
            }
        }

        public static void AreNotEqual(object notExpected, object actual)
        {
            if (Equals(notExpected, actual))
            {
                throw new Exception($"AreNotEqual failed.\nNot Expected={notExpected}\nActual={actual}");
            }
        }

        public static void IsNull(object obj)
        {
            if (obj is not null)
            {
                throw new Exception($"IsNull failed.\nGot={obj}");
            }
        }

        public static void IsNotNull(object obj)
        {
            if (obj is null)
            {
                throw new Exception($"IsNotNull failed.");
            }
        }

        public static void IsTrue(bool value)
        {
            if (!value)
            {
                throw new Exception($"IsTrue failed.");
            }
        }

        public static void IsFalse(bool value)
        {
            if (value)
            {
                throw new Exception($"IsFalse failed.");
            }
        }

        public static void Throws<T>(Action action) where T : Exception
        {
            bool threwException = false;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (ex is not T) throw new Exception($"Action threw wrong exception. Expected {typeof(T).Name}, got {ex}");
                threwException = true;
            }
            if (!threwException) throw new Exception($"Action did not throw exception. Expected {typeof(T).Name}");
        }
    }
}