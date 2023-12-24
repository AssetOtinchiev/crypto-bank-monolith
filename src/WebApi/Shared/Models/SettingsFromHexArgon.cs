namespace WebApi.Shared.Models;

public class SettingsFromHexArgon
{
    public string Salt { get; set; }
    public string Hash { get; set; }
    public int DegreeOfParallelism { get; set; }
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
}