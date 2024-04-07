using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CommonExtensions 
{
    public static T MostOrDefault<T>(this IEnumerable<T> collection, Func<T, T, bool> firstIsDesired)
    {
        var output = default(T);
        var first = true;

        foreach(var item in collection)
        {
            if(first || firstIsDesired(item, output))
            {
                output = item;
            }
            first = false;
        }

        return output;
    }

    public static LinkedList<Toutput> FlattenMany<Tinput, Toutput>(this IEnumerable<Tinput> collection, Func<Tinput, IEnumerable<Toutput>> selector)
    {
        var output = new LinkedList<Toutput>();

        foreach(var item in collection)
        {
            var subCollection = selector(item);

            foreach(var subItem in subCollection)
            {
                output.AddLast(subItem);
            }
        }

        return output;
    }

    public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Action<TAccumulate, TSource> action) where TAccumulate : class
    {
        Func<TAccumulate, TSource, TAccumulate> func = (seed, item) => {
            action(seed, item);
            return seed;
        };

        var acc = source.Aggregate(seed, func);

        return acc;
    }

    public static LinkedList<TItem> Aggregate<TSource, TItem>(this IEnumerable<TSource> source, Func<TSource, TItem> selector)
    {
        return source.Aggregate(new LinkedList<TItem>(), (s, i) => s.AddLast(selector(i)));
    }
}