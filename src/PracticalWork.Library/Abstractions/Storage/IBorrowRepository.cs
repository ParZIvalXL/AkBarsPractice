using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBorrowRepository
{
    Task<List<Guid>> GetReaderBorrowedBooks(Guid readerId);
}