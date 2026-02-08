using System.Security.Cryptography;
using System.Text;
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
        // Префиксы для массовой инвалидации
        public string BooksListPrefix => "books:list";
        public string LibraryBooksPrefix => "library:books";
        public string BookDetailsPrefix => "book:details";
        public string ReaderDetailsPrefix => "reader:details";
        public string ReadersListPrefix => "readers:list";
        public string ReaderBooksPrefix => "reader:books";
        public string ReportsListPrefix => "reports:list";
        public string ReportsListKey => "reports:list:all";
        
        /// <summary>
        /// Генерация хэша MD5 для фильтров
        /// </summary>
        public string GenerateFilterHash(
            BookCategory? category = null, 
            [CanBeNull] string author = null, 
            BookStatus? status = null,
            [CanBeNull] string name = null,
            bool? isAvailable = null)
        {
            using var md5 = MD5.Create();
            
            var inputBuilder = new StringBuilder();
            
            if (status.HasValue) 
                inputBuilder.Append($"status={(int)status.Value}");
            if (category.HasValue) 
                inputBuilder.Append($"category={(int)category.Value}");
            if (!string.IsNullOrWhiteSpace(author)) 
                inputBuilder.Append($"author={author.Trim().ToLowerInvariant()}");
            if (!string.IsNullOrWhiteSpace(name)) 
                inputBuilder.Append($"name={name.Trim().ToLowerInvariant()}");
            if (isAvailable.HasValue) 
                inputBuilder.Append($"available={isAvailable.Value}");
            
            var input = inputBuilder.ToString();
            
            if (string.IsNullOrEmpty(input))
                return "all";
            
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
        
        /// <summary>
        /// books:list:{hash}:page={page}:size={pageSize}
        /// TTL: 10 минут
        /// </summary>
        public string GenerateBooksListKey(
            int? page, 
            int? pageSize, 
            BookCategory? category = null, 
            string author = null, 
            BookStatus? status = null)
        {
            var filterHash = GenerateFilterHash(category, author, status);
            var pageNum = page ?? 1;
            var size = pageSize ?? 20;
            return $"{BooksListPrefix}:{filterHash}:page={pageNum}:size={size}";
        }
        
        /// <summary>
        /// library:books:{hash}:page={page}:size={pageSize}
        /// TTL: 5 минут
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
        /// TTL: 30 минут
        /// </summary>
        public string GenerateBookDetailsKey(Guid bookId)
        {
            return $"{BookDetailsPrefix}:{bookId}";
        }
        
        /// <summary>
        /// reader:details:{id}
        /// TTL: 30 минут
        /// </summary>
        public string GenerateReaderDetailsKey(Guid readerId)
        {
            return $"{ReaderDetailsPrefix}:{readerId}";
        }
        
        /// <summary>
        /// readers:list:{hash}:page={page}:size={pageSize}
        /// TTL: 10 минут
        /// </summary>
        public string GenerateReadersListKey(
            int? page, 
            int? pageSize, 
            [CanBeNull] string name = null)
        {
            var filterHash = GenerateFilterHash(name: name);
            var pageNum = page ?? 1;
            var size = pageSize ?? 20;
            return $"{ReadersListPrefix}:{filterHash}:page={pageNum}:size={size}";
        }
        
        /// <summary>
        /// reader:books:{readerId}
        /// TTL: 15 минут
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
        
        /// <summary>
        /// Генерация ключа для проверки телефона
        /// phone:check:{normalizedPhone}
        /// </summary>
        public string GeneratePhoneCheckKey(string phoneNumber)
        {
            var normalized = NormalizePhoneNumber(phoneNumber);
            return $"phone:check:{normalized}";
        }
        
        /// <summary>
        /// Нормализация номера телефона для ключа
        /// </summary>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "empty";
            
            var normalized = System.Text.RegularExpressions.Regex.Replace(
                phoneNumber, 
                @"[^\d+]", 
                "");
            if (normalized.StartsWith("+7"))
                normalized = "7" + normalized.Substring(2);
            else if (normalized.StartsWith("8") && normalized.Length > 1)
                normalized = "7" + normalized.Substring(1);
            
            return normalized;
        }
        
        /// <summary>
        /// Генерация ключа для кэширования с произвольными фильтрами
        /// </summary>
        public string GenerateCustomKey(string prefix, params (string Key, string Value)[] filters)
        {
            var filterBuilder = new StringBuilder();
            foreach (var filter in filters.OrderBy(f => f.Key))
            {
                if (!string.IsNullOrWhiteSpace(filter.Value))
                {
                    filterBuilder.Append($"{filter.Key}={filter.Value}|");
                }
            }
            
            var filterString = filterBuilder.ToString().TrimEnd('|');
            var filterHash = string.IsNullOrEmpty(filterString) 
                ? "all" 
                : ComputeMD5Hash(filterString);
            
            return $"{prefix}:{filterHash}";
        }
        
        private string ComputeMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}