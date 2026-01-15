using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListUtils
{
    public static bool NullOrEmpty<T>(this IEnumerable<T> list)
    {
        return list == null || list.Count() == 0;
    }

    public static bool NotNullAndContains<T>(this IEnumerable<T> list, T item)
    {
        return list != null && list.Count() > 0 && list.Contains(item);
    }

    public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> source)
    {
        return source.OrderBy<T, int>((item) => UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    public static T RandomElement<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
            throw new InvalidOperationException("Cannot pick a random element from an empty list");

        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    public static T RandomElementOrDefault<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
            return default;

        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    public static void SortBy<T, U>(this List<T> list, Func<T, U> selector) where U : IComparable<U>
    {
        if(list.Count <= 1)
            return;

        list.Sort((a, b) => selector(a).CompareTo(selector(b)));
    }
    public static void SortByDescending<T, U>(this List<T> list, Func<T, U> selector) where U : IComparable<U>
    {
        if(list.Count <= 1)
            return;

        list.Sort((a, b) => selector(b).CompareTo(selector(a)));
    }
}
