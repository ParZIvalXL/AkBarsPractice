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
        var reader = await _readerRepository.GetReaderById(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        reader.ExpiryDate = new DateTime(newExpiryDate.Year, newExpiryDate.Month, newExpiryDate.Day);
        
        await _readerRepository.UpdateReader(id, reader);
    }

    public Task CloseCard(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Guid>> GetReaderBooksIds(Guid id)
    {
        Reader reader = await _readerRepository.GetReaderById(id);
        
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