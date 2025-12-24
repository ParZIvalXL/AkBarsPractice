using JetBrains.Annotations;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Abstractions.Storage
{
    /// <summary>
    /// Генератор ключей для кэша
    /// </summary>
    public interface ICacheKeyGenerator
    {
        // Books
        string GenerateBooksListKey(int? page, int? pageSize, BookCategory? category = null, 
            [CanBeNull] string author = null, BookStatus? status = null);
        string GenerateLibraryBooksKey(int page, int pageSize, BookCategory? category = null, 
            [CanBeNull] string author = null, BookStatus? status = null);
        string GenerateBookDetailsKey(Guid bookId);
        
        // Readers
        string GenerateReaderDetailsKey(Guid readerId);
        string GenerateReadersListKey(int? page, int? pageSize, [CanBeNull] string name = null);
        string GenerateReaderBooksKey(Guid readerId);
        
        // Префиксы для массовой инвалидации
        string BooksListPrefix { get; }
        string LibraryBooksPrefix { get; }
        string BookDetailsPrefix { get; }
        string ReaderDetailsPrefix { get; }
        string ReadersListPrefix { get; }
        string ReaderBooksPrefix { get; }
        
        // Вспомогательные методы
        string GenerateFilterHash(BookCategory? category = null, [CanBeNull] string author = null, 
            BookStatus? status = null, [CanBeNull] string name = null, bool? isAvailable = null);
        string GenerateLockKey(string resource, Guid resourceId);
        string GenerateStatsKey(string statName, DateTime date);
        string GeneratePhoneCheckKey(string phoneNumber);
    }
}