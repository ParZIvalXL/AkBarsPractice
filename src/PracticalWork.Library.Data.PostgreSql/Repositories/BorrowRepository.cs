using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Models;

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
        return _appDbContext.BookBorrows.Where(x => x.ReaderId == readerId)
            .Select(x => x.BookId)
            .ToListAsync();
    }
}