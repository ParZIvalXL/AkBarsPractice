// Cache/CacheKeys.cs

using JetBrains.Annotations;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Cache.Redis
{
    /// <summary>
    /// Ключи кэша
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Получение книги
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <returns>Ключ книги</returns>
        public static string BookDetails(Guid id) => $"books:details:{id}";
        
        /// <summary>
        /// Получение списка книг
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="pageSize">Количество книг на странице</param>
        /// <param name="category">Категория книг</param>
        /// <param name="author">Автор книг</param>
        /// <param name="status">Статус книг</param>
        /// <returns>Ключ списка книг</returns>
        public static string BooksList(
            int page, 
            int pageSize, 
            BookCategory? category = null, 
            [CanBeNull] string author = null, 
            BookStatus? status = null)
        {
            var key = $"books:list:page={page}:size={pageSize}";
            
            if (status.HasValue)
                key += $":status={status.Value.ToString().ToLowerInvariant()}";
            
            if (category.HasValue)
                key += $":category={category.Value.ToString().ToLowerInvariant()}";
            
            if (!string.IsNullOrWhiteSpace(author))
                key += $":author={author.Trim().ToLowerInvariant()}";
                
            return key;
        }
        
        
        public static string LibraryBooks(Guid libraryId, int page, int pageSize) 
            => $"library:books:{libraryId}:page={page}:size={pageSize}";
    }
}