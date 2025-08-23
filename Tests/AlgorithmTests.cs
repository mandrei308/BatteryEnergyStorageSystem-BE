using System.Text;

using Api;
using Api.Models;
using Api.Utils;

using ExcelDataReader;

namespace Tests;

public class AlgorithmTests
{
    [Theory, MemberData(nameof(OptimalIntervalCases))]
    public void TestCalculateOptimalInterval(string file, int initialEnergy, int cap, int maxPower,
        TransactionResult expected)
    {
        // Arrange
        EntryData entryData = new(
            InitialEnergy: initialEnergy,
            Capacity: cap,
            MaximumPower: maxPower,
            Intervals: 1,
            Lines: ExcelUtils.Read(file));

        // Act
        var actual = Algorithm.CalculateOneCycle(entryData);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, int, int, int, TransactionResult> OptimalIntervalCases = new()
    {
        {
            "data/18_a.xls", 12, 24, 6,
            new TransactionResult(Buy: new DateTime(2024, 9, 18, 14, 45, 0), Sell: new DateTime(2024, 9, 18, 9, 0, 0))
        },
        {
            "data/10_a.xls", 12, 24, 6,
            new TransactionResult(Buy: new DateTime(2024, 9, 10, 00, 45, 0), Sell: new DateTime(2024, 9, 10, 21, 0, 0))
        }
    };

    [Theory, MemberData(nameof(OneCycleCases))]
    public void TestCalculateOneCycle(string file, int initialEnergy, int cap, int maxPower, int intervalCount,
        TransactionResult expected)
    {
        // Arrange
        EntryData entryData = new(
            InitialEnergy: initialEnergy,
            Capacity: cap,
            MaximumPower: maxPower,
            Intervals: intervalCount,
            Lines: ExcelUtils.Read(file));

        // Act
        var actual = Algorithm.CalculateOneCycle(entryData);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, int, int, int, int, TransactionResult> OneCycleCases = new()
    {
        {
            "data/18_b.xls", 10, 24, 6, 4,
            new TransactionResult(Buy: new DateTime(2024, 9, 18, 15, 15, 0), Sell: new DateTime(2024, 9, 18, 20, 0, 0))
        },
        {
            "data/10_b.xls", 12, 24, 6, 4,
            new TransactionResult(Buy: new DateTime(2024, 9, 10, 4, 15, 0), Sell: new DateTime(2024, 9, 10, 20, 30, 0))
        }
    };
}
