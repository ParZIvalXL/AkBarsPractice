using PracticalWork.Library.Contracts.v1.Abstracts;

namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на обновление книги
/// <param name="Title">Новое название книги</param>
/// <param name="Authors">Новые авторы</param>
/// <param name="Description">Описание книги, не используется</param>
/// <param name="Year">Новый год издания</param>
/// </summary>
public sealed record UpdateBookRequest(string Title, IReadOnlyList<string> Authors, string Description, int Year)
    : AbstractBook(Title, Authors, Description, Year);