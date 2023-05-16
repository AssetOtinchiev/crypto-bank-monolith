namespace Contracts.Dtos.Responses;

public class ErrorResponse
{
    public List<string> Errors { get; } = new List<string>();

    public string GetErrorsString() => string.Join(" ", this.Errors);
}