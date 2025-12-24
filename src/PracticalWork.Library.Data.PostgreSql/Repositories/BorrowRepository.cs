using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public class BorrowRepository : IBorrowRepository
{
    private readonly AppDbContext _appDbContext;

    public BorrowRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public Task<List<Guid>> GetReaderBorrowedBooks(Guid readerId)
    {
        return _appDbContext.BookBorrows.Where(x => x.ReaderId == readerId
            && x.Status == BookIssueStatus.Issued)
            .Select(x => x.BookId)
            .ToListAsync();
    }

    public Task<bool> IsReaderHasBorrowedBooks(Guid readerId)
    {
        return _appDbContext.BookBorrows.AnyAsync(
            x => x.ReaderId == readerId && 
                 x.Status == BookIssueStatus.Issued);
    }

    public async Task<Guid> BorrowBook(Guid readerId, Guid bookId)
    {
        BookBorrowEntity entity = new BookBorrowEntity()
        {
            ReaderId = readerId,
            BookId = bookId,
            Status = BookIssueStatus.Issued,
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateTime.UtcNow,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
        };
        
        _appDbContext.BookBorrows.Add(entity);
        await _appDbContext.SaveChangesAsync();
        
        return entity.Id;
    }

    public async Task ReturnBook(Guid bookId)
    {
        var borrowEntity = await GetBorrowForBook(bookId);
        
        if(borrowEntity == null)
            throw new ArgumentException("В базе нет записи о выдаче книги");
        
        borrowEntity.Status = 
            DateOnly.FromDateTime(DateTime.UtcNow) < borrowEntity.DueDate ? 
                BookIssueStatus.Returned 
            : BookIssueStatus.Overdue;
        
        borrowEntity.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        borrowEntity.UpdatedAt = DateTime.UtcNow;
        
        _appDbContext.BookBorrows.Update(borrowEntity);
        await _appDbContext.SaveChangesAsync();
    }
    
    private Task<BookBorrowEntity> GetBorrowEntity(Guid readerId, Guid bookId)
    {
        return _appDbContext.BookBorrows.FirstOrDefaultAsync(x => x.ReaderId == readerId && x.BookId == bookId);
    }
    
    private Task<BookBorrowEntity> GetBorrowForBook(Guid bookId)
    {
        return _appDbContext.BookBorrows.FirstOrDefaultAsync(x => x.BookId == bookId || x.Status == BookIssueStatus.Issued);
    }
}