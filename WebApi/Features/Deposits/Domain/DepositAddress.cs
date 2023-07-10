using WebApi.Features.Users.Domain;

namespace WebApi.Features.Deposits.Domain;

public class DepositAddress
{
    public int Id { get; set; } // Автогенерируемый идентификатор
    public string CurrencyCode { get; set; } // Код валюты. Сейчас всегда будет BTC
    public Guid UserId { get; set; }  // Идентификатор пользователя, которому принадлежит адрес
    public User User { get; set; }
    public int XpubId { get; set; }  // Идентификатор xpub, использованного для генерации адреса
    public Xpub Xpub { get; set; } 
    public int DerivationIndex { get; set; }  // Индекс, использованный для генерации адреса
    public string CryptoAddress { get; set; }  // Непосредственно адрес
}