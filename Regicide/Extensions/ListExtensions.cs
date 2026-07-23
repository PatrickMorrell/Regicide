namespace Regicide.Extensions;

public static class ListExtensions
{
    public static void ShuffleDeck<T>(this IList<T> list)
    {
        list = list.Shuffle().ToList();
    }
}
