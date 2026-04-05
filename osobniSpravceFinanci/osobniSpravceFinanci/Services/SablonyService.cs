using LiteDB;
using osobniSpravceFinanci.Models;
using System.Collections.Generic;

namespace osobniSpravceFinanci.Services
{
    public class SablonyService
    {
        // ziskani sablon
        public List<SablonaPlatby> GetSablony()
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<SablonaPlatby>("sablony");
                return kolekce.FindAll().ToList();
            }
        }

        // pridani sablony
        public void PridatSablonu(SablonaPlatby novaSablona)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<SablonaPlatby>("sablony");
                kolekce.Insert(novaSablona);
            }
        }

        // uprava sablony
        public void UpravitSablonu(SablonaPlatby upravenaSablona)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<SablonaPlatby>("sablony");
                kolekce.Update(upravenaSablona);
            }
        }

        // smazani sablony
        public void SmazatSablonu(int idSablony)
        {
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var kolekce = db.GetCollection<SablonaPlatby>("sablony");
                kolekce.Delete(idSablony);
            }
        }
    }
}