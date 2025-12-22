using JetBrains.Annotations;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IBookService
{
    /// <summary>
    /// Создание книги
    /// </summary>
    /// <param name="book">Книга</param>
    /// <returns>Идентификатор книги</returns>
    Task<Guid> CreateBook(Book book);
    
    /// <summary>
    /// Обновление книги
    /// </summary>
    /// <param name="id">Идентификатор книги</param>
    /// <param name="book">Книга</param>
    Task UpdateBook(Guid id, Book book);
    
    /// <summary>
    /// Архивирование книги
    /// </summary>
    /// <param name="id">Идентификатор книги</param>
    Task ArchiveBook(Guid id);

    /// <summary>
    /// Получение списка книг
    /// </summary>
    /// <param name="booksPerPage">Количество книг на странице</param>
    /// <param name="page">Страница</param>
    /// <param name="category">Категория</param>
    /// <param name="author">Автор</param>
    /// <param name="status">Статус</param>
    /// <returns>Список книг</returns>
    Task<List<Book>> GetBooks(int? booksPerPage = 20, int? page = 1, BookCategory? category = null, [CanBeNull] string author = null, BookStatus? status = null);
    
    Task AddDetails(Guid id, string description,byte[] file);
}