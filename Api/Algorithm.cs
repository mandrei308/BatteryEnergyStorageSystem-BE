using System.Transactions;

using Api.Models;

namespace Api;

public static class Algorithm
{
    public static TransactionResult CalculateOptimalInterval(EntryData data)
    {
        var intervalPower = data.MaximumPower / 4.0;

        if (data.InitialEnergy < intervalPower)
        {
            return CalculateBuyFirst(data.Lines);
        }

        if (data.InitialEnergy > data.Capacity - intervalPower)
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
            Sell: lines[sellIdx].Timestamp,
            Profit: maxProfit);
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
            Sell: lines[sellIdx].Timestamp,
            Profit: maxProfit);
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
            Sell: lines[maxIdx].Timestamp,
            Profit: maxPrice - minPrice);
    }

    public static TransactionResult CalculateOneCycle(EntryData data)
    {
        double intervalPower = data.MaximumPower / 4.0;

        var maxProfit = 0.0;
        var bestCycle = new TransactionResult(DateTime.MinValue, DateTime.MinValue, 0);

        var windowsCount = data.Lines.Count - data.Intervals + 1;

        for (int i = 0; i < windowsCount; i++)
        {
            for (int j = 0; j < windowsCount; j++)
            {
                if (i == j || Math.Abs(i - j) < data.Intervals) continue;

                double currentEnergy = data.InitialEnergy;
                var profit = 0.0;
                var isValidCycle = true;

                for (int k = 0; k < data.Lines.Count; k++)
                {
                    var isBuyWindow = k >= i && k < i + data.Intervals;
                    var isSellWindow = k >= j && k < j + data.Intervals;

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
                        Sell: data.Lines[j].Timestamp,
                        Profit: maxProfit);
                }
            }
        }

        return bestCycle;
    }

    public static (TransactionResult, TransactionResult) CalculateTwoCycles(EntryData data)
    {
        double intervalPower = data.MaximumPower / 4.0;

        var bestTotalProfit = 0.0;
        var bestFirstCycle = new TransactionResult(DateTime.MinValue, DateTime.MinValue, 0);
        var bestSecondCycle = new TransactionResult(DateTime.MinValue, DateTime.MinValue, 0);

        for (int split = data.Intervals * 2; split < data.Lines.Count - data.Intervals * 2; split++)
        {
            var maxFirstProfit = 0.0;
            var bestFirst = new TransactionResult(DateTime.MinValue, DateTime.MinValue, 0);

            for (int i = 0; i < split - data.Intervals; i++)
            {
                for (int j = 0; j < split - data.Intervals; j++)
                {
                    if (i == j || Math.Abs(i - j) < data.Intervals) continue;

                    double currentEnergy = data.InitialEnergy;
                    var profit = 0.0;
                    var isValidCycle = true;

                    for (int k = 0; k < split; k++)
                    {
                        bool isBuyWindow = k >= i && k < i + data.Intervals;
                        bool isSellWindow = k >= j && k < j + data.Intervals;

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

                    if (isValidCycle && profit > maxFirstProfit)
                    {
                        maxFirstProfit = profit;
                        bestFirst = new TransactionResult(
                            Buy: data.Lines[i].Timestamp,
                            Sell: data.Lines[j].Timestamp,
                            Profit: maxFirstProfit);
                    }
                }
            }

            var maxSecondProfit = 0.0;
            var bestSecond = new TransactionResult(DateTime.MinValue, DateTime.MinValue, 0);

            for (int i = split; i < data.Lines.Count - data.Intervals; i++)
            {
                for (int j = split; j < data.Lines.Count - data.Intervals; j++)
                {
                    if (i == j || Math.Abs(i - j) < data.Intervals) continue;

                    double currentEnergy = data.InitialEnergy;
                    var profit = 0.0;
                    var isValidCycle = true;

                    for (int k = split; k < data.Lines.Count; k++)
                    {
                        bool isBuyWindow = k >= i && k < i + data.Intervals;
                        bool isSellWindow = k >= j && k < j + data.Intervals;

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

                    if (isValidCycle && profit > maxSecondProfit)
                    {
                        maxSecondProfit = profit;
                        bestSecond = new TransactionResult(
                            Buy: data.Lines[i].Timestamp,
                            Sell: data.Lines[j].Timestamp,
                            Profit: maxSecondProfit);
                    }
                }
            }

            var totalProfit = maxFirstProfit + maxSecondProfit;
            if (totalProfit > bestTotalProfit)
            {
                bestTotalProfit = totalProfit;
                bestFirstCycle = bestFirst;
                bestSecondCycle = bestSecond;
            }
        }

        return (bestFirstCycle, bestSecondCycle);
    }
}
