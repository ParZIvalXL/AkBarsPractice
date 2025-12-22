using JetBrains.Annotations;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBookRepository
{
    /// <summary>
    /// Создание книги
    /// </summary>
    /// <param name="book">Книга</param>
    /// <returns>Идентификатор книги</returns>
    Task<Guid> CreateBook(Book book);
    
    /// <summary>
    /// Обновить данные книги
    /// </summary>
    /// <param name="id">Идентификатор целевой книги</param>
    /// <param name="book">Новые данные</param>
    /// <returns></returns>
    Task UpdateBook(Guid id, Book book);
    
    /// <summary>
    /// Получить список книг
    /// </summary>
    /// <param name="booksPerPage">Количество книг в одной странице</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="category">Категория книги</param>
    /// <param name="author">Автор книги</param>
    /// <param name="status">Статус книги</param>
    /// <returns>Список книг</returns>
    Task<List<Book>> GetBooks(
        int? booksPerPage,
        int? page,
        BookCategory? category = null,
        [CanBeNull] string author = null,
        BookStatus? status = null);
    
    /// <summary>
    /// Получить книгу по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор книги</param>
    /// <returns>Книга</returns>
    Task<Book> GetBookById(Guid id);
}