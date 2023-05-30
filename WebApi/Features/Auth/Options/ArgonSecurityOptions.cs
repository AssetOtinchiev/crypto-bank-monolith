namespace WebApi.Features.Auth.Options;

public class ArgonSecurityOptions
{
    public const string ArgonSecuritySectionName = "Argon2IdParameters";
    public static Argon2IdParametersOptions Argon2IdParameters { get; set; } = new();
}

public class Argon2IdParametersOptions
{
    public int DegreeOfParallelism { get; set; }
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
}