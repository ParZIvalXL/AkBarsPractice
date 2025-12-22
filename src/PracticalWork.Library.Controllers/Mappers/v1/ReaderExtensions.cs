using PracticalWork.Library.Contracts.v1.Readers.Request;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Controllers.Mappers.v1;

public static class ReaderExtensions
{
    public static Reader ToReader(this CreateReaderRequest readerEntity)
    {
        return new Reader
        {
            FullName = readerEntity.FullName,
            PhoneNumber = readerEntity.PhoneNumber,
            ExpiryDate = readerEntity.ExpiryDate,
            IsActive = readerEntity.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}