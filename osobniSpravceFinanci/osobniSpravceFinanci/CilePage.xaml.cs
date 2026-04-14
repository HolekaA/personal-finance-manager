using osobniSpravceFinanci.Models;
using osobniSpravceFinanci.Services;
using System.Collections.Generic;
using System.Linq;

namespace osobniSpravceFinanci
{
    public partial class CilePage : ContentPage
    {
        private readonly CileService _cileService;
        private readonly TransakceService _transakceService;
        private readonly KategorieService _kategorieService;

        private List<Kategorie> _dostupneKategorie = new List<Kategorie>();

        private SporiciCil? _cilKeSmazani;
        private SporiciCil? _cilKUprave;
        private SporiciCil? _cilProVklad;
        private SporiciCil? _cilProVyber;
        private SporiciCil? _cilKeKoupi;

        public CilePage(CileService cileService, TransakceService transakceService, KategorieService kategorieService)
        {
            InitializeComponent();
            _cileService = cileService;
            _transakceService = transakceService;
            _kategorieService = kategorieService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _dostupneKategorie = _kategorieService.GetAktivniKategorie();

            ObnovitSeznam();
        }

        private void ObnovitSeznam()
        {
            var cileZDatabaze = _cileService.GetVsechnyCile();
            var seznamZobrazeni = new List<CilZobrazeni>();

            foreach (var cil in cileZDatabaze)
            {
                var nasporeno = _cileService.GetNaspornaCastka(cil.Id);

                seznamZobrazeni.Add(new CilZobrazeni
                {
                    CilPuvodni = cil,
                    NaspornaCastka = nasporeno
                });
            }

            BindableLayout.SetItemsSource(CileList, seznamZobrazeni);
        }

        private void OnVstupZmenen(object sender, EventArgs e)
        {
            ChybaLabel.IsVisible = false;
        }

        private void OnVkladVstupZmenen(object sender, EventArgs e)
        {
            VkladChybaLabel.IsVisible = false;
        }

