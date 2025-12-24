using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class ReaderService : IReaderService
{   
    private readonly IReaderRepository _readerRepository;
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;
    
    public ReaderService(IReaderRepository readerRepository, IBorrowRepository borrowRepository, IBookRepository bookRepository)
    {
        _readerRepository = readerRepository;
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
    }
    
    public async Task<Guid> CreateReader(Reader reader)
    {
        var id = await _readerRepository.CreateReader(reader);

        return id;
    }

    public async Task ExtendCard(Guid id, DateOnly newExpiryDate)
    {
        var reader = _readerRepository.GetReaderById(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if(!reader.IsActive)
            throw new ArgumentException("Карточка неактивна");
        
        var newExpiryUtc = newExpiryDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        
        if(reader.ExpiryDate > newExpiryUtc)
            throw new ArgumentException("Новая дата истечения срока действия не может быть меньше текущей");
        
        reader.ExpiryDate = newExpiryUtc;
        
        await _readerRepository.UpdateReader(id, reader);
    }

    public async Task CloseCard(Guid id)
    {
        var entity = _readerRepository.GetReaderById(id);
        
        if(entity == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if(!entity.IsActive)
            throw new ArgumentException("Карточка неактивна");
        
        if(_borrowRepository.IsReaderHasBorrowedBooks(id))
            throw new ArgumentException("Читатель имеет книги");
        
        entity.IsActive = false;
        
        await _readerRepository.UpdateReader(id, entity);
    }

    public async Task<List<Guid>> GetReaderBooksIds(Guid id)
    {
        Reader reader = _readerRepository.GetReaderById(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        List<Guid> books = await _borrowRepository.GetReaderBorrowedBooks(id);
        
        return books;
    }

    public Task<List<Book>> GetReaderBooks(Guid id)
    {
        throw new NotImplementedException();
        
        /*
        Reader reader = await _readerRepository.GetReaderById(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        var ids = await GetReaderBooksIds(id);
        List<Book> result = ids.Select(async (x) =>
        {
            return await _bookRepository.GetBookById(x);
        }).ToList();

        return result;
        */
    }

    public bool IsNumberExistsInDatabase(string number)
    {
        return _readerRepository.IsPhoneNumberExistsInDatabase(number);
    }
}