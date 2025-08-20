using System.Globalization;

using Api;
using Api.Models;

using ExcelDataReader;

namespace Tests;

public class EnergyAlgoTests
{

    [Fact]
    public void TestReadCsv()
    {
        var result = ReadCsv("data/lines.csv");
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public void TestCalculateOptimalInterval()
    {
        EntryData entryData = new
        (
            InitialEnergy: 12,
            Capacity: 24,
            MaximumPower: 6,
            Lines: ReadExcel("data/10_a.xls")
        );

        var sellTimestamp = new DateTime(2024, 9, 18, 9, 0, 0);
        var buyTimestamp = new DateTime(2024, 9, 18, 14, 45, 0);

        var result = EnergyAlgo.CalculateOptimalInterval(entryData);
        Assert.NotNull(result);
        Assert.True(result.Sell == sellTimestamp);
        Assert.True(result.Buy == buyTimestamp);
    }

    public static List<Line> ReadCsv(string filePath)
    {
        var entries = new List<Line>();
        var culture = new CultureInfo("en-US");
        using (var reader = new StreamReader(filePath))
        {
            string header = reader.ReadLine()!;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()!;
                var fields = line.Split(',');

                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = fields[i].Trim('\"');
                }

                var entry = new Line
                (
                    Timestamp: DateTime.Parse(fields[0], culture),
                    Price: double.Parse(fields[1], NumberStyles.Any, culture)
                );
                entries.Add(entry);
            }
        }
        return entries;
    }

    public static List<Line> ReadExcel(string filePath)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var entries = new List<Line>();
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            var result = reader.AsDataSet();
            var table = result.Tables[0];
            for (int i = 1; i < table.Rows.Count; i++) // skip header
            {
                var row = table.Rows[i];
                var entry = new Line(
                    Timestamp: DateTime.Parse(row[0].ToString()!),
                    Price: double.Parse(row[1].ToString()!)
                );
                entries.Add(entry);
            }
        }
        return entries;
    }

}
