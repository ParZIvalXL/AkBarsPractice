using JetBrains.Annotations;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Cache.Redis
{
    /// <summary>
    /// Ключи кэша
    /// </summary>
    public static class CacheKeys
    {
        // Префиксы для инвалидации групп ключей
        public const string BooksListPrefix = "books:list:";
        public const string ReadersListPrefix = "readers:list:";
        public const string LibraryBooksPrefix = "library:books:";
        public const string ReaderBooksPrefix = "reader:books:";
        
        /// <summary>
        /// Получение книги
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <returns>Ключ книги</returns>
        public static string BookDetails(Guid id) => $"book:details:{id}";
        
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
        
        /// <summary>
        /// Получение списка книг для модуля библиотеки
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="pageSize">Количество книг на странице</param>
        /// <param name="category">Категория книг</param>
        /// <param name="author">Автор книг</param>
        /// <param name="isAvailable">Доступность книги</param>
        /// <returns>Ключ списка книг библиотеки</returns>
        public static string LibraryBooksList(
            int page,
            int pageSize,
            BookCategory? category = null,
            [CanBeNull] string author = null,
            bool? isAvailable = null)
        {
            var key = $"library:books:page={page}:size={pageSize}";
            
            if (isAvailable.HasValue)
                key += $":available={isAvailable.Value}";
            
            if (category.HasValue)
                key += $":category={category.Value.ToString().ToLowerInvariant()}";
            
            if (!string.IsNullOrWhiteSpace(author))
                key += $":author={author.Trim().ToLowerInvariant()}";
                
            return key;
        }
        
        /// <summary>
        /// Получение деталей читателя
        /// </summary>
        /// <param name="id">ID читателя</param>
        /// <returns>Ключ читателя</returns>
        public static string ReaderDetails(Guid id) => $"reader:details:{id}";
        
        /// <summary>
        /// Получение списка читателей
        /// </summary>
        /// <param name="page">Страница</param>
        /// <param name="pageSize">Количество читателей на странице</param>
        /// <param name="name">Имя читателя (частичное совпадение)</param>
        /// <returns>Ключ списка читателей</returns>
        public static string ReadersList(
            int page,
            int pageSize,
            [CanBeNull] string name = null)
        {
            var key = $"readers:list:page={page}:size={pageSize}";
            
            if (!string.IsNullOrWhiteSpace(name))
                key += $":name={name.Trim().ToLowerInvariant()}";
                
            return key;
        }
        
        /// <summary>
        /// Получение списка книг на руках у читателя
        /// </summary>
        /// <param name="readerId">ID читателя</param>
        /// <returns>Ключ книг читателя</returns>
        public static string ReaderBooks(Guid readerId) => $"reader:books:{readerId}";
        
        /// <summary>
        /// Получение информации о заимствовании книги
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Ключ заимствования книги</returns>
        public static string BookBorrowInfo(Guid bookId) => $"borrow:book:{bookId}";
        
        /// <summary>
        /// Получение статистики библиотеки
        /// </summary>
        /// <returns>Ключ статистики</returns>
        public static string LibraryStats() => "library:stats";
        
        /// <summary>
        /// Получение проверки телефона
        /// </summary>
        /// <param name="phoneNumber">Номер телефона</param>
        /// <returns>Ключ проверки телефона</returns>
        public static string PhoneCheck(string phoneNumber) => 
            $"phone:check:{phoneNumber.Replace(" ", "").Replace("-", "").Replace("+", "")}";
        
        /// <summary>
        /// Получение авторов
        /// </summary>
        /// <returns>Ключ списка авторов</returns>
        public static string AuthorsList() => "authors:list";
        
        /// <summary>
        /// Получение доступных категорий
        /// </summary>
        /// <returns>Ключ категорий</returns>
        public static string CategoriesList() => "categories:list";
        
        /// <summary>
        /// Генерация ключа для кэширования с поддержкой nullable параметров
        /// </summary>
        public static string GenerateBooksListKey(
            int? page = null,
            int? pageSize = null,
            BookCategory? category = null,
            [CanBeNull] string author = null,
            BookStatus? status = null)
        {
            var pageNum = page ?? 1;
            var size = pageSize ?? 20;
            
            return BooksList(pageNum, size, category, author, status);
        }
        
        /// <summary>
        /// Генерация ключа для кэширования списка читателей с поддержкой nullable параметров
        /// </summary>
        public static string GenerateReadersListKey(
            int? page = null,
            int? pageSize = null,
            [CanBeNull] string name = null)
        {
            var pageNum = page ?? 1;
            var size = pageSize ?? 20;
            
            return ReadersList(pageNum, size, name);
        }
        
        /// <summary>
        /// Генерация ключа для кэширования списка книг библиотеки с поддержкой nullable параметров
        /// </summary>
        public static string GenerateLibraryBooksListKey(
            int? page = null,
            int? pageSize = null,
            BookCategory? category = null,
            [CanBeNull] string author = null,
            bool? isAvailable = null)
        {
            var pageNum = page ?? 1;
            var size = pageSize ?? 20;
            
            return LibraryBooksList(pageNum, size, category, author, isAvailable);
        }
        
        /// <summary>
        /// Генерация ключа для кэширования пагинированных данных
        /// </summary>
        public static string GeneratePaginationKey(
            string prefix,
            int? page = null,
            int? pageSize = null,
            Dictionary<string, string> filters = null)
        {
            var pageNum = page ?? 1;
            var size = pageSize ?? 20;
            
            var key = $"{prefix}:page={pageNum}:size={size}";
            
            if (filters != null)
            {
                foreach (var filter in filters.OrderBy(f => f.Key))
                {
                    if (!string.IsNullOrWhiteSpace(filter.Value))
                    {
                        key += $":{filter.Key.ToLowerInvariant()}={filter.Value.Trim().ToLowerInvariant()}";
                    }
                }
            }
            
            return key;
        }
        
        /// <summary>
        /// Создание фильтра для использования в GeneratePaginationKey
        /// </summary>
        public static Dictionary<string, string> CreateFilter(
            BookCategory? category = null,
            BookStatus? status = null,
            string author = null,
            bool? isAvailable = null,
            string name = null)
        {
            var filters = new Dictionary<string, string>();
            
            if (category.HasValue)
                filters["category"] = category.Value.ToString();
            
            if (status.HasValue)
                filters["status"] = status.Value.ToString();
            
            if (!string.IsNullOrWhiteSpace(author))
                filters["author"] = author;
            
            if (isAvailable.HasValue)
                filters["available"] = isAvailable.Value.ToString();
            
            if (!string.IsNullOrWhiteSpace(name))
                filters["name"] = name;
                
            return filters;
        }
    }
}