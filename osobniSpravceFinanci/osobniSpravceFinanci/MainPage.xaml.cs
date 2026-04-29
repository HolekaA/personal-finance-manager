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
        private readonly SablonyService _sablonyService;

        // seznamy pro kategorie
        private List<Kategorie> _dostupneKategorie = new List<Kategorie>();
        private List<Kategorie> _vsechnyKategorie = new List<Kategorie>();
        
        // aktualne vybrany mesic v prehledu
        private DateTime _vybranyMesic = DateTime.Today;

        // pomocne promenne
        private Transakce? _transakceKeSmazani;
        private Transakce? _transakceKUprave;

        // konstruktor
        public MainPage(TransakceService transakceService, KategorieService kategorieService, SablonyService sablonyService)
        {
            InitializeComponent();
            _transakceService = transakceService;
            _kategorieService = kategorieService;
            _sablonyService = sablonyService;
        }

        // po zapnuti stranky
        protected override void OnAppearing()
        {
            base.OnAppearing();

            NacistKategorie();

            // nastaveni data na dnesek
            DatumPicker.Date = DateTime.Today;

            // kontrola generovani sablon v aktualnim mesici
            ZkontrolovatAGenerovatSablony();

            ObnovitSeznam();
        }

        // nacteni kategorii do pickeru
        private void NacistKategorie()
        {
            _dostupneKategorie = _kategorieService.GetAktivniKategorie();
            _vsechnyKategorie = _kategorieService.GetVsechnyKategorie();

            KategoriePicker.ItemsSource = _dostupneKategorie;
            UpravitKategoriePicker.ItemsSource = _dostupneKategorie;

            TypPicker.SelectedIndex = 0;
        }

        // vypocet zustatku a zobrazeni transakci
        private void ObnovitSeznam()
        {
            var vsechnyTransakceZDatabaze = _transakceService.GetVsechnyTransakce();

            // vypocet zustatku
            decimal celkovyZustatek = vsechnyTransakceZDatabaze.Sum(t => t.Typ == TypTransakce.Prijem ? t.Castka : -t.Castka);
            ZustatekLabel.Text = $"{celkovyZustatek:N0} Kč";

            // filtrace transakci pro aktualni mesic a rok
            var transakceProTentoMesic = vsechnyTransakceZDatabaze
                .Where(t => t.Datum.Year == _vybranyMesic.Year && t.Datum.Month == _vybranyMesic.Month)
                .ToList();

            // preklad a formatovani aktualniho mesice a roku
            var nazevMesice = _vybranyMesic.ToString("MMMM yyyy", new CultureInfo("cs-CZ"));
            MesicLabel.Text = char.ToUpper(nazevMesice[0]) + nazevMesice.Substring(1);

            // priprava dat pro zobrazeni
            var seznamZobrazeni = new List<TransakceZobrazeni>();

            foreach (var transakce in transakceProTentoMesic)
            {
                // prirazeni kategorie k transakci
                var kategorie = _vsechnyKategorie.FirstOrDefault(k => k.Id == transakce.KategorieId);

                seznamZobrazeni.Add(new TransakceZobrazeni
                {
                    TransakcePuvodni = transakce,
                    KategorieNazev = kategorie != null ? kategorie.Nazev : "Neznámá",
                    KategorieBarva = kategorie != null ? kategorie.Barva : "#808080"
                });
            }

            // poslani dat do gui
            BindableLayout.SetItemsSource(TransakceList, seznamZobrazeni);
        }

        // skryti chybove hlasky
        private void OnVstupZmenen(object sender, EventArgs e)
        {
            ChybaLabel.IsVisible = false;
        }

        // pridani transakce
        private void OnPridatClicked(object sender, EventArgs e)
        {
            // kontrola inputu
            if (string.IsNullOrWhiteSpace(NazevEntry.Text) || string.IsNullOrWhiteSpace(CastkaEntry.Text))
            {
                ChybaLabel.Text = "Vyplňte prosím název i částku!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // kontrola castky
            if (!decimal.TryParse(CastkaEntry.Text, out decimal castka) || castka <= 0)
            {
                ChybaLabel.Text = "Částka musí být kladné číslo!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // prirazeni kategorie
            Kategorie zvolenaKategorie;

            if (KategoriePicker.SelectedItem != null)
            {
                zvolenaKategorie = (Kategorie)KategoriePicker.SelectedItem;
            }
            else
            {
                zvolenaKategorie = _kategorieService.GetKategorieNeznama();
            }

            var zvolenyTyp = TypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;

            // vytvoreni transakce a ulozeni do databaze
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

            // reset inputu
            NazevEntry.Text = "";
            CastkaEntry.Text = "";
            TypPicker.SelectedIndex = 0;
            DatumPicker.Date = DateTime.Today;

            ObnovitSeznam();
        }

        // tlacitko smazani transakce
        private void OnSmazatClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _transakceKeSmazani = (Transakce)tlacitko.CommandParameter;

            SmazatTextLabel.Text = $"Opravdu chcete smazat transakci '{_transakceKeSmazani.Nazev}'?";
            SmazatOverlay.IsVisible = true;
        }

        // zruseni smazani transakce
        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false;
            _transakceKeSmazani = null;
        }

        // potvrzeni smazani transakce
        private void OnPotvrditSmazaniClicked(object sender, EventArgs e)
        {
            if (_transakceKeSmazani != null)
            {
                _transakceService.SmazatTransakci(_transakceKeSmazani.Id);
                ObnovitSeznam();
                SmazatOverlay.IsVisible = false;
                _transakceKeSmazani = null;
            }
        }

        // skryti chybove hlasky v uprave transakce
        private void OnUpravitVstupZmenen(object sender, EventArgs e)
        {
            UpravitChybaLabel.IsVisible = false;
        }

        // uprava transakce
        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _transakceKUprave = (Transakce)tlacitko.CommandParameter;

            // predvyplneni inputu daty
            UpravitNazevEntry.Text = _transakceKUprave.Nazev;
            UpravitCastkaEntry.Text = _transakceKUprave.Castka.ToString();
            UpravitTypPicker.SelectedIndex = _transakceKUprave.Typ == TypTransakce.Vydaj ? 0 : 1;
            UpravitDatumPicker.Date = _transakceKUprave.Datum;

            UpravitKategoriePicker.SelectedItem = _dostupneKategorie.FirstOrDefault(k => k.Id == _transakceKUprave.KategorieId);

            UpravitChybaLabel.IsVisible = false;
            UpravitOverlay.IsVisible = true;
        }

        // zruseni upravy transakce
        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _transakceKUprave = null;
        }

        // potvrzeni upravy transakce
        private void OnUlozitUpravuClicked(object sender, EventArgs e)
        {
            // kontrola inputu
            if (string.IsNullOrWhiteSpace(UpravitNazevEntry.Text) || string.IsNullOrWhiteSpace(UpravitCastkaEntry.Text))
            {
                UpravitChybaLabel.Text = "Vyplňte prosím název i částku!";
                UpravitChybaLabel.IsVisible = true;
                return;
            }

            // kontrola castky
            if (!decimal.TryParse(UpravitCastkaEntry.Text, out decimal castka) || castka <= 0)
            {
                UpravitChybaLabel.Text = "Částka musí být kladné číslo!";
                UpravitChybaLabel.IsVisible = true;
                return;
            }

            if (_transakceKUprave != null)
            {
                // aktualizace dat
                _transakceKUprave.Nazev = UpravitNazevEntry.Text;
                _transakceKUprave.Castka = castka;
                _transakceKUprave.Typ = UpravitTypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;
                _transakceKUprave.Datum = UpravitDatumPicker.Date ?? DateTime.Today;

                if (UpravitKategoriePicker.SelectedItem is Kategorie vybranaKat)
                {
                    _transakceKUprave.KategorieId = vybranaKat.Id;
                }

                // ulozeni do databaze
                _transakceService.UpravitTransakci(_transakceKUprave);
                ObnovitSeznam();

                UpravitOverlay.IsVisible = false;
                _transakceKUprave = null;
            }
        }

        // posunuti mesice zpet
        private void OnPredchoziMesicClicked(object sender, EventArgs e)
        {
            _vybranyMesic = _vybranyMesic.AddMonths(-1);
            ObnovitSeznam();
        }

        // posunuti mesice dopredu
        private void OnDalsiMesicClicked(object sender, EventArgs e)
        {
            _vybranyMesic = _vybranyMesic.AddMonths(1);
            ObnovitSeznam();
        }

        // automaticke propisovani sablon
        private void ZkontrolovatAGenerovatSablony()
        {
            var aktualniMesic = DateTime.Today.Month;
            var aktualniRok = DateTime.Today.Year;
            bool tentoMesicUzJeHotovy = false;

            // kontrola v databazi zda byl mesic vygenerovan
            using (var db = new LiteDatabase(DatabaseContext.DbPath))
            {
                var historieKolekce = db.GetCollection<VygenerovanyMesic>("vygenerovaneMesice");
                // ------------PRO TESTOVANI----------------
                //historieKolekce.DeleteAll();
                //------------------------------------------
                tentoMesicUzJeHotovy = historieKolekce.Exists(x => x.Rok == aktualniRok && x.Mesic == aktualniMesic);
            } 

            // pokud nebyl, vytvori se
            if (!tentoMesicUzJeHotovy)
            {
                var vsechnySablony = _sablonyService.GetSablony();

                if (vsechnySablony.Count > 0)
                {
                    // pro kazdou sablonu vytvorei transakce
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

                // zapsani do databaze vygenerovany mesic
                using (var db = new LiteDatabase(DatabaseContext.DbPath))
                {
                    var historieKolekce = db.GetCollection<VygenerovanyMesic>("vygenerovaneMesice");
                    historieKolekce.Insert(new VygenerovanyMesic { Rok = aktualniRok, Mesic = aktualniMesic });
                }
            }
        }
    }

    // trida pro zobrazeni transakci
    public class TransakceZobrazeni
    {
        public Transakce TransakcePuvodni { get; set; } = null!;

        public string Nazev => TransakcePuvodni.Nazev;
        public string KategorieNazev { get; set; } = "";
        public string KategorieBarva { get; set; } = "";

        // schovani smaani a upravy kdyz se jedna a sporeni
        public bool LzeUpravovat => KategorieNazev != "Spoření";

        public string DatumZobrazeni => TransakcePuvodni.Datum.ToString("dd.MM.yyyy");

        // formatovani castky v gui
        public string CastkaZobrazeni => TransakcePuvodni.Typ == TypTransakce.Prijem
            ? $"+ {TransakcePuvodni.Castka} Kč"
            : $"- {TransakcePuvodni.Castka} Kč";

        public string BarvaTypu => TransakcePuvodni.Typ == TypTransakce.Prijem ? "#28A745" : "#DC3545";
    }
}