using PracticalWork.Library.Contracts.v1.Abstracts;

namespace PracticalWork.Library.Contracts.v1.Readers.Request;

/// <summary>
/// Запрос на создание карточки читателя
/// </summary>
/// <param name="FullName">ФИО</param>
/// <param name="PhoneNumber">Номер телефона</param>
/// <param name="ExpiryDate">Срок действия</param>
public sealed record CreateReaderRequest(string FullName, string PhoneNumber, DateTime ExpiryDate) : AbstractReader(FullName, PhoneNumber, ExpiryDate, true);