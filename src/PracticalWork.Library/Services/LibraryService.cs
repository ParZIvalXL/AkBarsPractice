using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;
using PracticalWork.Library.Contracts.v2.Abstracts;
using PracticalWork.Library.Contracts.v2.Messaging;
using PracticalWork.Library.Contracts.v2.Readers.Events;

namespace PracticalWork.Library.Services;

public class LibraryService : ILibraryService
{
    private readonly IReaderRepository _readerRepository;
    private readonly IBookService _bookService;
    private readonly IBorrowRepository _borrowRepository;
    private readonly IMessagePublisher _messagePublisher;

    public LibraryService(
        IReaderRepository readerRepository,
        IBookService bookService,
        IBorrowRepository borrowRepository,
        IMessagePublisher messagePublisher)
    {
        _readerRepository = readerRepository;
        _bookService = bookService;
        _borrowRepository = borrowRepository;
        _messagePublisher = messagePublisher;
    }
    
    public async Task<Guid> CreateBook(Book book)
    {
        book.Status = BookStatus.Available;

        var id = await _bookService.CreateBook(book);

        await _messagePublisher.PublishAsync(new BookCreatedEvent(
            id,
            book.Title,
            book.Category.ToString(),
            book.Authors.ToArray(),
            book.Year,
            DateTime.UtcNow
        ));

        return id;
    }
    
    public async Task ArchiveBook(Guid bookId, string reason = "Archived by admin")
    {
        var book = await _bookService.GetBookById(bookId);

        if (book == null)
            throw new ArgumentException("Книга не найдена");

        if (book.Status == BookStatus.Borrow)
            throw new ArgumentException("Нельзя архивировать выданную книгу");

        book.Status = BookStatus.Archived;
        await _bookService.UpdateBook(bookId, book);

        var evt = new BookArchivedEvent(
            BookId: bookId,
            Title: book.Title,
            Reason: reason,
            ArchivedAt: DateTime.UtcNow
        );
        await _messagePublisher.PublishAsync(evt);
    }

    public async Task<Guid> BorrowBook(Guid readerId, Guid bookId)
    {
        var reader = await _readerRepository.GetReaderById(readerId);
        var book = await _bookService.GetBookById(bookId);

        ValidateReaderCard(reader);

        if (book == null)
            throw new ArgumentException("Книга не найдена");
        if (book.Status == BookStatus.Borrow)
            throw new ArgumentException("Книга уже выдана");
        if (book.Status == BookStatus.Archived)
            throw new ArgumentException("Книга заархивирована");

        var borrowId = await _borrowRepository.BorrowBook(readerId, bookId);

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
        BookStatus? status = isAvailable == true ? BookStatus.Available : null;

        return await _bookService.GetBooks(booksPerPage, page, category, author, status, true);
    }

    public async Task<BookDetails> GetBookDetails(string idOrTitle)
    {
        return await _bookService.GetBookDetails(idOrTitle);
    }

    private void ValidateReaderCard(Reader reader)
    {
        if (reader == null)
            throw new ArgumentException("Карточка читателя не найдена");
        if (!reader.IsActive)
            throw new ArgumentException("Карточка читателя не активна");
    }
}
