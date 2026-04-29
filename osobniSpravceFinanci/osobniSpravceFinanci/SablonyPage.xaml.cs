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

        // pomocne promenne
        private SablonaPlatby? _sablonaKeSmazani;
        private SablonaPlatby? _sablonaKUprave;

        // seznamy kateorii
        private List<Kategorie> _dostupneKategorie = new List<Kategorie>();
        private List<Kategorie> _vsechnyKategorie = new List<Kategorie>();

        // konstruktor
        public SablonyPage(SablonyService sablonyService, KategorieService kategorieService)
        {
            InitializeComponent();
            _sablonyService = sablonyService;
            _kategorieService = kategorieService;
        }

        // po zapnuti stranky
        protected override void OnAppearing()
        {
            base.OnAppearing();

            NacistKategorieDoPickeru();
            ObnovitSeznam();
        }

        // nacteni kategorii
        private void NacistKategorieDoPickeru()
        {
            _dostupneKategorie = _kategorieService.GetAktivniKategorie();
            _vsechnyKategorie = _kategorieService.GetVsechnyKategorie();

            KategoriePicker.ItemsSource = _dostupneKategorie;
            UpravitKategoriePicker.ItemsSource = _dostupneKategorie;

            TypPicker.SelectedIndex = 0;

            KategoriePicker.SelectedItem = null;
        }

        // nacteni ulozenych sablon
        private void ObnovitSeznam()
        {
            var sablonyZDatabaze = _sablonyService.GetSablony();
            var seznamZobrazeni = new List<SablonaZobrazeni>();

            foreach (var sablona in sablonyZDatabaze)
            {
                // nalezeni kategorie k sablone
                var kategorie = _vsechnyKategorie.FirstOrDefault(k => k.Id == sablona.KategorieId);

                // prevedeni do tridy pro zobrazeni
                seznamZobrazeni.Add(new SablonaZobrazeni
                {
                    SablonaPuvodni = sablona,
                    KategorieNazev = kategorie != null ? kategorie.Nazev : "Neznámá kategorie",
                    KategorieBarva = kategorie != null ? kategorie.Barva : "#808080"
                });
            }

            // odeslani dat do gui
            BindableLayout.SetItemsSource(SablonyList, seznamZobrazeni);
        }

        // skryti chybove hlasky
        private void OnVstupZmenen(object sender, EventArgs e)
        {
            ChybaLabel.IsVisible = false;
        }

        // pridani sablony
        private void OnPridatClicked(object sender, EventArgs e)
        {
            // kontrola inputu
            if (string.IsNullOrWhiteSpace(NazevEntry.Text) ||
                string.IsNullOrWhiteSpace(CastkaEntry.Text) ||
                TypPicker.SelectedIndex == -1)
            {
                ChybaLabel.Text = "Vyplňte prosím všechna pole!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // kontrola castky
            if (!decimal.TryParse(CastkaEntry.Text, out decimal castka) || castka < 0)
            {
                ChybaLabel.Text = "Částka musí být platné číslo!";
                ChybaLabel.IsVisible = true;
                return;
            }

            // 0 - vydej, 1 - prijem
            var zvolenyTyp = TypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;

            // pri nevybrani kategorie -> neznama
            Kategorie zvolenaKategorie;
            if (KategoriePicker.SelectedItem != null)
            {
                zvolenaKategorie = (Kategorie)KategoriePicker.SelectedItem;
            }
            else
            {
                zvolenaKategorie = _kategorieService.GetKategorieNeznama();
            }

            // ulozeni sablony do databaze
            var novaSablona = new SablonaPlatby
            {
                Nazev = NazevEntry.Text,
                Castka = castka,
                Typ = zvolenyTyp,
                KategorieId = zvolenaKategorie.Id
            };

            _sablonyService.PridatSablonu(novaSablona);

            // reset inputu
            NazevEntry.Text = "";
            CastkaEntry.Text = "";
            TypPicker.SelectedIndex = 0;
            KategoriePicker.SelectedItem = null;

            ObnovitSeznam();
        }

        // tlacitko smazani sablony
        private void OnSmazatClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _sablonaKeSmazani = (SablonaPlatby)tlacitko.CommandParameter;

            SmazatTextLabel.Text = $"Opravdu chcete smazat šablonu '{_sablonaKeSmazani.Nazev}'?";
            SmazatOverlay.IsVisible = true;
        }

        // zruseni smazani sablony
        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false;
            _sablonaKeSmazani = null;
        }

        // potvrzeni smazani sablony
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

        // tlacitko upravy sablony
        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _sablonaKUprave = (SablonaPlatby)tlacitko.CommandParameter;

            // vyplneni inputu
            UpravitNazevEntry.Text = _sablonaKUprave.Nazev;
            UpravitCastkaEntry.Text = _sablonaKUprave.Castka.ToString();
            UpravitTypPicker.SelectedIndex = _sablonaKUprave.Typ == TypTransakce.Vydaj ? 0 : 1;

            UpravitKategoriePicker.SelectedItem = _dostupneKategorie.FirstOrDefault(k => k.Id == _sablonaKUprave.KategorieId);

            UpravitOverlay.IsVisible = true;
        }

        // zruseni upravy sablony
        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _sablonaKUprave = null;
        }

        // ulozeni upravy sablony
        private void OnUlozitUpravuClicked(object sender, EventArgs e)
        {
            // kontrola inputu a castky
            if (string.IsNullOrWhiteSpace(UpravitNazevEntry.Text) || string.IsNullOrWhiteSpace(UpravitCastkaEntry.Text))
                return;

            if (!decimal.TryParse(UpravitCastkaEntry.Text, out decimal castka))
                return;

            if (_sablonaKUprave != null)
            {
                // aktualizace dat
                _sablonaKUprave.Nazev = UpravitNazevEntry.Text;
                _sablonaKUprave.Castka = castka;
                _sablonaKUprave.Typ = UpravitTypPicker.SelectedIndex == 0 ? TypTransakce.Vydaj : TypTransakce.Prijem;

                if (UpravitKategoriePicker.SelectedItem is Kategorie vybranaKat)
                {
                    _sablonaKUprave.KategorieId = vybranaKat.Id;
                }

                // ulozeni do databaze a obnoveni
                _sablonyService.UpravitSablonu(_sablonaKUprave);
                ObnovitSeznam();

                UpravitOverlay.IsVisible = false;
                _sablonaKUprave = null;
            }
        }
    }

    // trida pro zobrazeni sablon
    public class SablonaZobrazeni
    {
        public SablonaPlatby SablonaPuvodni { get; set; } = null!;

        public string Nazev => SablonaPuvodni.Nazev;
        public string KategorieNazev { get; set; } = "";
        public string KategorieBarva { get; set; } = "";

        // naformatovani vzhledu castky
        public string CastkaZobrazeni => SablonaPuvodni.Typ == TypTransakce.Prijem
            ? $"+ {SablonaPuvodni.Castka} Kč"
            : $"- {SablonaPuvodni.Castka} Kč";

        public string BarvaTypu => SablonaPuvodni.Typ == TypTransakce.Prijem ? "#28A745" : "#DC3545";
    }
}