namespace PracticalWork.Library.Contracts.v1.Abstracts;

/// <summary>
/// Абстрактная карточка читателя
/// </summary>
/// <param name="FullName">ФИО читателя</param>
/// <param name="PhoneNumber">Номер телефона</param>
/// <param name="ExpiryDate">Срок действия</param>
/// <param name="IsActive">Активность карточки</param>
public abstract record AbstractReader(string FullName, string PhoneNumber, DateTime ExpiryDate, bool IsActive);