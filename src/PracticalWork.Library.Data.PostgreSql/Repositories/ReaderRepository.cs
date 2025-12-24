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
        entity.ExpiryDate = reader.ExpiryDate.ToUniversalTime();
        entity.IsActive = reader.IsActive;
        entity.UpdatedAt = reader.UpdatedAt;
        
        _appDbContext.Readers.Update(entity);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<Reader> GetReaderById(Guid id)
    {
        ReaderEntity entity = await _appDbContext.Readers.FirstOrDefaultAsync(r => r.Id == id);
        
        if(entity == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        return new Reader()
        {
            FullName = entity.FullName,
            PhoneNumber = entity.PhoneNumber,
            ExpiryDate = entity.ExpiryDate,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public Task<List<Reader>> GetReaders(int? readersPerPage, int? page, string name = null)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsPhoneNumberExistsInDatabase(string phoneNumber)
    {
        return await _appDbContext.Readers.AnyAsync(r => r.PhoneNumber == phoneNumber);
    }
}