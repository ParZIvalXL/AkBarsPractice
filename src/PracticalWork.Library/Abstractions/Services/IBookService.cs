using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IBookService
{
    /// <summary>
    /// Создание книги
    /// </summary>
    Task<Guid> CreateBook(Book book);
    
    /// <summary>
    /// Обновление книги
    /// </summary>
    Task UpdateBook(Guid id, Book book);
    
    /// <summary>
    /// Архивирование книги
    /// </summary>
    Task ArchiveBook(Guid id);
    
    /// <summary>
    /// Получение списка книг
    /// </summary>
    Task<List<Book>> GetBooks();
    
    Task AddDetails(Guid id, byte[] file);
}