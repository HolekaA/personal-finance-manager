using LiteDB;
using osobniSpravceFinanci.Models;
using System.Collections.Generic;
using System.Linq;

namespace osobniSpravceFinanci.Services
{
    public class TransakceService
    {
        // ziskani transakci
        public List<Transakce> GetVsechnyTransakce()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                return db.GetCollection<Transakce>("transakce")
                         .FindAll()
                         .OrderByDescending(t => t.Datum)
                         .ToList();
            }
        }

        // pridani transakce
        public int PridatTransakci(Transakce novaTransakce)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Transakce>("transakce");
                kolekce.Insert(novaTransakce);

                return novaTransakce.Id;
            }
        }

        // uprava transakce
        public void UpravitTransakci(Transakce upravenaTransakce)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Transakce>("transakce");
                kolekce.Update(upravenaTransakce);
            }
        }

        // smazani transakce
        public void SmazatTransakci(int idTransakce)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<Transakce>("transakce");
                kolekce.Delete(idTransakce);
            }
        }
    }
}