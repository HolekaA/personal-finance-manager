using LiteDB;
using osobniSpravceFinanci.Models;
using System;
using System.Collections.Generic;

namespace osobniSpravceFinanci.Services
{
    public static class DatabaseSeeder
    {
        public static void NaplnitTestovaciData()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var transakceKolekce = db.GetCollection<Transakce>("transakce");

                // POJISTKA: Pokud už v databázi nějaké transakce jsou, nic neděláme
                if (transakceKolekce.Count() > 0) return;

                var kategorieKolekce = db.GetCollection<Kategorie>("kategorie");
                var cileKolekce = db.GetCollection<SporiciCil>("sporiciCile");
                var sablonyKolekce = db.GetCollection<SablonaPlatby>("sablony");
                var vkladyKolekce = db.GetCollection<VkladNaCil>("vkladyNaCile");

                // --- 1. VYTVOŘENÍ ROZŠÍŘENÝCH KATEGORIÍ ---
                var katJidlo = new Kategorie { Nazev = "Jídlo", Barva = "#FF3B30", JeAktivni = true };
                var katBydleni = new Kategorie { Nazev = "Bydlení", Barva = "#007AFF", JeAktivni = true };
                var katZabava = new Kategorie { Nazev = "Zábava", Barva = "#FFCC00", JeAktivni = true };
                var katDoprava = new Kategorie { Nazev = "Doprava", Barva = "#FF9500", JeAktivni = true };
                var katObleceni = new Kategorie { Nazev = "Oblečení", Barva = "#AF52DE", JeAktivni = true };
                var katZdravi = new Kategorie { Nazev = "Zdraví", Barva = "#FF2D55", JeAktivni = true };
                var katVyplata = new Kategorie { Nazev = "Výplata", Barva = "#34C759", JeAktivni = true };

                kategorieKolekce.Insert(new[] { katJidlo, katBydleni, katZabava, katDoprava, katObleceni, katZdravi, katVyplata });

                // Skryté kategorie
                var katSporeni = new Kategorie { Nazev = "Spoření", Barva = "#00C7BE", JeAktivni = false };
                var katNeznama = new Kategorie { Nazev = "Neznámá", Barva = "#808080", JeAktivni = false };
                kategorieKolekce.Insert(new[] { katSporeni, katNeznama });

                // --- 2. AUTOMATICKÝ GENERÁTOR TRANSAKCÍ (Leden 2025 - Duben 2026) ---
                Random rnd = new Random();

                // Cíle založíme předem, abychom do nich mohli v cyklu "sypat" peníze
                var cilDovolena = new SporiciCil { Nazev = "Letní dovolená", CilovaCastka = 40000, DatumVytvoreni = new DateTime(2025, 1, 1), JeAktivni = false }; // Koupili jsme v létě 2025
                var cilTelefon = new SporiciCil { Nazev = "Nový telefon", CilovaCastka = 25000, DatumVytvoreni = new DateTime(2025, 8, 1), JeAktivni = false }; // Koupili jsme na Vánoce 2025
                var cilAuto = new SporiciCil { Nazev = "Ojeté auto", CilovaCastka = 150000, DatumVytvoreni = new DateTime(2026, 1, 1), JeAktivni = true }; // Stále spoříme
                var cilKonzole = new SporiciCil { Nazev = "PlayStation 5", CilovaCastka = 12000, DatumVytvoreni = new DateTime(2026, 3, 1), JeAktivni = true }; // Stále spoříme
                cileKolekce.Insert(new[] { cilDovolena, cilTelefon, cilAuto, cilKonzole });

