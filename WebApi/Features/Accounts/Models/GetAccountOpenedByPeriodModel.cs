namespace WebApi.Features.Accounts.Models;

public class GetAccountOpenedByPeriodModel
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}