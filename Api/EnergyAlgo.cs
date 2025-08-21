using Api.Models;

namespace Api;

public class EnergyAlgo
{
    public static Interval CalculateOptimalInterval(EntryData data)
    {
        double intervalPower = data.MaximumPower / 4.0;

        if (data.InitialEnergy == 0)
        {
            return CalculateWithZeroCap(data.Lines);
        }
        else if (data.InitialEnergy == data.Capacity)
        {
            return CalculateWithMaxCap(data.Lines);
        }

        return CalculateMinMax(data.Lines);
    }

    private static Interval CalculateWithZeroCap(List<Line> lines)
    {
        var min = double.MaxValue;
        var max = double.MinValue;
        var minIdx = 0;
        var maxIdx = 0;

        for (int i = 0; i < lines.Count - 1; i++)
        {
            if (lines[i].Price < min)
            {
                min = lines[i].Price;
                minIdx = i;
            }
        }
        for (int i = minIdx; i < lines.Count; i++)
        {
            if (lines[i].Price > max)
            {
                max = lines[i].Price;
                maxIdx = i;
            }
        }

        return new Interval(
            Buy: lines[minIdx].Timestamp,
            Sell: lines[maxIdx].Timestamp);
    }

    private static Interval CalculateWithMaxCap(List<Line> lines)
    {
        var maxPrice = double.MinValue;
        var minPrice = double.MaxValue;
        var maxIdx = 0;
        var minIdx = 0;

        for (int i = 0; i < lines.Count - 1; i++)
        {
            if (maxPrice < lines[i].Price)
            {
                maxPrice = lines[i].Price;
                maxIdx = i;
            }
        }
        for (int i = maxIdx; i >= 0; i--)
        {
            if (lines[i].Price < minPrice)
            {
                minPrice = lines[i].Price;
                minIdx = i;
            }
        }

        return new Interval(
            Buy: lines[minIdx].Timestamp,
            Sell: lines[maxIdx].Timestamp);
    }

    private static Interval CalculateMinMax(List<Line> lines)
    {
        var maxPrice = double.MinValue;
        var minPrice = double.MaxValue;
        var maxIdx = 0;
        var minIdx = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Price > maxPrice)
            {
                maxPrice = lines[i].Price;
                maxIdx = i;
            }
            if (lines[i].Price < minPrice)
            {
                minPrice = lines[i].Price;
                minIdx = i;
            }
        }

        return new Interval(
            Buy: lines[minIdx].Timestamp,
            Sell: lines[maxIdx].Timestamp);
    }

    public static Cycle CalculateOneCycle(EntryData data, int intervalCount)
    {
        // edge case energy < energyToBeTraded | ...
        double intervalPower = data.MaximumPower / 4.0;
        double energyToBeTraded = intervalPower * intervalCount;

        if (data.InitialEnergy < energyToBeTraded)
        {
            return CalculateWithNotEnoughEnergy(data.Lines, intervalCount);
        }

        return null!;
    }

    private static Cycle CalculateWithNotEnoughEnergy(List<Line> lines, int intervalCount)
    {
        double sum = lines[..intervalCount].Sum(line => line.Price);
        double buy = sum;
        double profit = 0;

        int buyStartIndex = 0;
        int sellStartIndex = 0;



        for (int i = intervalCount; i < lines.Count; i++)
        {
            sum -= lines[i - intervalCount].Price;
            sum += lines[i].Price;

            if (sum < buy)
            {
                buy = sum;
                buyStartIndex = i - intervalCount;
            }
            else if (sum - buy > profit)
            {
                profit = sum - buy;
                sellStartIndex = i;
            }
        }

        return new Cycle(
            Sell: [.. lines.Skip(sellStartIndex).Take(intervalCount).Select(line => line.Timestamp)],
            Buy: [.. lines.Skip(buyStartIndex).Take(intervalCount).Select(line => line.Timestamp)]);
    }
}
