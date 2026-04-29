using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using osobniSpravceFinanci.Models;
using osobniSpravceFinanci.Services;

namespace osobniSpravceFinanci
{
    public partial class StatistikyPage : ContentPage
    {
        private readonly TransakceService _transakceService;
        private readonly KategorieService _kategorieService;

        // konstruktor
        public StatistikyPage(TransakceService transakceService, KategorieService kategorieService)
        {
            InitializeComponent();
            _transakceService = transakceService;
            _kategorieService = kategorieService;

            ObdobiPicker.SelectedIndex = 0;
        }

        // po zapnuti stranky
        protected override void OnAppearing()
        {
            base.OnAppearing();
            VypocitatStatistiky();
        }

        // zmena vyberu obdobi pro statistiku
        private void OnObdobiZmeneno(object sender, EventArgs e)
        {
            VypocitatStatistiky();
        }

        // vypocet statistiky
        private void VypocitatStatistiky()
        {
            // kontrola zda je vybrano obdobi
            if (ObdobiPicker.SelectedIndex == -1) return;

            var vsechnyTransakce = _transakceService.GetVsechnyTransakce();
            var vsechnyKategorie = _kategorieService.GetVsechnyKategorie();

            DateTime dnes = DateTime.Today;
            List<Transakce> filtrovaneTransakce = new List<Transakce>();

            // volba obdobi
            switch (ObdobiPicker.SelectedIndex)
            {
                case 0: // Tento mesic
                    filtrovaneTransakce = vsechnyTransakce.Where(t => t.Datum.Month == dnes.Month && t.Datum.Year == dnes.Year).ToList();
                    break;
                case 1: // Minuly mesic
                    DateTime minulyMesic = dnes.AddMonths(-1);
                    filtrovaneTransakce = vsechnyTransakce.Where(t => t.Datum.Month == minulyMesic.Month && t.Datum.Year == minulyMesic.Year).ToList();
                    break;
                case 2: // Tento rok
                    filtrovaneTransakce = vsechnyTransakce.Where(t => t.Datum.Year == dnes.Year).ToList();
                    break;
                case 3: // Cela historie
                    filtrovaneTransakce = vsechnyTransakce.ToList();
                    break;
            }

            // vypocet souhrnu vydaju a prijmu 
            decimal celkovePrijmy = filtrovaneTransakce.Where(t => t.Typ == TypTransakce.Prijem).Sum(t => t.Castka);
            decimal celkoveVydaje = filtrovaneTransakce.Where(t => t.Typ == TypTransakce.Vydaj).Sum(t => t.Castka);
            decimal bilance = celkovePrijmy - celkoveVydaje;

            // doplneni do gui
            PrijmyLabel.Text = $"{celkovePrijmy:N0} Kč";
            VydajeLabel.Text = $"{celkoveVydaje:N0} Kč";
            BilanceLabel.Text = $"{bilance:N0} Kč";
            BilanceLabel.TextColor = bilance >= 0 ? Color.FromArgb("#28A745") : Color.FromArgb("#DC3545");

            // vypocet grafu podle kategorii

            // filtrace vydaju
            var vydaje = filtrovaneTransakce.Where(t => t.Typ == TypTransakce.Vydaj).ToList();

            // vyrazeni sporeni z vydaju (neni to vydaj jen presun penez)
            var kategorieSporeni = vsechnyKategorie.FirstOrDefault(k => k.Nazev == "Spoření");
            int idSporeni = kategorieSporeni != null ? kategorieSporeni.Id : -1;

            // vypocet sumy vydaju
            decimal celkoveVydajeProGraf = vydaje.Where(t => t.KategorieId != idSporeni).Sum(t => t.Castka);

            var statistikyKategorii = new List<KategorieStatistika>();

            if (celkoveVydajeProGraf > 0)
            {
                // seskupeni vydaju podle kategorie
                var seskupeneVydaje = vydaje.Where(t => t.KategorieId != idSporeni).GroupBy(t => t.KategorieId);

                // prochazeni jednotlivych kategorii
                foreach (var skupina in seskupeneVydaje)
                {
                    var katId = skupina.Key;
                    var kategorie = vsechnyKategorie.FirstOrDefault(k => k.Id == katId);

                    // vydaje dane kategorie
                    decimal sumaKategorie = skupina.Sum(t => t.Castka);

                    // zobrazeni a vypocet procent
                    statistikyKategorii.Add(new KategorieStatistika
                    {
                        Nazev = kategorie?.Nazev ?? "Neznámá",
                        Barva = kategorie?.Barva ?? "#808080",
                        Castka = sumaKategorie,
                        Procenta = (double)(sumaKategorie / celkoveVydajeProGraf)
                    });
                }

                // serazeni grafu sestupne
                statistikyKategorii = statistikyKategorii.OrderByDescending(s => s.Castka).ToList();
            }

            // odeslani dat do gui
            BindableLayout.SetItemsSource(KategorieStatistikyList, statistikyKategorii);
        }
    }

    // trida pro zobrazeni kategorie
    public class KategorieStatistika
    {
        public string Nazev { get; set; } = "";
        public string Barva { get; set; } = "";
        public decimal Castka { get; set; }
        public double Procenta { get; set; }

        // naformatovani castky a procent 
        public string CastkaZobrazeni => $"{Castka:N0} Kč";
        public string ProcentaZobrazeni => $"{(Procenta * 100):0.0} %";
    }
}