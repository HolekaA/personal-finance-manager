using LiteDB;
using osobniSpravceFinanci.Models;
using System.Collections.Generic;

namespace osobniSpravceFinanci.Services
{
    public class KategorieService
    {
        // ziskani kategorii
        public List<Kategorie> GetAktivniKategorie()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Kategorie>("kategorie");
                return kolekce.Find(k => k.JeAktivni == true).ToList();
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