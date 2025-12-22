using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

/// <summary>
/// Репозиторий читателей
/// </summary>
public class ReaderRepository : IReaderRepository
{
    private readonly AppDbContext _appDbContext;

    public ReaderRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Guid> CreateReader(Reader reader)
    {
        ReaderEntity entity = new ReaderEntity()
        {
            FullName = reader.FullName,
            PhoneNumber = reader.PhoneNumber,
            ExpiryDate = reader.ExpiryDate,
            IsActive = reader.IsActive,
            CreatedAt = reader.CreatedAt,
            UpdatedAt = reader.UpdatedAt
        };
        
        _appDbContext.Readers.Add(entity);
        await _appDbContext.SaveChangesAsync();
        
        return entity.Id;
    }

    public async Task UpdateReader(Guid id, Reader reader)
    {
        ReaderEntity entity = _appDbContext.Readers.FirstOrDefault(r => r.Id == id);
        
        if(entity == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        entity.FullName = reader.FullName;
        entity.PhoneNumber = reader.PhoneNumber;
        entity.ExpiryDate = reader.ExpiryDate;
        entity.IsActive = reader.IsActive;
        entity.UpdatedAt = reader.UpdatedAt;
        
        _appDbContext.Readers.Update(entity);
        await _appDbContext.SaveChangesAsync();
    }

    public Task<Reader> GetReaderById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Reader>> GetReaders(int? readersPerPage, int? page, string name = null)
    {
        throw new NotImplementedException();
    }

    public bool IsPhoneNumberExistsInDatabase(string phoneNumber)
    {
        return _appDbContext.Readers.Any(r => r.PhoneNumber == phoneNumber);
    }
}