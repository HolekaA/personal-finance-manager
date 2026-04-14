using osobniSpravceFinanci.Models;
using osobniSpravceFinanci.Services;

namespace osobniSpravceFinanci
{
    public partial class KategoriePage : ContentPage
    {
        private readonly KategorieService _kategorieService;

        private string _zvolenaBarva = "#007AFF";

        private Kategorie? _kategorieKeSmazani;
        private Kategorie? _kategorieKUprave;
        private string _zvolenaBarvaProUpravu = "";

        public KategoriePage(KategorieService kategorieService)
        {
            InitializeComponent();
            _kategorieService = kategorieService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ObnovitSeznam();
        }

        private void ObnovitSeznam()
        {
            var data = _kategorieService.GetAktivniKategorie();
            BindableLayout.SetItemsSource(KategorieList, data);
        }

        private void OnNazevZmenen(object sender, TextChangedEventArgs e)
        {
            ChybaLabel.IsVisible = false;
        }

        private void OnBarvaZvolena(object sender, EventArgs e)
        {
            var kliknuteTlacitko = (Button)sender;
            _zvolenaBarva = kliknuteTlacitko.CommandParameter.ToString();

            foreach (var prvek in PaletaBarevLayout.Children)
            {
                if (prvek is Button btn)
                {
                    btn.BorderWidth = 0;
                }
            }

            kliknuteTlacitko.BorderWidth = 3;
            kliknuteTlacitko.BorderColor = Color.Parse("#333333");
        }

        private void OnPridatClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NazevEntry.Text))
            {
                ChybaLabel.Text = "Název kategorie nesmí zůstat prázdný!";
                ChybaLabel.IsVisible = true;
                return;
            }

            var novaKategorie = new Kategorie
            {
                Nazev = NazevEntry.Text,
                Barva = _zvolenaBarva,
                JeAktivni = true
            };

            _kategorieService.PridatKategorii(novaKategorie);

            NazevEntry.Text = "";

            _zvolenaBarva = "#007AFF";
            foreach (var prvek in PaletaBarevLayout.Children)
            {
                if (prvek is Button btn)
                {
                    btn.BorderWidth = 0;

                    if (btn.CommandParameter.ToString() == "#007AFF")
                    {
                        btn.BorderWidth = 3;
                        btn.BorderColor = Color.Parse("#333333");
                    }
                }
            }

            ObnovitSeznam();
        }

        private void OnSmazatClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _kategorieKeSmazani = (Kategorie)tlacitko.CommandParameter;

            SmazatTextLabel.Text = $"Opravdu chcete smazat kategorii '{_kategorieKeSmazani.Nazev}'?";

            SmazatOverlay.IsVisible = true;
        }

        private void OnZrusitSmazaniClicked(object sender, EventArgs e)
        {
            SmazatOverlay.IsVisible = false; 
            _kategorieKeSmazani = null;
        }

        private void OnPotvrditSmazaniClicked(object sender, EventArgs e)
        {
            if (_kategorieKeSmazani != null)
            {
                _kategorieService.SmazatKategorii(_kategorieKeSmazani.Id);
                ObnovitSeznam();

                SmazatOverlay.IsVisible = false;
                _kategorieKeSmazani = null;
            }
        }

        private void OnUpravitClicked(object sender, EventArgs e)
        {
            var tlacitko = (Button)sender;
            _kategorieKUprave = (Kategorie)tlacitko.CommandParameter;

            UpravitNazevEntry.Text = _kategorieKUprave.Nazev;
            _zvolenaBarvaProUpravu = _kategorieKUprave.Barva;

            foreach (var prvek in PaletaBarevUpravaLayout.Children)
            {
                if (prvek is Button btn)
                {
                    if (btn.CommandParameter.ToString() == _zvolenaBarvaProUpravu)
                    {
                        btn.BorderWidth = 3;
                        btn.BorderColor = Color.Parse("#333333");
                    }
                    else
                    {
                        btn.BorderWidth = 0;
                    }
                }
            }

            UpravitOverlay.IsVisible = true;
        }

        private void OnBarvaUpravaZvolena(object sender, EventArgs e)
        {
            var kliknuteTlacitko = (Button)sender;
            _zvolenaBarvaProUpravu = kliknuteTlacitko.CommandParameter.ToString();

            foreach (var prvek in PaletaBarevUpravaLayout.Children)
            {
                if (prvek is Button btn)
                {
                    btn.BorderWidth = 0;
                }
            }

            kliknuteTlacitko.BorderWidth = 3;
            kliknuteTlacitko.BorderColor = Color.Parse("#333333");
        }

        private void OnZrusitUpravuClicked(object sender, EventArgs e)
        {
            UpravitOverlay.IsVisible = false;
            _kategorieKUprave = null;
        }

        private void OnUlozitUpravuClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UpravitNazevEntry.Text))
            {
                return;
            }

            if (_kategorieKUprave != null)
            {
                _kategorieKUprave.Nazev = UpravitNazevEntry.Text;
                _kategorieKUprave.Barva = _zvolenaBarvaProUpravu;

                _kategorieService.UpravitKategorii(_kategorieKUprave);
                ObnovitSeznam();

                UpravitOverlay.IsVisible = false;
                _kategorieKUprave = null;
            }
        }
    }
}