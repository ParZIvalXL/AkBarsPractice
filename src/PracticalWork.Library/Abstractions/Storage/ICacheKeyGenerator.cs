using JetBrains.Annotations;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Abstractions.Storage
{
    /// <summary>
    /// Генератор ключей для кэша
    /// </summary>
    public interface ICacheKeyGenerator
    {
        string GenerateBooksListKey(int? page, int? pageSize, BookCategory? category = null, [CanBeNull] string author = null, BookStatus? status = null);
        string GenerateLibraryBooksKey(int page, int pageSize, BookCategory? category = null, [CanBeNull] string author = null, BookStatus? status = null);
        string GenerateBookDetailsKey(Guid bookId);
        string GenerateReaderBooksKey(Guid readerId);
        
        // Префиксы для массовой инвалидации
        string BooksListPrefix { get; }
        string LibraryBooksPrefix { get; }
        string BookDetailsPrefix { get; }
        string ReaderBooksPrefix { get; }
    }
}