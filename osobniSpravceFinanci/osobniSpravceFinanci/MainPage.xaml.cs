using osobniSpravceFinanci.Services;
using osobniSpravceFinanci.Models;

namespace osobniSpravceFinanci
{
    public partial class MainPage : ContentPage
    {
        private readonly KategorieService _kategorieService;

        public MainPage(KategorieService kategorieService)
        {
            InitializeComponent();

            _kategorieService = kategorieService;

        }

    }
}
