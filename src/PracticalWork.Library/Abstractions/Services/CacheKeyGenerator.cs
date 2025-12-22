// Application/Services/CacheKeyGenerator.cs

using JetBrains.Annotations;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Abstractions.Services
{
    /// <summary>
    /// Генератор ключей для Redis кэша
    /// </summary>
    public class CacheKeyGenerator : ICacheKeyGenerator
    {
        public string BooksListPrefix => "books:list";
        public string LibraryBooksPrefix => "library:books";
        public string BookDetailsPrefix => "book:details";
        public string ReaderBooksPrefix => "reader:books";

        /// <summary>
        /// Генерация хэша для фильтров
        /// </summary>
        private string GenerateFilterHash(BookCategory? category = null, [CanBeNull] string author = null, BookStatus? status = null)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            
            var input = string.Empty;
            if (status.HasValue) input += $"status={(int)status.Value}";
            if (category.HasValue) input += $"category={(int)category.Value}";
            if (!string.IsNullOrWhiteSpace(author)) input += $"author={author.Trim().ToLowerInvariant()}";
            
            if (string.IsNullOrEmpty(input))
                return "all";
            
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// books:list:{hash}
        /// </summary>
        public string GenerateBooksListKey(
            int? page, 
            int? pageSize, 
            BookCategory? category = null, 
            string author = null, 
            BookStatus? status = null)
        {
            var filterHash = GenerateFilterHash(category, author, status);
            return $"{BooksListPrefix}:{filterHash}:page={page}:size={pageSize}";
        }

        /// <summary>
        /// library:books:{hash}
        /// </summary>
        public string GenerateLibraryBooksKey(
            int page, 
            int pageSize, 
            BookCategory? category = null, 
            string author = null, 
            BookStatus? status = null)
        {
            var filterHash = GenerateFilterHash(category, author, status);
            return $"{LibraryBooksPrefix}:{filterHash}:page={page}:size={pageSize}";
        }

        /// <summary>
        /// book:details:{id}
        /// </summary>
        public string GenerateBookDetailsKey(Guid bookId)
        {
            return $"{BookDetailsPrefix}:{bookId}";
        }

        /// <summary>
        /// reader:books:{readerId}
        /// </summary>
        public string GenerateReaderBooksKey(Guid readerId)
        {
            return $"{ReaderBooksPrefix}:{readerId}";
        }

        /// <summary>
        /// Генерация ключа для блокировки (distributed lock)
        /// </summary>
        public string GenerateLockKey(string resource, Guid resourceId)
        {
            return $"lock:{resource}:{resourceId}";
        }

        /// <summary>
        /// Генерация ключа для статистики
        /// </summary>
        public string GenerateStatsKey(string statName, DateTime date)
        {
            return $"stats:{statName}:{date:yyyy-MM-dd}";
        }
    }
}