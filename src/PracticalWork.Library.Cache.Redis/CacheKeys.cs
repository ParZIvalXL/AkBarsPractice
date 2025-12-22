// Cache/CacheKeys.cs

using JetBrains.Annotations;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Cache.Redis
{
    public static class CacheKeys
    {
        public static string BookDetails(Guid id) => $"books:details:{id}";
        
        public static string BooksList(
            int page, 
            int pageSize, 
            BookCategory? category = null, 
            [CanBeNull] string author = null, 
            BookStatus? status = null)
        {
            // Базовый ключ с пагинацией
            var key = $"books:list:page={page}:size={pageSize}";
            
            // Добавляем фильтры, если они указаны
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