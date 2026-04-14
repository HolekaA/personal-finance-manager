using System;

namespace osobniSpravceFinanci.Models
{
    public enum TypTransakce
    {
        Prijem,
        Vydaj
    }

    public class Kategorie
    {
        public int Id { get; set; }
        public string Nazev { get; set; }
        public string Barva { get; set; }
        public bool JeAktivni { get; set; } = true;
    }

    public class SablonaPlatby
    {
        public int Id { get; set; }
        public string Nazev { get; set; }
        public decimal Castka { get; set; }
        public TypTransakce Typ { get; set; }
        public int KategorieId { get; set; }
    }

    public class Transakce
    {
        public int Id { get; set; }
        public string Nazev { get; set; } = "";
        public decimal Castka { get; set; }
        public DateTime Datum { get; set; }
        public TypTransakce Typ { get; set; }
        public int KategorieId { get; set; }
        public int? SablonaId { get; set; }
    }

    public class SporiciCil
    {
        public int Id { get; set; }
        public string Nazev { get; set; } = "";
        public decimal CilovaCastka { get; set; }
        public DateTime DatumVytvoreni { get; set; }
    }

    public class VkladNaCil
    {
        public int Id { get; set; }
        public int SporiciCilId { get; set; }
        public int? TransakceId { get; set; }
        public decimal VlozenaCastka { get; set; }
        public DateTime DatumVkladu { get; set; }
    }

    public class VygenerovanyMesic
    {
        public int Id { get; set; }
        public int Rok { get; set; }
        public int Mesic { get; set; }
    }
}