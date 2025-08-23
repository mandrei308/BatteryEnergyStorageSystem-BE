using Api.Models;

namespace Api;

public static class Algorithm
{
    public static TransactionResult CalculateOptimalInterval(EntryData data)
    {
        if (data.InitialEnergy == 0)
        {
            return CalculateBuyFirst(data.Lines);
        }

        if (data.InitialEnergy == data.Capacity)
        {
            return CalculateSellFirst(data.Lines);
        }

        return CalculateMinMax(data.Lines);
    }

    private static TransactionResult CalculateBuyFirst(List<EnergyData> lines, int cycleOffset = 0)
    {
        double minPrice = lines[0].Price;
        double maxProfit = 0;

        int minIdx = 0;
        int buyIdx = -1;
        int sellIdx = -1;

        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Price < minPrice)
            {
                minPrice = lines[i].Price;
                minIdx = i;
            }
            else if (i - minIdx >= cycleOffset)
            {
                double profit = lines[i].Price - minPrice;
                if (profit > maxProfit)
                {
                    maxProfit = profit;
                    buyIdx = minIdx;
                    sellIdx = i;
                }
            }
        }

        return new TransactionResult(
            Buy: lines[buyIdx].Timestamp,
            Sell: lines[sellIdx].Timestamp);
    }

    private static TransactionResult CalculateSellFirst(List<EnergyData> lines, int cycleOffset = 0)
    {
        double maxPrice = lines[0].Price;
        double maxProfit = 0;

        int maxIdx = 0;
        int buyIdx = -1;
        int sellIdx = -1;

        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Price > maxPrice)
            {
                maxPrice = lines[i].Price;
                maxIdx = i;
            }
            else if (i - maxIdx >= cycleOffset)
            {
                double profit = maxPrice - lines[i].Price;
                if (profit > maxProfit)
                {
                    maxProfit = profit;
                    sellIdx = maxIdx;
                    buyIdx = i;
                }
            }
        }

        return new TransactionResult(
            Buy: lines[buyIdx].Timestamp,
            Sell: lines[sellIdx].Timestamp);
    }

    private static TransactionResult CalculateMinMax(List<EnergyData> lines, int cycleOffset = 0)
    {
        var maxPrice = double.MinValue;
        var minPrice = double.MaxValue;
        var maxIdx = 0;
        var minIdx = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Price > maxPrice && i - minIdx >= cycleOffset)
            {
                maxPrice = lines[i].Price;
                maxIdx = i;
            }

            if (lines[i].Price < minPrice && i - maxIdx >= cycleOffset)
            {
                minPrice = lines[i].Price;
                minIdx = i;
            }
        }

        return new TransactionResult(
            Buy: lines[minIdx].Timestamp,
            Sell: lines[maxIdx].Timestamp);
    }

    public static TransactionResult CalculateOneCycle(EntryData data, int intervalCount)
    {
        // edge case energy < energyToBeTraded | ...
        double intervalPower = data.MaximumPower / 4.0;
        double energyToBeTraded = intervalPower * intervalCount;
        // var pre = PrecomputePrices(data.Lines, intervalCount).ToList();

        // if (data.InitialEnergy < energyToBeTraded)
        // {
        //     return CalculateBuyFirst(pre, intervalCount);
        // }

        // if (data.InitialEnergy >= data.Capacity)
        // {
        //     return CalculateSellFirst(pre, intervalCount);
        // }

        var maxProfit = 0.0;
        var bestCycle = new TransactionResult(DateTime.MinValue, DateTime.MinValue);

        var windowsCount = data.Lines.Count - intervalCount + 1;

        for (int i = 0; i < windowsCount; i++)
        {
            for (int j = 0; j < windowsCount; j++)
            {
                if (i == j || Math.Abs(i - j) < intervalCount) continue;

                double currentEnergy = data.InitialEnergy;
                var profit = 0.0;
                var isValidCycle = true;

                for (int k = 0; k < data.Lines.Count; k++)
                {
                    var isBuyWindow = k >= i && k < i + intervalCount;
                    var isSellWindow = k >= j && k < j + intervalCount;

                    if (isBuyWindow)
                    {
                        if (currentEnergy + intervalPower > data.Capacity)
                        {
                            isValidCycle = false;
                            break;
                        }

                        currentEnergy += intervalPower;
                        profit -= data.Lines[k].Price * intervalPower;
                    }
                    else if (isSellWindow)
                    {
                        if (currentEnergy - intervalPower < 0)
                        {
                            isValidCycle = false;
                            break;
                        }

                        currentEnergy -= intervalPower;
                        profit += data.Lines[k].Price * intervalPower;
                    }
                }

                if (isValidCycle && profit > maxProfit)
                {
                    maxProfit = profit;
                    bestCycle = new TransactionResult(
                        Buy: data.Lines[i].Timestamp,
                        Sell: data.Lines[j].Timestamp);
                }
            }
        }

        return bestCycle;

        // return CalculateMinMax(pre);
    }

    private static EnergyData[] PrecomputePrices(List<EnergyData> lines, int len)
    {
        EnergyData[] sum = new EnergyData[lines.Count - len + 1];

        double initSum = 0;
        for (int i = 0; i < len && i < lines.Count; i++)
        {
            initSum += lines[i].Price;
        }

        sum[0] = new EnergyData(lines[0].Timestamp, initSum);
        for (int i = 1; i <= lines.Count - len; i++)
        {
            sum[i] = new EnergyData(lines[i].Timestamp,
                sum[i - 1].Price - lines[i - 1].Price + lines[i + len - 1].Price);
        }

        return sum;
    }
}
