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

        // pomocne promenne
        private SporiciCil? _cilKeSmazani;
        private SporiciCil? _cilKUprave;
        private SporiciCil? _cilProVklad;
        private SporiciCil? _cilProVyber;
        private SporiciCil? _cilKeKoupi;

        // konstruktor
        public CilePage(CileService cileService, TransakceService transakceService, KategorieService kategorieService)
        {
            InitializeComponent();
            _cileService = cileService;
            _transakceService = transakceService;
            _kategorieService = kategorieService;
        }

        // po zapnuti stranky
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ObnovitSeznam();
        }

        // nacte cile a vypocita jejich stav
        private void ObnovitSeznam()
        {
            var cileZDatabaze = _cileService.GetVsechnyCile();
            var seznamZobrazeni = new List<CilZobrazeni>();

            foreach (var cil in cileZDatabaze)
            {
                // soucet vkladu
                var nasporeno = _cileService.GetNaspornaCastka(cil.Id);

                // priprava pro zobrazeni
                seznamZobrazeni.Add(new CilZobrazeni
                {
                    CilPuvodni = cil,
                    NaspornaCastka = nasporeno
                });
            }

            // poslani dat do gui
            BindableLayout.SetItemsSource(CileList, seznamZobrazeni);
        }

        // skryti chybove hlasky
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
            // kontrola inputu
            if (string.IsNullOrWhiteSpace(NazevEntry.Text) || string.IsNullOrWhiteSpace(CilovaCastkaEntry.Text))
            {
                ChybaLabel.Text = "Vyplňte název i částku!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // kontrola castky
            if (!decimal.TryParse(CilovaCastkaEntry.Text, out decimal castka) || castka <= 0)
            {
                ChybaLabel.Text = "Částka musí být číslo větší než nula!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // ulozeni cile
            var novyCil = new SporiciCil
            {
                Nazev = NazevEntry.Text,
                CilovaCastka = castka,
                DatumVytvoreni = DateTime.Now
            };

            _cileService.PridatCil(novyCil);

            // reset inputu
            NazevEntry.Text = "";
            CilovaCastkaEntry.Text = "";

            ObnovitSeznam();
        }

        // tlacitko koupeni cile
        private void OnKoupitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilKeKoupi = (SporiciCil)tlacitko.CommandParameter;

            KoupitTextLabel.Text = $"Gratulujeme k naspoření! Opravdu jste '{_cilKeKoupi.Nazev}' zakoupili? Cíl bude skryt z aktivních cílů.";
            KoupitOverlay.IsVisible = true;
        }

        // zruseni koupeni cile
        private void OnZrusitKoupiClicked(object sender, EventArgs e)
        {
            KoupitOverlay.IsVisible = false;
            _cilKeKoupi = null;
        }

        // potvrzeni koupeni cile
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

        // vlozeni vkladu na cil
        private void OnOtevritVkladClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilProVklad = (SporiciCil)tlacitko.CommandParameter;

            VkladCilNazevLabel.Text = _cilProVklad.Nazev;
            VkladCastkaEntry.Text = "";

            VkladOverlay.IsVisible = true;
        }

        // zruseni vkladu na cil
        private void OnZrusitVkladClicked(object sender, EventArgs e)
        {
            VkladOverlay.IsVisible = false;
            _cilProVklad = null;
        }

        // potvrzeni vkladu na cil
        private void OnPotvrditVkladClicked(object sender, EventArgs e)
        {
            if (_cilProVklad == null) return;

            if (!decimal.TryParse(VkladCastkaEntry.Text, out decimal vkladanaCastka) || vkladanaCastka <= 0)
            {
                VkladChybaLabel.Text = "Zadejte platnou částku!";
                VkladChybaLabel.IsVisible = true;
                return;
            }

            // nalezeni kategorie sporeni
            var vybranaKat = _kategorieService.GetKategorieSporeni();

            // vytvoreni transakce pro vklad na cil
            var novaTransakce = new Transakce
            {
                Nazev = $"Spoření: {_cilProVklad.Nazev}",
                Castka = vkladanaCastka,
                Typ = TypTransakce.Vydaj,
                KategorieId = vybranaKat.Id,
                Datum = DateTime.Today
            };

            int idNoveTransakce = _transakceService.PridatTransakci(novaTransakce);

            // vytvoreni zaznamu vkladu na cil
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

        // skryti chybove hlasky u vyberu penez z cile
        private void OnVyberVstupZmenen(object sender, EventArgs e)
        {
            VyberChybaLabel.IsVisible = false;
        }

        // vyber penez z cile
        private void OnOtevritVyberClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilProVyber = (SporiciCil)tlacitko.CommandParameter;

            VyberCilNazevLabel.Text = _cilProVyber.Nazev;
            VyberCastkaEntry.Text = "";

            VyberOverlay.IsVisible = true;
        }

        // zruseni vyberu penez z cile
        private void OnZrusitVyberClicked(object sender, EventArgs e)
        {
            VyberOverlay.IsVisible = false;
            _cilProVyber = null;
        }

        // potvrzeni vyberu pene z cile
        private void OnPotvrditVyberClicked(object sender, EventArgs e)
        {
            if (_cilProVyber == null) return;

            if (!decimal.TryParse(VyberCastkaEntry.Text, out decimal vyberCastka) || vyberCastka <= 0)
            {
                VyberChybaLabel.Text = "Zadejte platnou částku!";
                VyberChybaLabel.IsVisible = true;
                return;
            }

            // kontrola zda v cili je dostatek penez k vyberu
            decimal aktualneNasporeno = _cileService.GetNaspornaCastka(_cilProVyber.Id);
            if (vyberCastka > aktualneNasporeno)
            {
                VyberChybaLabel.Text = $"Můžete odebrat maximálně {aktualneNasporeno} Kč!";
                VyberChybaLabel.IsVisible = true;
                return;
            }

            // nalezeni kategorie sporeni
            var vybranaKat = _kategorieService.GetKategorieSporeni();

            // vytvoreni transakce pro vyber penez
            var novaTransakce = new Transakce
            {
                Nazev = $"Výběr ze spoření: {_cilProVyber.Nazev}",
                Castka = vyberCastka,
                Typ = TypTransakce.Prijem,
                KategorieId = vybranaKat.Id,
                Datum = DateTime.Today
            };

            int idNoveTransakce = _transakceService.PridatTransakci(novaTransakce);

            // ulozeni zaporneho vkladu na cil
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

        // zruseni smazani cile
        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false;
            _cilKeSmazani = null;
        }

        // potvrzeni smaani cile
        private void OnPotvrditSmazaniClicked(object sender, EventArgs e)
        {
            if (_cilKeSmazani != null)
            {
                // nalezeni vkladu/vyberu patrici cili
                var vklady = _cileService.GetVkladyProCil(_cilKeSmazani.Id);
                
                // smazani vsech transakci k tomuto cili
                foreach (var vklad in vklady)
                {
                    if (vklad.TransakceId.HasValue)
                    {
                        _transakceService.SmazatTransakci(vklad.TransakceId.Value);
                    }
                }

                // smazani cile
                _cileService.SmazatCil(_cilKeSmazani.Id);

                ObnovitSeznam();
                SmazatOverlay.IsVisible = false;
                _cilKeSmazani = null;
            }
        }

        // uprava cile 
        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _cilKUprave = (SporiciCil)tlacitko.CommandParameter;

            UpravitNazevEntry.Text = _cilKUprave.Nazev;
            UpravitCastkaEntry.Text = _cilKUprave.CilovaCastka.ToString();

            UpravitOverlay.IsVisible = true;
        }

        // zruseni upravy cile
        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _cilKUprave = null;
        }

        // potvrzeni upravy cile
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

    // trida pro zobrazeni cilu a progress baru
    public class CilZobrazeni
    {
        public SporiciCil CilPuvodni { get; set; } = null!;
        public string Nazev => CilPuvodni.Nazev;
        public decimal CilovaCastka => CilPuvodni.CilovaCastka;
        public decimal NaspornaCastka { get; set; }

        // vypocet na kolik procent je splnen cil
        public double Procenta
        {
            get
            {
                if (CilovaCastka == 0) return 0;
                double procento = (double)(NaspornaCastka / CilovaCastka);
                // kontrola proti preplneni cile
                return procento > 1.0 ? 1.0 : procento;
            }
        }

        // zmena barvy progress baru
        public string BarvaPrubehu
        {
            get
            {
                if (Procenta >= 1.0) return "#28A745"; // 100%
                if (Procenta >= 0.5) return "#FFC107"; // 50-99%
                return "#007AFF"; // 0-49%
            }
        }
    }
}