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
    /// <param name="archivedFilter">Фильтр по архиву</param>
    /// <returns>Список книг</returns>
    Task<List<Book>> GetBooks(
        int? booksPerPage,
        int? page,
        BookCategory? category = null,
        [CanBeNull] string author = null,
        BookStatus? status = null,
        bool archivedFilter = false);
    
    /// <summary>
    /// Получить книгу по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор книги</param>
    /// <returns>Книга</returns>
    Task<Book> GetBookById(Guid id);

    /// <summary>
    /// Получить подробные детали о книге по ее ID
    /// </summary>
    /// <param name="id">ID книги</param>
    /// <returns>Детали книги</returns>
    Task<BookDetails> GetBookDetails(Guid id);
    
    /// <summary>
    /// Получить id книги по ее названию
    /// </summary>
    /// <param name="title">Название книги</param>
    /// <returns>Id книги</returns>
    Task<Guid> GetBookIdByTitle(string title);

    /// <summary>
    /// Проверяет существует ли книга с таким названием
    /// </summary>
    /// <param name="title">Название книги</param>
    Task<bool> BookTitleExists(string title);
}