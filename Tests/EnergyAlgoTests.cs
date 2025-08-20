using System.Globalization;

using Api;
using Api.Models;

using NPOI.HSSF.UserModel;

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
            InitialEnergy: 10,
            Capacity: 24,
            MaximumPower: 6,
            Lines: ReadXls("data/18_a.csv")
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

    public static List<Line> ReadXls(string filePath)
    {
        var entries = new List<Line>();
        var culture = new CultureInfo("en-US");
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            var workbook = new HSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);
            for (int i = 1; i <= sheet.LastRowNum; i++) // skip header row
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var entry = new Line
                (
                    Timestamp: DateTime.Parse(row.GetCell(0).ToString()!, culture),
                    Price: double.Parse(row.GetCell(1).ToString()!, NumberStyles.Any, culture)
                );
                entries.Add(entry);
            }
        }
        return entries;
    }

}
