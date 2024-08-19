using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecompatible
{
    public class BaseLevelModifier : IValueModifier<float>
    {
        public float BaseValue { get; }

        public BaseLevelModifier(float baseValue)
        {
            BaseValue = baseValue;
        }

        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            return new BaseLevelModificationOutput(BaseValue);
        }
    }
}
