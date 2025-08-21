using System.Text;

using Api;
using Api.Models;

using ExcelDataReader;

namespace Tests;

public class EnergyAlgoTests
{
    [Theory, MemberData(nameof(OptimalIntervalCases))]
    public void TestCalculateOptimalInterval(string file, int initialEnergy, int cap, int maxPower, Interval expected)
    {
        // Arrange
        EntryData entryData = new(
            InitialEnergy: initialEnergy,
            Capacity: cap,
            MaximumPower: maxPower,
            Lines: ReadExcel(file));

        // Act
        var actual = EnergyAlgo.CalculateOptimalInterval(entryData);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, int, int, int, Interval> OptimalIntervalCases = new()
    {
        { "data/18_a.xls", 12, 24, 6, new(Buy: new DateTime(2024, 9, 18, 14, 45, 0), Sell: new DateTime(2024, 9, 18, 9, 0, 0)) },
        { "data/10_a.xls", 12, 24, 6, new(Buy: new DateTime(2024, 9, 10, 00, 45, 0), Sell: new DateTime(2024, 9, 10, 21, 0, 0)) }
    };

    [Theory, MemberData(nameof(OneCycleCases))]
    public void TestCalculateOneCycle(string file, int initialEnergy, int cap, int maxPower, int intervalCount, Cycle expected)
    {
        // Arrange
        EntryData entryData = new(
            InitialEnergy: initialEnergy,
            Capacity: cap,
            MaximumPower: maxPower,
            Lines: ReadExcel(file));

        // Act
        var actual = EnergyAlgo.CalculateOneCycle(entryData, intervalCount);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, int, int, int, int, Cycle> OneCycleCases = new()
    {
        { "data/18_b.xls", 2, 24, 6, 4, new(Buy: [
            new(2024, 9, 18, 15, 15, 0),
            new(2024, 9, 18, 15, 30, 0),
            new(2024, 9, 18, 15, 45, 0),
            new(2024, 9, 18, 16, 00, 0)
        ], Sell: [
            new(2024, 9, 18, 20, 0, 0),
            new(2024, 9, 18, 20, 15, 0),
            new(2024, 9, 18, 20, 30, 0),
            new(2024, 9, 18, 20, 45, 0)
        ]) },
        { "data/10_b.xls", 2, 24, 6, 4, new(Buy: [
            new(2024, 9, 10, 4, 15, 0),
            new(2024, 9, 10, 4, 30, 0),
            new(2024, 9, 10, 4, 45, 0),
            new(2024, 9, 10, 5, 0, 0)
        ], Sell: [
            new(2024, 9, 10, 20, 30, 0),
            new(2024, 9, 10, 20, 45, 0),
            new(2024, 9, 10, 21, 0, 0),
            new(2024, 9, 10, 21, 15, 0)
        ]) }
    };

    public static List<Line> ReadExcel(string filePath)
    {
        Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var entries = new List<Line>();
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
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

        return entries;
    }

}
