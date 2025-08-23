using System.Text;

using Api.Models;

using ExcelDataReader;

namespace Api.Utils;

public static class ExcelUtils
{
    public static List<EnergyData> Read(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var entries = new List<EnergyData>();

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet();
        var table = result.Tables[0];
        for (int i = 1; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var entry = new EnergyData(
                Timestamp: DateTime.Parse(row[0].ToString()!),
                Price: double.Parse(row[1].ToString()!)
            );
            entries.Add(entry);
        }

        return entries;
    }

    public static List<EnergyData> Read(string filePath)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        return Read(stream);
    }
}
