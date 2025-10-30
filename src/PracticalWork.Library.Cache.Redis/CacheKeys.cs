namespace PracticalWork.Library.Cache.Redis;

public class CacheKeys
{
    public static string BooksList(string hash) => $"books:list:{hash}";
    
    public static string LibraryBooks(string hash) => $"library:books:{hash}";

    public static string BookDetails(Guid id) => $"book:details:{id}";

    public static string ReaderBooks(Guid readerId) => $"reader:books:{readerId}";
}