using osobniSpravceFinanci.Models;
using osobniSpravceFinanci.Services;
using System.Collections.Generic;
using System.Linq;

namespace osobniSpravceFinanci
{
    public partial class SablonyPage : ContentPage
    {
        private readonly SablonyService _sablonyService;
        private readonly KategorieService _kategorieService;

        private SablonaPlatby? _sablonaKeSmazani;
        private SablonaPlatby? _sablonaKUprave;

        private List<Kategorie> _dostupneKategorie = new List<Kategorie>();

        public SablonyPage(SablonyService sablonyService, KategorieService kategorieService)
        {
            InitializeComponent();
            _sablonyService = sablonyService;
            _kategorieService = kategorieService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            NacistKategorieDoPickeru();
            ObnovitSeznam();
        }

        private void NacistKategorieDoPickeru()
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
            var sablonyZDatabaze = _sablonyService.GetSablony();
            var seznamZobrazeni = new List<SablonaZobrazeni>();

            foreach (var sablona in sablonyZDatabaze)
            {
                var kategorie = _dostupneKategorie.FirstOrDefault(k => k.Id == sablona.KategorieId);

                seznamZobrazeni.Add(new SablonaZobrazeni
                {
                    SablonaPuvodni = sablona,
                    KategorieNazev = kategorie != null ? kategorie.Nazev : "Neznámá kategorie",
                    KategorieBarva = kategorie != null ? kategorie.Barva : "#808080"
                });
            }

            SablonyList.ItemsSource = seznamZobrazeni;
        }

        private void OnVstupZmenen(object sender, EventArgs e)
        {
            ChybaLabel.IsVisible = false;
        }

        private void OnPridatClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NazevEntry.Text) ||
                string.IsNullOrWhiteSpace(CastkaEntry.Text) ||
                TypPicker.SelectedIndex == -1 ||
                KategoriePicker.SelectedItem == null)
            {
                ChybaLabel.Text = "Vyplňte prosím všechna pole!";
                ChybaLabel.IsVisible = true;
                return;
            }

            if (!decimal.TryParse(CastkaEntry.Text, out decimal castka) || castka < 0)
            {
                ChybaLabel.Text = "Částka musí být platné číslo!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // 0 = Výdaj, 1 = Příjem
            var zvolenyTyp = TypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;
            var zvolenaKategorie = (Kategorie)KategoriePicker.SelectedItem;

            var novaSablona = new SablonaPlatby
            {
                Nazev = NazevEntry.Text,
                Castka = castka,
                Typ = zvolenyTyp,
                KategorieId = zvolenaKategorie.Id
            };

            _sablonyService.PridatSablonu(novaSablona);

            NazevEntry.Text = "";
            CastkaEntry.Text = "";
            TypPicker.SelectedIndex = 0;

            if (_dostupneKategorie.Count > 0)
            {
                KategoriePicker.SelectedIndex = 0;
            }

            ObnovitSeznam();
        }

        private void OnSmazatClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _sablonaKeSmazani = (SablonaPlatby)tlacitko.CommandParameter;

            SmazatTextLabel.Text = $"Opravdu chcete smazat šablonu '{_sablonaKeSmazani.Nazev}'?";
            SmazatOverlay.IsVisible = true;
        }

        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false;
            _sablonaKeSmazani = null;
        }

        private void OnPotvrditSmazaniClicked(object sender, EventArgs e)
        {
            if (_sablonaKeSmazani != null)
            {
                _sablonyService.SmazatSablonu(_sablonaKeSmazani.Id);
                ObnovitSeznam();
                SmazatOverlay.IsVisible = false;
                _sablonaKeSmazani = null;
            }
        }

        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _sablonaKUprave = (SablonaPlatby)tlacitko.CommandParameter;

            UpravitNazevEntry.Text = _sablonaKUprave.Nazev;
            UpravitCastkaEntry.Text = _sablonaKUprave.Castka.ToString();
            UpravitTypPicker.SelectedIndex = _sablonaKUprave.Typ == TypTransakce.Vydaj ? 0 : 1;

            UpravitKategoriePicker.SelectedItem = _dostupneKategorie.FirstOrDefault(k => k.Id == _sablonaKUprave.KategorieId);

            UpravitOverlay.IsVisible = true;
        }

        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _sablonaKUprave = null;
        }

        private void OnUlozitUpravuClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UpravitNazevEntry.Text) || string.IsNullOrWhiteSpace(UpravitCastkaEntry.Text))
                return;

            if (!decimal.TryParse(UpravitCastkaEntry.Text, out decimal castka))
                return;

            if (_sablonaKUprave != null)
            {
                _sablonaKUprave.Nazev = UpravitNazevEntry.Text;
                _sablonaKUprave.Castka = castka;
                _sablonaKUprave.Typ = UpravitTypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;

                if (UpravitKategoriePicker.SelectedItem is Kategorie vybranaKat)
                {
                    _sablonaKUprave.KategorieId = vybranaKat.Id;
                }

                _sablonyService.UpravitSablonu(_sablonaKUprave);
                ObnovitSeznam();

                UpravitOverlay.IsVisible = false;
                _sablonaKUprave = null;
            }
        }
    }

    public class SablonaZobrazeni
    {
        public SablonaPlatby SablonaPuvodni { get; set; } = null!;

        public string Nazev => SablonaPuvodni.Nazev;
        public string KategorieNazev { get; set; } = "";
        public string KategorieBarva { get; set; } = "";

        public string CastkaZobrazeni => SablonaPuvodni.Typ == TypTransakce.Prijem
            ? $"+ {SablonaPuvodni.Castka} Kč"
            : $"- {SablonaPuvodni.Castka} Kč";

        public string BarvaTypu => SablonaPuvodni.Typ == TypTransakce.Prijem ? "#28A745" : "#DC3545";
    }
}