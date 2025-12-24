using JetBrains.Annotations;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface ILibraryService
{
    /// <summary>
    /// Выдать книгу читателю
    /// </summary>
    /// <param name="readerId">Карточка читателя</param>
    /// <param name="bookId">Книга</param>
    public Task<Guid> BorrowBook(Guid readerId, Guid bookId);
    
    /// <summary>
    /// Вернуть книгу в библиотеку
    /// </summary>
    /// <param name="bookId">Книга</param>
    public Task ReturnBook(Guid bookId);

    /// <summary>
    /// Получить доступные книги библиотеки
    /// </summary>
    /// <param name="bookCategory">Категория книги</param>
    /// <param name="author">Автор</param>
    /// <param name="isAvailable">Доступность книги</param>
    /// <param name="booksPerPage">Книг в одной странице</param>
    /// <param name="page">Страница</param>
    public Task<List<Book>> GetLibraryBooks(int? bookCategory, [CanBeNull] string author, bool? isAvailable,
        int? booksPerPage, int? page);
    
    /// <summary>
    /// Получить книгу по идентификатору или названию
    /// </summary>
    /// <param name="idOrTitle">Идентификатор или название книги</param>
    public Task<BookDetails> GetBookDetails(string idOrTitle);
}