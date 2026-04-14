using LiteDB;
using osobniSpravceFinanci.Models;
using System.Collections.Generic;
using System.Linq;

namespace osobniSpravceFinanci.Services
{
    public class CileService
    {
        // ziskani cilu
        public List<SporiciCil> GetVsechnyCile()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                return db.GetCollection<SporiciCil>("sporiciCile").Find(c => c.JeAktivni == true).ToList();
            }
        }

        // pridani cile
        public void PridatCil(SporiciCil novyCil)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                db.GetCollection<SporiciCil>("sporiciCile").Insert(novyCil);
            }
        }

        // uprava cile
        public void UpravitCil(SporiciCil upravenyCil)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                db.GetCollection<SporiciCil>("sporiciCile").Update(upravenyCil);
            }
        }

        // smazani cile
        public void SmazatCil(int idCile)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                db.GetCollection<SporiciCil>("sporiciCile").Delete(idCile);
                db.GetCollection<VkladNaCil>("vkladyNaCile").DeleteMany(v => v.SporiciCilId == idCile);
            }
        }


        // ziskanu vkladu na cile
        public List<VkladNaCil> GetVkladyProCil(int idCile)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                return db.GetCollection<VkladNaCil>("vkladyNaCile")
                         .Find(v => v.SporiciCilId == idCile)
                         .OrderByDescending(v => v.DatumVkladu)
                         .ToList();
            }
        }

        // pridani vkladu na cil
        public void PridatVklad(VkladNaCil novyVklad)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                db.GetCollection<VkladNaCil>("vkladyNaCile").Insert(novyVklad);
            }
        }

        // smazani vkladu na cil
        public void SmazatVklad(int idVkladu)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                db.GetCollection<VkladNaCil>("vkladyNaCile").Delete(idVkladu);
            }
        }

        // Přidat do CileService.cs
        public void UpravitVkladPodleTransakce(int transakceId, decimal novaCastka)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<VkladNaCil>("vkladyNaCile");
                var vklad = kolekce.FindOne(v => v.TransakceId == transakceId);

                if (vklad != null)
                {
                    vklad.VlozenaCastka = novaCastka;
                    kolekce.Update(vklad);
                }
            }
        }

        public void SmazatVkladPodleTransakce(int transakceId)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<VkladNaCil>("vkladyNaCile");
                var vklad = kolekce.FindOne(v => v.TransakceId == transakceId);

                if (vklad != null)
                {
                    kolekce.Delete(vklad.Id);
                }
            }
        }

        // soucet castek pro progress 
        public decimal GetNaspornaCastka(int idCile)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var vklady = db.GetCollection<VkladNaCil>("vkladyNaCile").Find(v => v.SporiciCilId == idCile);
                return vklady.Sum(v => v.VlozenaCastka);
            }
        }
    }
}