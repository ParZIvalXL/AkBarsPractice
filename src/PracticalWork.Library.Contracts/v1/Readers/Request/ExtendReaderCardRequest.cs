using PracticalWork.Library.Contracts.v1.Abstracts;

namespace PracticalWork.Library.Contracts.v1.Readers.Request;

/// <summary>
/// Запрос на создание карточки читателя
/// </summary>
/// <param name="NewExpiryDate">Новый срок карточки</param>
public sealed record ExtendReaderCardRequest(DateOnly NewExpiryDate);