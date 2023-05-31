namespace WebApi.Features.Auth.Options;

public class ArgonSecurityOptions
{
    public const string ArgonSecuritySectionName = "Argon2IdParameters";
    public int DegreeOfParallelism { get; set; }
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
}