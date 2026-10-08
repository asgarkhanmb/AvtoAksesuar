namespace PcKod.UI.Data.Helpers
{
    public static class BarcodeParser
    {
        public static (string ProductCode, double Weight) Parse(string rawBarcode)
        {
            if (string.IsNullOrWhiteSpace(rawBarcode)) return (string.Empty, 1.0);

            if (rawBarcode.Length == 13 && (rawBarcode.StartsWith("27") || rawBarcode.StartsWith("28") || rawBarcode.StartsWith("29")))
            {
                string code = rawBarcode.Substring(2, 5); // 
                if (double.TryParse(rawBarcode.Substring(7, 5), out double gram))
                {
                    return (code, gram / 1000.0); 
                }
            }
            return (rawBarcode, 1.0);
        }
    }
}