namespace PcKod.UI.Models
{
    public class SatisAnalizModel
    {
        public string MehsulAdi { get; set; }
        public double ToplamMiqdar { get; set; }
        public double GrafikGenişligi { get; set; }
        public string MiktarGosterim => ToplamMiqdar.ToString("N2");
    }

    public class KritikStokModel
    {
        public string MehsulAdi { get; set; }
        public double KalanMiqdar { get; set; }
        public string Birim { get; set; }
        public string KalanGosterim => $"{KalanMiqdar:N2} {Birim}";
    }
}