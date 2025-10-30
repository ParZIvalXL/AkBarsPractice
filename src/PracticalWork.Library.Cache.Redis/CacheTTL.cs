namespace PracticalWork.Library.Cache.Redis;

public class CacheTtl
{
    public static readonly TimeSpan BooksList = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan LibraryBooks = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan BookDetails = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan ReaderBooks = TimeSpan.FromMinutes(15);
}