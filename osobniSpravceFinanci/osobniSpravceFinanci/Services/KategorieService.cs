using LiteDB;
using osobniSpravceFinanci.Models;
using System.Collections.Generic;

namespace osobniSpravceFinanci.Services
{
    public class KategorieService
    {
        public KategorieService()
        {
            ZajistitSystemoveKategorie();
        }

        private void ZajistitSystemoveKategorie()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");

                if (!kolekce.Exists(k => k.Nazev == "Neznámá"))
                {
                    kolekce.Insert(new Kategorie { Nazev = "Neznámá", Barva = "#808080", JeAktivni = false });
                }

                if (!kolekce.Exists(k => k.Nazev == "Spoření"))
                {
                    kolekce.Insert(new Kategorie { Nazev = "Spoření", Barva = "#007AFF", JeAktivni = false });
                }
            }
        }

        // ziskani nezname kategorie a sporeni
        public Kategorie GetKategorieNeznama()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                return db.GetCollection<Kategorie>("kategorie").FindOne(k => k.Nazev == "Neznámá");
            }
        }

        public Kategorie GetKategorieSporeni()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                return db.GetCollection<Kategorie>("kategorie").FindOne(k => k.Nazev == "Spoření");
            }
        }

        // ziskani kategorii
        public List<Kategorie> GetAktivniKategorie()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");
                return kolekce.Find(k => k.JeAktivni == true).ToList();
            }
        }

        public List<Kategorie> GetVsechnyKategorie()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");
                return kolekce.FindAll().ToList();
            }
        }

        // pridani kategorie
        public void PridatKategorii(Kategorie novaKategorie)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");
                kolekce.Insert(novaKategorie);
            }
        }

        // uprava kategorie
        public void UpravitKategorii(Kategorie upravenaKategorie)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");
                kolekce.Update(upravenaKategorie);
            }
        }

        // smazani kategorie - soft delete
        public void SmazatKategorii(int idKategorie)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");
                var kategorieKsmazani = kolekce.FindById(idKategorie);

                if (kategorieKsmazani != null)
                {
                    kategorieKsmazani.JeAktivni = false;
                    kolekce.Update(kategorieKsmazani);
                }
            }
        }
    }
}