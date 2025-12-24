using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Contracts.v1.Abstracts;

namespace PracticalWork.Library.Contracts.v1.Library;

/// <summary>
/// Запрос на создание карточки читателя
/// </summary>
/// <param name="BookId">Книга</param>
public sealed record ReturnBookRequest([FromForm] Guid BookId);