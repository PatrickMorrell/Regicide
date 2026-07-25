namespace Regicide.Extensions;

public static class ListExtensions
{
    public static void ShuffleDeck<T>(this IList<T> list)
    {
        Random random = Random.Shared;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
