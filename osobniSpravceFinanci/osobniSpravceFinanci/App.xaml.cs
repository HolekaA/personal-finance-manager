using Microsoft.Extensions.DependencyInjection;
using osobniSpravceFinanci.Services;

namespace osobniSpravceFinanci
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DatabaseSeeder.NaplnitTestovaciData();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}