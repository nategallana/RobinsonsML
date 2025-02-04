using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public static class DecimalExtensions
    {
        public static string ToDecimalPlaces(this decimal value, int decimalPlaces)
        {
            if (decimalPlaces == 0)
            {
                return value.ToString("F0") + ".00";
            }
            else
            {
                string format = "F" + decimalPlaces.ToString();
                return value.ToString(format);
            }
        }
    }
}
