namespace Api.Models;

public record Response(TransactionResult? FirstCycle, TransactionResult? SecondCycle, List<EnergyData> Lines);
