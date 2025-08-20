using Api.Models;

namespace Api;

public class EnergyAlgo
{
    public static Interval CalculateOptimalInterval(EntryData data)
    {
        double intervalPower = data.MaximumPower / 4.0;

        if (data.InitialEnergy == 0)
        {
            double minPrice = data.Lines.Min(x => x.Price);
            double maxPrice = int.MinValue;
            int indexMin = data.Lines.IndexOf(data.Lines.First(x => x.Price == minPrice));

            for (int i = indexMin; i < data.Lines.Count; i++)
            {
                if (data.Lines[i].Price > maxPrice) maxPrice = data.Lines[i].Price;
            }

            int indexMax = data.Lines.IndexOf(data.Lines.First(x => x.Price == maxPrice));

            return new Interval(
                Sell: data.Lines[indexMax].Timestamp,
                Buy: data.Lines[indexMin].Timestamp
            );

        }
        else if (data.InitialEnergy == data.Capacity)
        {
            double maxPrice = data.Lines.Max(x => x.Price);
            double minPrice = int.MaxValue;
            int indexMax = data.Lines.IndexOf(data.Lines.First(x => x.Price == maxPrice));

            for (int i = indexMax; i >= 0; i--)
            {
                if (data.Lines[i].Price < minPrice) minPrice = data.Lines[i].Price;
            }

            int indexMin = data.Lines.IndexOf(data.Lines.First(x => x.Price == minPrice));

            return new Interval(
                Sell: data.Lines[indexMax].Timestamp,
                Buy: data.Lines[indexMin].Timestamp
            );
        }
        else
        {
            return new Interval(
                Sell: data.Lines[data.Lines.IndexOf(data.Lines.First(x => x.Price == data.Lines.Max(y => y.Price)))].Timestamp,
                Buy: data.Lines[data.Lines.IndexOf(data.Lines.First(x => x.Price == data.Lines.Min(y => y.Price)))].Timestamp
            );
        }
    }
}

