using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
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

    public bool IsReaderHasBorrowedBooks(Guid readerId)
    {
        return _appDbContext.BookBorrows.Any(
            x => x.ReaderId == readerId && 
                 x.Status == BookIssueStatus.Issued);
    }
}