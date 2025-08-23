namespace Api.Models;

public record EntryData(int InitialEnergy, int Capacity, int MaximumPower, int Intervals, List<EnergyData> Lines);
