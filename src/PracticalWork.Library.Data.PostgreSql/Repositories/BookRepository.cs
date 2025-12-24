using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Data.PostgreSql.Extensions;
using PracticalWork.Library.Enums;
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
        entity.CoverImagePath = book.CoverImagePath;

        _appDbContext.Update(entity);
        return _appDbContext.SaveChangesAsync();
    }

    public async Task<List<Book>> GetBooks(
            int? booksPerPage,
            int? page,
            BookCategory? category,
            string author = null,
            BookStatus? status = null,
            bool archivedFilter = false)
    {
        page ??= 1;
        booksPerPage ??= 20;
        
        var query = _appDbContext.Books
            .Include(b => b.IssuanceRecords)
            .AsQueryable();
                
        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }
        
        if (archivedFilter)
        {
            query = query.Where(b => b.Status != BookStatus.Archived);
        }
                
        if (!string.IsNullOrWhiteSpace(author))
        {
            var authorLower = author.Trim().ToLower();
            query = query.Where(b => b.Authors.Any(a => a.ToLower().Contains(authorLower)));
        }
                
        if (category is not null)
        {
            query = FilterByCategory(query, category);
        }
            
        var result = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip(((int)page - 1) * (int)booksPerPage)
            .Take((int)booksPerPage)
            .Select(b => b.ToBook())
            .ToListAsync();
                    
        return result;
    }

        private IQueryable<AbstractBookEntity> FilterByCategory(
            IQueryable<AbstractBookEntity> query, 
            BookCategory? category)
        {
            return category switch
            {
                BookCategory.ScientificBook => query.OfType<ScientificBookEntity>(),
                BookCategory.EducationalBook => query.OfType<EducationalBookEntity>(),
                BookCategory.FictionBook => query.OfType<FictionBookEntity>(),
                _ => query.OfType<AbstractBookEntity>()
            };
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

    public Task<BookDetails> GetBookDetails(Guid id)
    {
        AbstractBookEntity entity = _appDbContext.Books.FirstOrDefault(f => f.Id == id);
        
        if (entity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена", nameof(id));
        
        return Task.FromResult(entity.ToBookDetails());
    }

    public async Task<Guid> GetBookIdByTitle(string title)
    {
        var book = await _appDbContext.Books
            .AsNoTracking() // Добавляем для оптимизации, если не нужно отслеживание
            .FirstOrDefaultAsync(f => f.Title == title);
    
        if (book == null)
            throw new ArgumentException($"Книга с названием {title} не найдена", nameof(title));
    
        return book.Id;
    }

    public Task<bool> BookTitleExists(string title)
    {
        return _appDbContext.Books.AnyAsync(x => x.Title == title);
    }
}