        // pridani cile
        private void OnPridatClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NazevEntry.Text) || string.IsNullOrWhiteSpace(CilovaCastkaEntry.Text))
            {
                ChybaLabel.Text = "Vyplňte název i částku!";
                ChybaLabel.IsVisible = true;
                return;
            }

            if (!decimal.TryParse(CilovaCastkaEntry.Text, out decimal castka) || castka <= 0)
            {
                ChybaLabel.Text = "Částka musí být číslo větší než nula!";
                ChybaLabel.IsVisible = true;
                return;
            }

            var novyCil = new SporiciCil
            {
                Nazev = NazevEntry.Text,
                CilovaCastka = castka,
                DatumVytvoreni = DateTime.Now
            };

            _cileService.PridatCil(novyCil);

            NazevEntry.Text = "";
            CilovaCastkaEntry.Text = "";

            ObnovitSeznam();
        }

        // koupeni cile
        private void OnKoupitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilKeKoupi = (SporiciCil)tlacitko.CommandParameter;

            KoupitTextLabel.Text = $"Gratulujeme k naspoření! Opravdu jste '{_cilKeKoupi.Nazev}' zakoupili? Cíl bude skryt z aktivních cílů.";
            KoupitOverlay.IsVisible = true;
        }

        private void OnZrusitKoupiClicked(object sender, EventArgs e)
        {
            KoupitOverlay.IsVisible = false;
            _cilKeKoupi = null;
        }

        private void OnPotvrditKoupiClicked(object sender, EventArgs e)
        {
            if (_cilKeKoupi != null)
            {
                _cilKeKoupi.JeAktivni = false;

                _cileService.UpravitCil(_cilKeKoupi);

                ObnovitSeznam();
                KoupitOverlay.IsVisible = false;
                _cilKeKoupi = null;
            }
        }

        // vlozeni vkladu
        private void OnOtevritVkladClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilProVklad = (SporiciCil)tlacitko.CommandParameter;

            VkladCilNazevLabel.Text = _cilProVklad.Nazev;
            VkladCastkaEntry.Text = "";

            VkladOverlay.IsVisible = true;
        }

        private void OnZrusitVkladClicked(object sender, EventArgs e)
        {
            VkladOverlay.IsVisible = false;
            _cilProVklad = null;
        }

        private void OnPotvrditVkladClicked(object sender, EventArgs e)
        {
            if (_cilProVklad == null) return;

            if (!decimal.TryParse(VkladCastkaEntry.Text, out decimal vkladanaCastka) || vkladanaCastka <= 0)
            {
                VkladChybaLabel.Text = "Zadejte platnou částku!";
                VkladChybaLabel.IsVisible = true;
                return;
            }

            var vybranaKat = _kategorieService.GetKategorieSporeni();

            var novaTransakce = new Transakce
            {
                Nazev = $"Spoření: {_cilProVklad.Nazev}",
                Castka = vkladanaCastka,
                Typ = TypTransakce.Vydaj,
                KategorieId = vybranaKat.Id,
                Datum = DateTime.Today
            };

            int idNoveTransakce = _transakceService.PridatTransakci(novaTransakce);

            var novyVklad = new VkladNaCil
            {
                SporiciCilId = _cilProVklad.Id,
                VlozenaCastka = vkladanaCastka,
                DatumVkladu = DateTime.Today,
                TransakceId = idNoveTransakce
            };

            _cileService.PridatVklad(novyVklad);

            ObnovitSeznam();
            VkladOverlay.IsVisible = false;
            _cilProVklad = null;
        }

        // odebrani vkladu
        private void OnVyberVstupZmenen(object sender, EventArgs e)
        {
            VyberChybaLabel.IsVisible = false;
        }

        private void OnOtevritVyberClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilProVyber = (SporiciCil)tlacitko.CommandParameter;

            VyberCilNazevLabel.Text = _cilProVyber.Nazev;
            VyberCastkaEntry.Text = "";

            VyberOverlay.IsVisible = true;
        }

        private void OnZrusitVyberClicked(object sender, EventArgs e)
        {
            VyberOverlay.IsVisible = false;
            _cilProVyber = null;
        }

        private void OnPotvrditVyberClicked(object sender, EventArgs e)
        {
            if (_cilProVyber == null) return;

            if (!decimal.TryParse(VyberCastkaEntry.Text, out decimal vyberCastka) || vyberCastka <= 0)
            {
                VyberChybaLabel.Text = "Zadejte platnou částku!";
                VyberChybaLabel.IsVisible = true;
                return;
            }

            decimal aktualneNasporeno = _cileService.GetNaspornaCastka(_cilProVyber.Id);
            if (vyberCastka > aktualneNasporeno)
            {
                VyberChybaLabel.Text = $"Můžete odebrat maximálně {aktualneNasporeno} Kč!";
                VyberChybaLabel.IsVisible = true;
                return;
            }

            var vybranaKat = _kategorieService.GetKategorieSporeni();

            var novaTransakce = new Transakce
            {
                Nazev = $"Výběr ze spoření: {_cilProVyber.Nazev}",
                Castka = vyberCastka,
                Typ = TypTransakce.Prijem,
                KategorieId = vybranaKat.Id,
                Datum = DateTime.Today
            };

            int idNoveTransakce = _transakceService.PridatTransakci(novaTransakce);

            var novyVklad = new VkladNaCil
            {
                SporiciCilId = _cilProVyber.Id,
                VlozenaCastka = -vyberCastka,
                DatumVkladu = DateTime.Today,
                TransakceId = idNoveTransakce
            };

            _cileService.PridatVklad(novyVklad);

            ObnovitSeznam();
            VyberOverlay.IsVisible = false;
            _cilProVyber = null;
        }

        // smazani cile
        private void OnSmazatClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilKeSmazani = (SporiciCil)tlacitko.CommandParameter;

            SmazatTextLabel.Text = $"Opravdu chcete smazat cíl '{_cilKeSmazani.Nazev}'?";
            SmazatOverlay.IsVisible = true;
        }

        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false;
            _cilKeSmazani = null;
        }

        private void OnPotvrditSmazaniClicked(object sender, EventArgs e)
        {
            if (_cilKeSmazani != null)
            {
                var vklady = _cileService.GetVkladyProCil(_cilKeSmazani.Id);
                foreach (var vklad in vklady)
                {
                    if (vklad.TransakceId.HasValue)
                    {
                        _transakceService.SmazatTransakci(vklad.TransakceId.Value);
                    }
                }

                _cileService.SmazatCil(_cilKeSmazani.Id);

                ObnovitSeznam();
                SmazatOverlay.IsVisible = false;
                _cilKeSmazani = null;
            }
        }

        // uprava
        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilKUprave = (SporiciCil)tlacitko.CommandParameter;

            UpravitNazevEntry.Text = _cilKUprave.Nazev;
            UpravitCastkaEntry.Text = _cilKUprave.CilovaCastka.ToString();

            UpravitOverlay.IsVisible = true;
        }

        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _cilKUprave = null;
        }

        private void OnUlozitUpravuClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UpravitNazevEntry.Text) || string.IsNullOrWhiteSpace(UpravitCastkaEntry.Text))
                return;

            if (decimal.TryParse(UpravitCastkaEntry.Text, out decimal castka) && castka > 0)
            {
                if (_cilKUprave != null)
                {
                    _cilKUprave.Nazev = UpravitNazevEntry.Text;
                    _cilKUprave.CilovaCastka = castka;

                    _cileService.UpravitCil(_cilKUprave);
                    ObnovitSeznam();

                    UpravitOverlay.IsVisible = false;
                    _cilKUprave = null;
                }
            }
        }
    }

    public class CilZobrazeni
    {
        public SporiciCil CilPuvodni { get; set; } = null!;
        public string Nazev => CilPuvodni.Nazev;
        public decimal CilovaCastka => CilPuvodni.CilovaCastka;
        public decimal NaspornaCastka { get; set; }

        public double Procenta
        {
            get
            {
                if (CilovaCastka == 0) return 0;
                double procento = (double)(NaspornaCastka / CilovaCastka);
                return procento > 1.0 ? 1.0 : procento;
            }
        }

        public string BarvaPrubehu
        {
            get
            {
                if (Procenta >= 1.0) return "#28A745";
                if (Procenta >= 0.5) return "#FFC107";
                return "#007AFF";
            }
        }
    }
}