using System.Collections.Generic;
using Unity.Collections;

public static class NativeCollectionUtility
{
    public static T[] ToNewArray<T>(this ICollection<T> collection)
    {
        T[] array = new T[collection.Count];
        collection.CopyTo(array, 0);
        return array;
    }

    public static NativeArray<T> ToNativeArray<T>(this ICollection<T> collection, Allocator allocator = Allocator.TempJob) where T : struct
        => new NativeArray<T>(ToNewArray(collection), allocator);

    public static bool IsDeafult<T>(this T obj) => EqualityComparer<T>.Default.Equals(obj, default(T));
}