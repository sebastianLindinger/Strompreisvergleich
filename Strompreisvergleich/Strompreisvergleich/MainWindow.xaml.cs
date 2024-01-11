using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Aspose.Cells;
using Aspose.Cells.Charts;
using Microsoft.Extensions.Configuration;
using StromDbLib;

namespace Strompreisvergleich;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly StromDbContext db;
    private readonly string analyseExcelPath;

    public MainWindow(StromDbContext db, IServiceProvider services, IConfiguration configuration)
    {
        InitializeComponent();
        this.db = db;
        //db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        Title = db.Stromverbrauch.Count() + "";

        analyseExcelPath = configuration["AnalyseExcelPath"];
    }

    private void RectangleDropVerbrauch_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            var worksheet = new Workbook(file).Worksheets[0];

            HandleExcelDrop(worksheet);
        }
    }

    private void RectangleDropStrompreis_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            var worksheet = new Workbook(file).Worksheets[0];

            HandleExcelDrop(worksheet);
        }
    }

    private void HandleExcelDrop(Worksheet worksheet)
    {
        bool isAwattar = worksheet.Name.ToLower().Contains("awattar");
        bool isWarmepumpe = !isAwattar && worksheet.Name.ToLower().Contains("eigendeckung");

        int rows = worksheet.Cells.MaxDataRow;

        for (int i = 1; i <= rows; i++)
        {
            if (isAwattar)
            {
                Strompreis strompreis = new()
                {
                    Von = worksheet.Cells[i, 0].DateTimeValue,
                    Bis = worksheet.Cells[i, 1].DateTimeValue,
                    Preis = worksheet.Cells[i, 2].DoubleValue
                };

                if (!IsAlreadyInserted(strompreis))
                {
                    db.Strompreis.Add(strompreis);
                }

                continue;
            }

            Stromverbrauch stromverbrauch = new()
            {
                Zeitpunkt = DateTime.ParseExact(worksheet.Cells[i, 0].StringValue, "dd.MM.yyyy HH:mm", null),
                Verbrauch = double.Parse(worksheet.Cells[i, 1].StringValue, NumberStyles.Any, CultureInfo.InvariantCulture),
                IsWaermepumpe = isWarmepumpe,
            };

            if (!IsAlreadyInserted(stromverbrauch))
            {
                db.Stromverbrauch.Add(stromverbrauch);
            }
        }

        db.SaveChanges();

        Title = (isAwattar ? db.Strompreis.Count() : db.Stromverbrauch.Count()) + "";
    }

    private void Button_Analyse_Click(object sender, RoutedEventArgs e)
    {
        if (!CheckParameter(out double fixpreis))
        {
            return;
        }

        DateTime from = datepickerFrom.SelectedDate!.Value;
        DateTime to = datepickerTo.SelectedDate!.Value;


        Workbook workbook = new Workbook();

        Worksheet worksheet = workbook.Worksheets[0];

        List<AnalyseItem> haushaltItems = new();
        Stromdaten gesamtStromdaten = new();

        int max = Math.Abs((to - from).Days);
        for (int i = 0; i <= max; i++)
        {
            DateTime day = from.AddDays(i);
            worksheet.Cells[i + 1, 0].Value = day.ToShortDateString();

            var stromdaten = CalculateStromkostenAndVerbrauchForDay(day, false, fixpreis);
            gesamtStromdaten.Verbrauch += stromdaten.Verbrauch;
            gesamtStromdaten.KostenAwattar += stromdaten.KostenAwattar;
            gesamtStromdaten.KostenFix += stromdaten.KostenFix;

            haushaltItems.Add(new()
            {
                Tag = from.AddDays(i).ToShortDateString(),
                Verbrauch = stromdaten.Verbrauch.ToString("0.00 kWh"),
                KostenAwattar = (stromdaten.KostenAwattar / 100).ToString("0.000 €"),
                KostenFix = (stromdaten.KostenFix / 100).ToString("0.000 €"),
                Info = stromdaten.KostenAwattar > stromdaten.KostenFix ? "Fixe Kosten sind besser" : "",
            });
        }
        haushaltItems.Insert(0, new()
        {
            Tag = "Gesamt",
            Verbrauch = gesamtStromdaten.Verbrauch.ToString("0.00 kWh"),
            KostenAwattar = (gesamtStromdaten.KostenAwattar / 100).ToString("0.000 €"),
            KostenFix = (gesamtStromdaten.KostenFix / 100).ToString("0.000 €"),
            Info = gesamtStromdaten.KostenAwattar > gesamtStromdaten.KostenFix ? "Fixe Kosten sind besser" : "",
        });

        datagridHaushalt.ItemsSource = haushaltItems;

        List<AnalyseItem> waermepumpeItems = new();
        gesamtStromdaten = new();

        for (int i = 0; i <= max; i++)
        {
            DateTime day = from.AddDays(i);

            var stromdaten = CalculateStromkostenAndVerbrauchForDay(day, true, fixpreis);
            gesamtStromdaten.Verbrauch += stromdaten.Verbrauch;
            gesamtStromdaten.KostenAwattar += stromdaten.KostenAwattar;
            gesamtStromdaten.KostenFix += stromdaten.KostenFix;

            waermepumpeItems.Add(new()
            {
                Tag = from.AddDays(i).ToShortDateString(),
                Verbrauch = stromdaten.Verbrauch.ToString("0.00 kWh"),
                KostenAwattar = (stromdaten.KostenAwattar / 100).ToString("0.000 €"),
                KostenFix = (stromdaten.KostenFix / 100).ToString("0.000 €"),
                Info = stromdaten.KostenAwattar > stromdaten.KostenFix ? "Fixe Kosten sind besser" : "",
            });
        }
        waermepumpeItems.Insert(0, new()
        {
            Tag = "Gesamt",
            Verbrauch = gesamtStromdaten.Verbrauch.ToString("0.00 kWh"),
            KostenAwattar = (gesamtStromdaten.KostenAwattar / 100).ToString("0.000 €"),
            KostenFix = (gesamtStromdaten.KostenFix / 100).ToString("0.000 €"),
            Info = gesamtStromdaten.KostenAwattar > gesamtStromdaten.KostenFix ? "Fixe Kosten sind besser" : "",
        });

        datagridWaermepumpe.ItemsSource = waermepumpeItems;
    }

    private void Button_GenerateExcel_Click(object sender, RoutedEventArgs e)
    {
        if (!CheckParameter(out double fixpreis))
        {
            return;
        }

        DateTime from = datepickerFrom.SelectedDate!.Value;
        DateTime to = datepickerTo.SelectedDate!.Value;


        Workbook workbook = new Workbook();

        Worksheet worksheet = workbook.Worksheets[0];

        int max = Math.Abs((to - from).Days);

        worksheet.Cells[0, 0].Value = "Datum";
        worksheet.Cells[1, 0].Value = "Gesamt";

        for (int i = 0; i <= max; i++)
        {
            DateTime day = from.AddDays(i);
            worksheet.Cells[i + 2, 0].Value = day.ToShortDateString();
        }

        Stromdaten gesamtStromdaten = new();

        worksheet.Cells[0, 1].Value = "Haushalt Verbrauch";
        worksheet.Cells[0, 2].Value = "Haushalt AWATTar";
        worksheet.Cells[0, 3].Value = "Haushalt Fix";

        for (int i = 0; i <= max; i++)
        {
            DateTime day = from.AddDays(i);

            var stromdaten = CalculateStromkostenAndVerbrauchForDay(day, false, fixpreis);
            worksheet.Cells[i + 2, 1].Value = stromdaten.Verbrauch;
            worksheet.Cells[i + 2, 2].Value = stromdaten.KostenAwattar / 100;
            worksheet.Cells[i + 2, 3].Value = stromdaten.KostenFix / 100;

            gesamtStromdaten.Verbrauch += stromdaten.Verbrauch;
            gesamtStromdaten.KostenAwattar += stromdaten.KostenAwattar;
            gesamtStromdaten.KostenFix += stromdaten.KostenFix;
        }
        worksheet.Cells[1, 1].Value = $"{gesamtStromdaten.Verbrauch:0.00 kWh}";
        worksheet.Cells[1, 2].Value = $"{(gesamtStromdaten.KostenAwattar / 100):0.000 €}";
        worksheet.Cells[1, 3].Value = $"{(gesamtStromdaten.KostenFix / 100):0.000 €}"; ;

        gesamtStromdaten = new();

        worksheet.Cells[0, 4].Value = "Wärmepumpe Verbrauch";
        worksheet.Cells[0, 5].Value = "Wärmepumpe AWATTar";
        worksheet.Cells[0, 6].Value = "Wärmepumpe Fix";

        for (int i = 0; i <= max; i++)
        {
            DateTime day = from.AddDays(i);

            var stromdaten = CalculateStromkostenAndVerbrauchForDay(day, true, fixpreis);
            worksheet.Cells[i + 2, 4].Value = stromdaten.Verbrauch;
            worksheet.Cells[i + 2, 5].Value = stromdaten.KostenAwattar / 100;
            worksheet.Cells[i + 2, 6].Value = stromdaten.KostenFix / 100;

            gesamtStromdaten.Verbrauch += stromdaten.Verbrauch;
            gesamtStromdaten.KostenAwattar += stromdaten.KostenAwattar;
            gesamtStromdaten.KostenFix += stromdaten.KostenFix;
        }
        worksheet.Cells[1, 4].Value = $"{gesamtStromdaten.Verbrauch:0.00 kWh}";
        worksheet.Cells[1, 5].Value = $"{(gesamtStromdaten.KostenAwattar / 100):0.000 €}";
        worksheet.Cells[1, 6].Value = $"{(gesamtStromdaten.KostenFix / 100):0.000 €}"; ;

        int idx = worksheet.Charts.Add(ChartType.Line, 1, 8, 25, 25);

        string xValues = $"A{3}: A{max + 3}";
        Chart chart = worksheet.Charts[idx];

        chart.Style = 3;
        chart.AutoScaling = true;
        chart.PlotArea.Area.ForegroundColor = Color.White;
        chart.ValueAxis.Title.Text = "Kosten in €";

        chart.Title.Text = "Stromkosten";

        int s1_idx = chart.NSeries.Add(xValues, true);

        chart.NSeries[s1_idx].XValues = xValues;
        chart.NSeries[s1_idx].Values = $"C{3}: C{max + 3}";
        chart.NSeries[s1_idx].Name = "Haushalt aWATTar";
        chart.NSeries[s1_idx].Border.Color = Color.LightBlue;

        int s2_idx = chart.NSeries.Add(xValues, true);

        chart.NSeries[s2_idx].XValues = xValues;
        chart.NSeries[s2_idx].Values = $"D{3}: D{max + 3}";
        chart.NSeries[s2_idx].Name = "Haushalt Fix";
        chart.NSeries[s2_idx].Border.Color =Color.DarkBlue;

        int s3_idx = chart.NSeries.Add(xValues, true);

        chart.NSeries[s3_idx].XValues = xValues;
        chart.NSeries[s3_idx].Values = $"F{3}: F{max + 3}";
        chart.NSeries[s3_idx].Name = "Wärmepumpe aWATTar";
        chart.NSeries[s3_idx].Border.Color = Color.FromArgb(255, 166, 0);

        int s4_idx = chart.NSeries.Add(xValues, true);

        chart.NSeries[s4_idx].XValues = xValues;
        chart.NSeries[s4_idx].Values = $"G{3}: G{max + 3}";
        chart.NSeries[s4_idx].Name = "Wärmepumpe Fix";
        chart.NSeries[s4_idx].Border.Color = Color.FromArgb(255, 99, 97);

        try
        {
            DateTime now = DateTime.Now;

            string path = Path.Combine(analyseExcelPath, $"analyse-{now:yyyy-MM-dd}-{now.Ticks.ToString()[14..]}.xlsx");
            workbook.Save(path, SaveFormat.Xlsx);
        }
        catch (Exception)
        {

        }
    }

    #region Helpers
    private bool IsAlreadyInserted(Strompreis strompreis)
    {
        return db.Strompreis.Any(x => x.Von == strompreis.Von && x.Bis == strompreis.Bis);
    }

    private bool IsAlreadyInserted(Stromverbrauch stromverbrauch)
    {
        return db.Stromverbrauch.Any(x => x.Zeitpunkt == stromverbrauch.Zeitpunkt && x.IsWaermepumpe == stromverbrauch.IsWaermepumpe);
    }

    private Stromdaten CalculateStromkostenAndVerbrauchForDay(DateTime day, bool isWaermepumpe, double fixpreis)
    {
        List<Strompreis> strompreise = db.Strompreis
            .Where(x => x.Von.Date == day.Date)
            .ToList();

        List<IGrouping<int, Stromverbrauch>>? stromverbrauch = db.Stromverbrauch
            .Where(x => x.Zeitpunkt.Date == day.Date && x.IsWaermepumpe == isWaermepumpe)
            .ToList()
            .GroupBy(x => x.Zeitpunkt.Hour)
            .ToList();

        Stromdaten result = new();

        for (int i = 0; i < 24 && i < stromverbrauch.Count; i++)
        {
            double stromverbrauchForHour = stromverbrauch[i].Sum(x => x.Verbrauch);
            result.Verbrauch += stromverbrauchForHour;

            result.KostenAwattar += GetStrompreisFromFormular(strompreise.FirstOrDefault(x => x.Von.Hour == i)?.Preis) * stromverbrauchForHour;
            result.KostenFix += fixpreis * stromverbrauchForHour;
        }

        return result;
    }

    private double GetStrompreisFromFormular(double? preis)
    {
        if (!preis.HasValue) return 0;

        return preis.Value + Math.Abs(preis.Value) * 0.03 + 1.5;
    }

    private bool CheckParameter(out double fixpreis)
    {
        fixpreis = 0;

        if (datepickerFrom.SelectedDate is null || datepickerTo.SelectedDate is null)
        {
            return false;
        }

        textboxFixpreis.Text = textboxFixpreis.Text.Replace(".", ",");
        if (!double.TryParse(textboxFixpreis.Text, out double result))
        {
            return false;
        }

        fixpreis = result;

        return true;
    }
    #endregion
}
