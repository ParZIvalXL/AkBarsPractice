using PracticalWork.Library.Contracts.v1.Abstracts;

namespace PracticalWork.Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ со всеми книгами в базе
/// </summary>
/// <param name="Books">Список книг</param>
public sealed record GetBooksResponse(List<AbstractBook> Books);