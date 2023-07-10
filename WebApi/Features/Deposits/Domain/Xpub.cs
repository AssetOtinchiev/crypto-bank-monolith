namespace WebApi.Features.Deposits.Domain;

public class Xpub
{
    public int Id { get; set; } // Автогенерируемый идентификатор
    public string CurrencyCode { get; set; } // Код валюты. Сейчас всегда будет BTC
    public string Value { get; set; }  // Значение ключа. Например "xpub67xpozcx8pe95XVuZLHXZeG6XWXHpGq6Qv5cmNfi7cS5mtjJ2tgypeQbBs2UAR6KECeeMVKZBPLrtJunSDMstweyLXhRgPxdp14sk9tJPW9"
}