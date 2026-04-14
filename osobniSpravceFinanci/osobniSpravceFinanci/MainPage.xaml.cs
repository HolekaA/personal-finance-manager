using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using osobniSpravceFinanci.Models;
using osobniSpravceFinanci.Services;
using LiteDB;

namespace osobniSpravceFinanci
{
    public partial class MainPage : ContentPage
    {
        private readonly TransakceService _transakceService;
        private readonly KategorieService _kategorieService;
        private readonly CileService _cileService;
        private readonly SablonyService _sablonyService;

        private List<Kategorie> _dostupneKategorie = new List<Kategorie>();
        private DateTime _vybranyMesic = DateTime.Today;

        private Transakce? _transakceKeSmazani;
        private Transakce? _transakceKUprave;

        public MainPage(TransakceService transakceService, KategorieService kategorieService, CileService cileService, SablonyService sablonyService)
        {
            InitializeComponent();
            _transakceService = transakceService;
            _kategorieService = kategorieService;
            _cileService = cileService;
            _sablonyService = sablonyService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            NacistKategorie();

            DatumPicker.Date = DateTime.Today;

            ZkontrolovatAGenerovatSablony();

            ObnovitSeznam();
        }

        private void NacistKategorie()
        {
            _dostupneKategorie = _kategorieService.GetAktivniKategorie();

            KategoriePicker.ItemsSource = _dostupneKategorie;
            UpravitKategoriePicker.ItemsSource = _dostupneKategorie;

            TypPicker.SelectedIndex = 0;
            if (_dostupneKategorie.Count > 0)
            {
                KategoriePicker.SelectedIndex = 0;
            }
        }

        private void ObnovitSeznam()
        {
            var vsechnyTransakceZDatabaze = _transakceService.GetVsechnyTransakce();

            decimal celkovyZustatek = vsechnyTransakceZDatabaze.Sum(t => t.Typ == TypTransakce.Prijem ? t.Castka : -t.Castka);
            ZustatekLabel.Text = $"{celkovyZustatek:N0} Kč";

            var transakceProTentoMesic = vsechnyTransakceZDatabaze
                .Where(t => t.Datum.Year == _vybranyMesic.Year && t.Datum.Month == _vybranyMesic.Month)
                .ToList();

            var nazevMesice = _vybranyMesic.ToString("MMMM yyyy", new CultureInfo("cs-CZ"));
            MesicLabel.Text = char.ToUpper(nazevMesice[0]) + nazevMesice.Substring(1);

            var seznamZobrazeni = new List<TransakceZobrazeni>();

            foreach (var transakce in transakceProTentoMesic)
            {
                var kategorie = _dostupneKategorie.FirstOrDefault(k => k.Id == transakce.KategorieId);

                seznamZobrazeni.Add(new TransakceZobrazeni
                {
                    TransakcePuvodni = transakce,
                    KategorieNazev = kategorie != null ? kategorie.Nazev : "Neznámá",
                    KategorieBarva = kategorie != null ? kategorie.Barva : "#808080"
                });
            }

            TransakceList.ItemsSource = seznamZobrazeni;
        }

        private void OnVstupZmenen(object sender, EventArgs e)
        {
            ChybaLabel.IsVisible = false;
        }

