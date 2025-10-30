using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBookRepository
{
    Task<Guid> CreateBook(Book book);
    Task UpdateBook(Guid id, Book book);
    Task<List<Book>> GetBooks();
    Task ArchiveBook(Guid id);
    Task<Book> GetBookById(Guid id);
}