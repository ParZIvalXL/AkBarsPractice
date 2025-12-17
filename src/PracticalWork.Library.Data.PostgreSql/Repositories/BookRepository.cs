using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Data.PostgreSql.Extenssions;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly AppDbContext _appDbContext;

    public BookRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        AbstractBookEntity entity = book.Category switch
        {
            BookCategory.ScientificBook => new ScientificBookEntity(),
            BookCategory.EducationalBook => new EducationalBookEntity(),
            BookCategory.FictionBook => new FictionBookEntity(),
            _ => throw new ArgumentException($"Неподдерживаемая категория книги: {book.Category}", nameof(book.Category))
        };

        entity.Title = book.Title;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Authors = book.Authors;
        entity.Status = book.Status;

        _appDbContext.Add(entity);
        await _appDbContext.SaveChangesAsync();

        return entity.Id;
    }

    public Task UpdateBook(Guid id, Book book)
    {
        AbstractBookEntity entity = _appDbContext.Books.FirstOrDefault(f => f.Id == id);
        
        if (entity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена", nameof(id));
        
        entity.Title = book.Title;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Authors = book.Authors;
        entity.Status = book.Status;

        _appDbContext.Update(entity);
        return _appDbContext.SaveChangesAsync();
    }

    public async Task<List<Book>> GetBooks(int booksPerPage, int page)
    {
        return await _appDbContext.Books
            .Skip((page - 1) * booksPerPage)
            .Take(booksPerPage)
            .Select(s => s.ToBook())
            .ToListAsync();
    }
    
    private static BookCategory GetBookCategory(AbstractBookEntity entity)
    {
        return entity switch
        {
            ScientificBookEntity => BookCategory.ScientificBook,
            EducationalBookEntity => BookCategory.EducationalBook,
            FictionBookEntity => BookCategory.FictionBook,
            _ => throw new InvalidOperationException($"Неизвестный тип entity: {entity.GetType()}")
        };
    }
    
    public Task<Book> GetBookById(Guid id)
    {
        AbstractBookEntity entity = _appDbContext.Books.FirstOrDefault(f => f.Id == id);
        
        if (entity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена", nameof(id));
        
        return Task.FromResult(entity.ToBook());
    }  
}