                for (int rok = 2025; rok <= 2026; rok++)
                {
                    int konecnyMesic = (rok == 2026) ? 4 : 12; // Pro rok 2026 jedeme jen do dubna

                    for (int mesic = 1; mesic <= konecnyMesic; mesic++)
                    {
                        // Pevné příjmy a výdaje
                        transakceKolekce.Insert(new Transakce { Nazev = "Výplata", Castka = 48000, Typ = TypTransakce.Prijem, KategorieId = katVyplata.Id, Datum = new DateTime(rok, mesic, 12) });
                        transakceKolekce.Insert(new Transakce { Nazev = "Nájem a energie", Castka = 16500, Typ = TypTransakce.Vydaj, KategorieId = katBydleni.Id, Datum = new DateTime(rok, mesic, 15) });
                        transakceKolekce.Insert(new Transakce { Nazev = "Internet a telefon", Castka = 950, Typ = TypTransakce.Vydaj, KategorieId = katBydleni.Id, Datum = new DateTime(rok, mesic, 18) });

                        // Náhodné běžné výdaje pro realističtější grafy
                        transakceKolekce.Insert(new Transakce { Nazev = "Supermarket", Castka = rnd.Next(4000, 7000), Typ = TypTransakce.Vydaj, KategorieId = katJidlo.Id, Datum = new DateTime(rok, mesic, 5) });
                        transakceKolekce.Insert(new Transakce { Nazev = "Rohlik.cz", Castka = rnd.Next(1500, 3500), Typ = TypTransakce.Vydaj, KategorieId = katJidlo.Id, Datum = new DateTime(rok, mesic, 20) });
                        transakceKolekce.Insert(new Transakce { Nazev = "Benzín", Castka = rnd.Next(1500, 3000), Typ = TypTransakce.Vydaj, KategorieId = katDoprava.Id, Datum = new DateTime(rok, mesic, 10) });
                        transakceKolekce.Insert(new Transakce { Nazev = "Restaurace/Kino", Castka = rnd.Next(800, 2500), Typ = TypTransakce.Vydaj, KategorieId = katZabava.Id, Datum = new DateTime(rok, mesic, 25) });

                        // Občasné výdaje (Oblečení, lékárna)
                        if (rnd.Next(0, 3) == 1) // Cca každý třetí měsíc
                        {
                            transakceKolekce.Insert(new Transakce { Nazev = "Nové oblečení", Castka = rnd.Next(1000, 4000), Typ = TypTransakce.Vydaj, KategorieId = katObleceni.Id, Datum = new DateTime(rok, mesic, 8) });
                        }
                        if (rnd.Next(0, 4) == 1) // Cca každý čtvrtý měsíc
                        {
                            transakceKolekce.Insert(new Transakce { Nazev = "Lékárna", Castka = rnd.Next(300, 1500), Typ = TypTransakce.Vydaj, KategorieId = katZdravi.Id, Datum = new DateTime(rok, mesic, 22) });
                        }

                        // --- LOGIKA SPOŘENÍ V PRŮBĚHU ČASU ---
                        DateTime datumSporeni = new DateTime(rok, mesic, 16);

                        // Dovolená (leden 2025 - červen 2025)
                        if (rok == 2025 && mesic <= 6)
                        {
                            int idTr = transakceKolekce.Insert(new Transakce { Nazev = "Spoření: Letní dovolená", Castka = 6500, Typ = TypTransakce.Vydaj, KategorieId = katSporeni.Id, Datum = datumSporeni }).AsInt32;
                            vkladyKolekce.Insert(new VkladNaCil { SporiciCilId = cilDovolena.Id, VlozenaCastka = 6500, DatumVkladu = datumSporeni, TransakceId = idTr });
                        }

                        // Telefon (srpen 2025 - listopad 2025)
                        if (rok == 2025 && mesic >= 8 && mesic <= 11)
                        {
                            int idTr = transakceKolekce.Insert(new Transakce { Nazev = "Spoření: Nový telefon", Castka = 6000, Typ = TypTransakce.Vydaj, KategorieId = katSporeni.Id, Datum = datumSporeni }).AsInt32;
                            vkladyKolekce.Insert(new VkladNaCil { SporiciCilId = cilTelefon.Id, VlozenaCastka = 6000, DatumVkladu = datumSporeni, TransakceId = idTr });
                        }

                        // Auto (leden 2026 - duben 2026)
                        if (rok == 2026)
                        {
                            int idTr = transakceKolekce.Insert(new Transakce { Nazev = "Spoření: Ojeté auto", Castka = 10000, Typ = TypTransakce.Vydaj, KategorieId = katSporeni.Id, Datum = datumSporeni }).AsInt32;
                            vkladyKolekce.Insert(new VkladNaCil { SporiciCilId = cilAuto.Id, VlozenaCastka = 10000, DatumVkladu = datumSporeni, TransakceId = idTr });
                        }

                        // Konzole (březen 2026 - duben 2026)
                        if (rok == 2026 && mesic >= 3)
                        {
                            int idTr = transakceKolekce.Insert(new Transakce { Nazev = "Spoření: PlayStation", Castka = 2500, Typ = TypTransakce.Vydaj, KategorieId = katSporeni.Id, Datum = datumSporeni.AddDays(1) }).AsInt32;
                            vkladyKolekce.Insert(new VkladNaCil { SporiciCilId = cilKonzole.Id, VlozenaCastka = 2500, DatumVkladu = datumSporeni.AddDays(1), TransakceId = idTr });
                        }
                    }
                }

                // --- 3. VYTVOŘENÍ ŠABLON ---
                sablonyKolekce.Insert(new SablonaPlatby { Nazev = "Pravidelná výplata", Castka = 48000, Typ = TypTransakce.Prijem, KategorieId = katVyplata.Id });
                sablonyKolekce.Insert(new SablonaPlatby { Nazev = "Nájem a energie", Castka = 16500, Typ = TypTransakce.Vydaj, KategorieId = katBydleni.Id });
                sablonyKolekce.Insert(new SablonaPlatby { Nazev = "Paušál Telefon", Castka = 950, Typ = TypTransakce.Vydaj, KategorieId = katBydleni.Id });
                sablonyKolekce.Insert(new SablonaPlatby { Nazev = "Předplatné Netflix", Castka = 319, Typ = TypTransakce.Vydaj, KategorieId = katZabava.Id });
            }
        }
    }
}