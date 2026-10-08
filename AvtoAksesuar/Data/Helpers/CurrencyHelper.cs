using System.Globalization;

namespace PcKod.UI.Data.Helpers
{
    public static class CurrencyHelper
    {
        public static readonly CultureInfo AzeCulture = new CultureInfo("azn-AZ");

        public static string ToAzn(this decimal value)
        {
            return value.ToString("C2", AzeCulture);
        }
    }
}