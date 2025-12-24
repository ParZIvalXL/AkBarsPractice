using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class LibraryService : ILibraryService
{
    private readonly IReaderRepository _readerRepository;
    private readonly IBookService _bookService;
    private readonly IBorrowRepository _borrowRepository;

    public LibraryService(IReaderRepository readerRepository,
        IBookService bookService,
        IBorrowRepository borrowRepository)
    {
        _readerRepository = readerRepository;
        _bookService = bookService;
        _borrowRepository = borrowRepository;
    }

    public async Task<Guid> BorrowBook(Guid readerId, Guid bookId)
    {
        var reader = await _readerRepository.GetReaderById(readerId);
        var book = await _bookService.GetBookById(bookId);

        ValidateReaderCard(reader);
        if (book == null)
            throw new ArgumentException("Книга не найдена");

        if (reader.IsActive == false)
            throw new ArgumentException("Карточка читателя не активна");

        if (book.Status == BookStatus.Borrow)
            throw new ArgumentException("Книга уже выдана");

        if (book.Status == BookStatus.Archived)
            throw new ArgumentException("Книга заархивирована");

        Guid borrowId = await _borrowRepository.BorrowBook(readerId, bookId);
        book.Status = BookStatus.Borrow;
        await _bookService.UpdateBook(bookId, book);


        return borrowId;
    }

    public async Task ReturnBook(Guid bookId)
    {
        var book = await _bookService.GetBookById(bookId);

        if (book == null)
            throw new ArgumentException("Книга не найдена");

        if (book.Status != BookStatus.Borrow)
            throw new ArgumentException("Книга не выдана");

        await _borrowRepository.ReturnBook(bookId);
        book.Status = BookStatus.Available;
        await _bookService.UpdateBook(bookId, book);
    }

    public async Task<List<Book>> GetLibraryBooks(int? bookCategory, string author, bool? isAvailable,
        int? booksPerPage, int? page)
    {
        BookCategory? category = bookCategory == null ? null : (BookCategory)bookCategory;
        return await _bookService.GetBooks(booksPerPage, page, category, author,
            isAvailable == true ? BookStatus.Available : null, true);
    }

    public async Task<BookDetails> GetBookDetails(string idOrTitle)
    {
        return await _bookService.GetBookDetails(idOrTitle);
    }

    private void ValidateReaderCard(Reader reader)
    {
        if (reader == null)
            throw new ArgumentException("Карточка читателя не найдена");

        if (reader.IsActive == false)
            throw new ArgumentException("Карточка читателя не активна");
    }
}