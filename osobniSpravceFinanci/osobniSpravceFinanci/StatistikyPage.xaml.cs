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

        public StatistikyPage(TransakceService transakceService, KategorieService kategorieService)
        {
            InitializeComponent();
            _transakceService = transakceService;
            _kategorieService = kategorieService;

            ObdobiPicker.SelectedIndex = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VypocitatStatistiky();
        }

        private void OnObdobiZmeneno(object sender, EventArgs e)
        {
            VypocitatStatistiky();
        }

        private void VypocitatStatistiky()
        {
            if (ObdobiPicker.SelectedIndex == -1) return;

            var vsechnyTransakce = _transakceService.GetVsechnyTransakce();
            var vsechnyKategorie = _kategorieService.GetVsechnyKategorie();

            DateTime dnes = DateTime.Today;
            List<Transakce> filtrovaneTransakce = new List<Transakce>();

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

            decimal celkovePrijmy = filtrovaneTransakce.Where(t => t.Typ == TypTransakce.Prijem).Sum(t => t.Castka);
            decimal celkoveVydaje = filtrovaneTransakce.Where(t => t.Typ == TypTransakce.Vydaj).Sum(t => t.Castka);
            decimal bilance = celkovePrijmy - celkoveVydaje;

            PrijmyLabel.Text = $"{celkovePrijmy:N0} Kč";
            VydajeLabel.Text = $"{celkoveVydaje:N0} Kč";
            BilanceLabel.Text = $"{bilance:N0} Kč";
            BilanceLabel.TextColor = bilance >= 0 ? Color.FromArgb("#28A745") : Color.FromArgb("#DC3545");

            var vydaje = filtrovaneTransakce.Where(t => t.Typ == TypTransakce.Vydaj).ToList();

            var kategorieSporeni = vsechnyKategorie.FirstOrDefault(k => k.Nazev == "Spoření");
            int idSporeni = kategorieSporeni != null ? kategorieSporeni.Id : -1;

            decimal celkoveVydajeProGraf = vydaje.Where(t => t.KategorieId != idSporeni).Sum(t => t.Castka);

            var statistikyKategorii = new List<KategorieStatistika>();

            if (celkoveVydajeProGraf > 0)
            {
                var seskupeneVydaje = vydaje.Where(t => t.KategorieId != idSporeni).GroupBy(t => t.KategorieId);

                foreach (var skupina in seskupeneVydaje)
                {
                    var katId = skupina.Key;
                    var kategorie = vsechnyKategorie.FirstOrDefault(k => k.Id == katId);

                    decimal sumaKategorie = skupina.Sum(t => t.Castka);

                    statistikyKategorii.Add(new KategorieStatistika
                    {
                        Nazev = kategorie?.Nazev ?? "Neznámá",
                        Barva = kategorie?.Barva ?? "#808080",
                        Castka = sumaKategorie,
                        Procenta = (double)(sumaKategorie / celkoveVydajeProGraf)
                    });
                }

                statistikyKategorii = statistikyKategorii.OrderByDescending(s => s.Castka).ToList();
            }

            BindableLayout.SetItemsSource(KategorieStatistikyList, statistikyKategorii);
        }
    }

    public class KategorieStatistika
    {
        public string Nazev { get; set; } = "";
        public string Barva { get; set; } = "";
        public decimal Castka { get; set; }
        public double Procenta { get; set; }

        public string CastkaZobrazeni => $"{Castka:N0} Kč";
        public string ProcentaZobrazeni => $"{(Procenta * 100):0.0} %";
    }
}