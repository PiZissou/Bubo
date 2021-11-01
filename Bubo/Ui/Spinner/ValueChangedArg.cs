using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class ValueChangedArg : EventArgs
    {
        public Double NewValue;
        public Double OldValue;

        public ValueChangedArg(double oldValue, double newValue)
            : base()
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}
