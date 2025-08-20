namespace Api.Models;

public record EntryData(int InitialEnergy, int Capacity, int MaximumPower, List<Line> Lines);
