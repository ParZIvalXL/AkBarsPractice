using JetBrains.Annotations;
using PracticalWork.Library.Contracts.v1.Enums;

namespace PracticalWork.Library.Contracts.v1.Library.Request;

/// <summary>
/// Запрос на получение книг библиотеки
/// </summary>
/// <param name="BookCategory">Категория книги</param>
/// <param name="Author">Автор книги</param>
/// <param name="IsAvailable">Доступность книги</param>
/// <param name="BooksPerPage">Количество книг на странице</param>
/// <param name="Page">Номер страницы</param>
public sealed record GetBooksRequest(BookCategory? BookCategory, [CanBeNull] string Author, bool? IsAvailable, int? BooksPerPage, int? Page);