        // pridani transakce
        private void OnPridatClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NazevEntry.Text) || string.IsNullOrWhiteSpace(CastkaEntry.Text))
            {
                ChybaLabel.Text = "Vyplňte prosím název i částku!";
                ChybaLabel.IsVisible = true;
                return;
            }

            if (!decimal.TryParse(CastkaEntry.Text, out decimal castka) || castka <= 0)
            {
                ChybaLabel.Text = "Částka musí být kladné číslo!";
                ChybaLabel.IsVisible = true;
                return;
            }

            if (KategoriePicker.SelectedItem == null)
            {
                ChybaLabel.Text = "Musíte vybrat kategorii!";
                ChybaLabel.IsVisible = true;
                return;
            }

            var zvolenaKategorie = (Kategorie)KategoriePicker.SelectedItem;
            var zvolenyTyp = TypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;

            var novaTransakce = new Transakce
            {
                Nazev = NazevEntry.Text,
                Castka = castka,
                Typ = zvolenyTyp,
                KategorieId = zvolenaKategorie.Id,
                Datum = DatumPicker.Date ?? DateTime.Today,
                SablonaId = null
            };

            _transakceService.PridatTransakci(novaTransakce);

            NazevEntry.Text = "";
            CastkaEntry.Text = "";
            TypPicker.SelectedIndex = 0;
            if (_dostupneKategorie.Count > 0) KategoriePicker.SelectedIndex = 0;
            DatumPicker.Date = DateTime.Today;

            ObnovitSeznam();
        }

        // smazani
        private void OnSmazatClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _transakceKeSmazani = (Transakce)tlacitko.CommandParameter;

            SmazatTextLabel.Text = $"Opravdu chcete smazat transakci '{_transakceKeSmazani.Nazev}'?";
            SmazatOverlay.IsVisible = true;
        }

        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false;
            _transakceKeSmazani = null;
        }

        private void OnPotvrditSmazaniClicked(object sender, EventArgs e)
        {
            if (_transakceKeSmazani != null)
            {
                _cileService.SmazatVkladPodleTransakce(_transakceKeSmazani.Id);
                _transakceService.SmazatTransakci(_transakceKeSmazani.Id);
                ObnovitSeznam();
                SmazatOverlay.IsVisible = false;
                _transakceKeSmazani = null;
            }
        }

        // uprava
        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _transakceKUprave = (Transakce)tlacitko.CommandParameter;

            UpravitNazevEntry.Text = _transakceKUprave.Nazev;
            UpravitCastkaEntry.Text = _transakceKUprave.Castka.ToString();
            UpravitTypPicker.SelectedIndex = _transakceKUprave.Typ == TypTransakce.Vydaj ? 0 : 1;
            UpravitDatumPicker.Date = _transakceKUprave.Datum;

            UpravitKategoriePicker.SelectedItem = _dostupneKategorie.FirstOrDefault(k => k.Id == _transakceKUprave.KategorieId);

            UpravitOverlay.IsVisible = true;
        }

        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _transakceKUprave = null;
        }

        private void OnUlozitUpravuClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UpravitNazevEntry.Text) || string.IsNullOrWhiteSpace(UpravitCastkaEntry.Text))
                return;

            if (!decimal.TryParse(UpravitCastkaEntry.Text, out decimal castka) || castka <= 0)
                return;

            if (_transakceKUprave != null)
            {
                _transakceKUprave.Nazev = UpravitNazevEntry.Text;
                _transakceKUprave.Castka = castka;
                _transakceKUprave.Typ = UpravitTypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;
                _transakceKUprave.Datum = UpravitDatumPicker.Date ?? DateTime.Today;

                if (UpravitKategoriePicker.SelectedItem is Kategorie vybranaKat)
                {
                    _transakceKUprave.KategorieId = vybranaKat.Id;
                }

                _cileService.UpravitVkladPodleTransakce(_transakceKUprave.Id, castka);
                _transakceService.UpravitTransakci(_transakceKUprave);
                ObnovitSeznam();

                UpravitOverlay.IsVisible = false;
                _transakceKUprave = null;
            }
        }

        private void OnPredchoziMesicClicked(object sender, EventArgs e)
        {
            _vybranyMesic = _vybranyMesic.AddMonths(-1);
            ObnovitSeznam();
        }

        private void OnDalsiMesicClicked(object sender, EventArgs e)
        {
            _vybranyMesic = _vybranyMesic.AddMonths(1);
            ObnovitSeznam();
        }

        private void ZkontrolovatAGenerovatSablony()
        {
            var aktualniMesic = DateTime.Today.Month;
            var aktualniRok = DateTime.Today.Year;
            bool tentoMesicUzJeHotovy = false;

            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var historieKolekce = db.GetCollection<VygenerovanyMesic>("vygenerovaneMesice");
                // ------------PRO TESTOVANI----------------
                //historieKolekce.DeleteAll();
                //------------------------------------------
                tentoMesicUzJeHotovy = historieKolekce.Exists(x => x.Rok == aktualniRok && x.Mesic == aktualniMesic);
            } 

            if (!tentoMesicUzJeHotovy)
            {
                var vsechnySablony = _sablonyService.GetSablony();

                if (vsechnySablony.Count > 0)
                {
                    foreach (var sablona in vsechnySablony)
                    {
                        var novaTransakce = new Transakce
                        {
                            Nazev = sablona.Nazev,
                            Castka = sablona.Castka,
                            Typ = sablona.Typ,
                            KategorieId = sablona.KategorieId,
                            Datum = DateTime.Today,
                            SablonaId = sablona.Id
                        };

                        _transakceService.PridatTransakci(novaTransakce);
                    }
                }

                using (var db = new LiteDatabase(DatabaseContext.DbPath))
                {
                    var historieKolekce = db.GetCollection<VygenerovanyMesic>("vygenerovaneMesice");
                    historieKolekce.Insert(new VygenerovanyMesic { Rok = aktualniRok, Mesic = aktualniMesic });
                }
            }
        }
    }

    public class TransakceZobrazeni
    {
        public Transakce TransakcePuvodni { get; set; } = null!;

        public string Nazev => TransakcePuvodni.Nazev;
        public string KategorieNazev { get; set; } = "";
        public string KategorieBarva { get; set; } = "";

        public string DatumZobrazeni => TransakcePuvodni.Datum.ToString("dd.MM.yyyy");

        public string CastkaZobrazeni => TransakcePuvodni.Typ == TypTransakce.Prijem
            ? $"+ {TransakcePuvodni.Castka} Kč"
            : $"- {TransakcePuvodni.Castka} Kč";

        public string BarvaTypu => TransakcePuvodni.Typ == TypTransakce.Prijem ? "#28A745" : "#DC3545";
    }
}