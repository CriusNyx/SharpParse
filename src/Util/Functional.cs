using System.Timers;

namespace GenParse.Functional
{
  public static class Functional
  {
    public static int[] Range(int value)
    {
      int[] output = new int[value];
      for (int i = 0; i < output.Length; i++)
      {
        output[i] = i;
      }
      return output;
    }

    public static U[] Map<T, U>(this T[] arr, Func<T, U> func)
    {
      return Map(arr, (t, _) => func(t));
    }

    public static U[] Map<T, U>(this T[] arr, Func<T, int, U> func)
    {
      var output = new U[arr.Length];
      for (var i = 0; i < arr.Length; i++)
      {
        output[i] = func(arr[i], i);
      }
      return output;
    }

    public static (U[] array, V context) MapReduce<T, U, V>(
      this T[] arr,
      V initialContext,
      Func<T, V, (U element, V context)> func
    )
    {
      U[] output = new U[arr.Length];
      var context = initialContext;
      for (int i = 0; i < arr.Length; i++)
      {
        U element;
        (element, context) = func(arr[i], context);
        output[i] = element;
      }
      return (output, context);
    }

    public static U[] FlatMap<T, U>(this T[] arr, Func<T, U[]> func)
    {
      List<U> output = new List<U>();
      foreach (var element in arr)
      {
        foreach (var outputElement in func(element))
        {
          output.Add(outputElement);
        }
      }
      return output.ToArray();
    }

    public static U Reduce<T, U>(this T[] arr, U initialValue, Func<T, U, U> func)
    {
      var current = initialValue;
      foreach (var element in arr)
      {
        current = func(element, current);
      }
      return current;
    }

    public static U[] Collapse<U>(this U[][] arr)
    {
      return arr.FlatMap((element) => element);
    }

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    public static Dictionary<T, V> Map<T, U, V>(this IDictionary<T, U> dict, Func<U, V> func)
    {
      var output = new Dictionary<T, V>();
      foreach (var (key, value) in dict)
      {
        output[key] = func(value);
      }
      return output;
    }
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

    public static bool TryMap<T, U>(this T[] arr, Func<T, U> tryFunc, out U[] result)
    {
      var output = new U[arr.Length];
      for (var i = 0; i < arr.Length; i++)
      {
        try
        {
          output[i] = tryFunc(arr[i]);
        }
        catch
        {
          result = [];
          return false;
        }
      }
      result = [];
      return false;
    }

    public static T[] Filter<T>(this T[] arr, Func<T, bool> func)
    {
      return arr.Where(func).ToArray();
    }

    public static U[] FilterByType<T, U>(this T[] arr)
    {
      List<U> output = new List<U>();
      foreach (var element in arr)
      {
        if (element is U u)
        {
          output.Add(u);
        }
      }
      return output.ToArray();
    }

    public static U FindByType<T, U>(this T[] arr)
    {
      foreach (var element in arr)
      {
        if (element is U u)
        {
          return u;
        }
      }
      return default!;
    }

    public static IEnumerable<T> UntilNull<T>(this Func<T?> func)
    {
      for (var t = func(); t != null; t = func())
      {
        yield return t;
      }
    }

    public static T? FirstNotNull<T>(this T[] arr)
    {
      return arr.FirstNotNull(t => t);
    }

    public static U? FirstNotNull<T, U>(this T[] arr, Func<T, U?> func)
    {
      foreach (var value in arr)
      {
        var result = func(value);
        if (result != null)
        {
          return result;
        }
      }
      return default;
    }

    public static bool TryGet<T>(this T[] arr, int index, out T value)
    {
      if (index >= 0 && index < arr.Length)
      {
        value = arr[index];
        return true;
      }
      value = default!;
      return false;
    }

    public static T? SafeGet<T>(this T[] arr, int index)
    {
      if (index >= 0 && index < arr.Length)
      {
        return arr[index];
      }
      return default;
    }

    public static U? Safe<T, U>(this IReadOnlyDictionary<T, U> dict, T key) =>
      SafeGet(dict!, key, default);

    public static U? SafeGet<T, U>(this IReadOnlyDictionary<T, U> dict, T key, U defaultValue)
    {
      if (dict.ContainsKey(key))
      {
        return dict[key];
      }
      return defaultValue;
    }

    public static U AddOrGet<T, U>(this IDictionary<T, U> dict, T key, Func<U> constructor)
    {
      if (dict.TryGetValue(key, out var u))
      {
        return u;
      }
      else
      {
        var output = constructor();
        dict.Add(key, output);
        return output;
      }
    }

    public static List<U> AddOrGet<T, U>(this IDictionary<T, List<U>> dict, T key) =>
      AddOrGet(dict, key, () => new List<U>());

    public static (T, U) With<T, U>(this T value, U other)
    {
      return (value, other);
    }

    public static T[] Push<T>(this T[] arr, T[] other)
    {
      T[] output = new T[arr.Length + other.Length];
      Array.Copy(arr, output, arr.Length);
      Array.Copy(other, 0, output, arr.Length, other.Length);
      return output;
    }

    public static T[] Push<T>(this T[] arr, T other)
    {
      T[] output = new T[arr.Length + 1];
      Array.Copy(arr, output, arr.Length);
      output[arr.Length] = other;
      return output;
    }

    public static T[] PushOrReplace<T>(this T[] arr, T other, Func<T, bool> evaluate = null!)
    {
      evaluate = evaluate ?? ((T element) => element!.Equals(other));
      for (int i = 0; i < arr.Length; i++)
      {
        var element = arr[i];
        if (evaluate(element))
        {
          var output = arr.Clone() as T[];
          output![i] = element;
          return output;
        }
      }
      return Push(arr, other);
    }

    public static T[] ToTypedArray<T>(this object o)
    {
      var arr = (object[])o;
      return arr!.Map(x => (T)x);
    }

    public static object ToTypedArray<T>(this T[] arr, Type t)
    {
      var output = Array.CreateInstance(t, arr.Length);
      arr.CopyTo(output, 0);
      return output;
    }

    public static T[] Spread<T>(this T[] arr, int startIndex, int? endIndex = null)
    {
      var end = endIndex ?? arr.Length;
      var len = end - startIndex;
      if (len < 0)
      {
        return new T[] { };
      }
      var output = new T[len];
      Array.Copy(arr, startIndex, output, 0, len);
      return output;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="root"></param>
    /// <param name="getChildren"></param>
    /// <param name="visit"></param>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// <code>
    /// root.Crawl(node => node.children, node => DoThing(node));
    /// </code>
    /// /// </example>
    public static void Crawl<T>(this T root, Func<T, IEnumerable<T>> getChildren, Action<T> visit)
    {
      visit(root);
      foreach (var child in getChildren(root))
      {
        Crawl(child, getChildren, visit);
      }
    }

    public static void Crawl<T, U>(
      this T root,
      U initialContext,
      Func<T, U, (IEnumerable<T>, U)> traversalFunc,
      Action<T, U> visitorFunc
    )
    {
      visitorFunc(root, initialContext);
      var (children, newContext) = traversalFunc(root, initialContext);
      foreach (var child in children)
      {
        Crawl(child, newContext, traversalFunc, visitorFunc);
      }
    }

    public static T[] FromDefined<T>(params T?[] arr)
    {
      return arr.FilterDefined();
    }

    public static T[] FilterDefined<T>(this T?[] arr)
    {
      return arr.Filter(x => x != null)!;
    }

    public static T NotNull<T>(this T? val)
    {
      if (val == null)
      {
        throw new NullReferenceException("Value should not be null");
      }
      return val;
    }

    public static List<T> ReplaceOrAdd<T>(this List<T> list, Func<T, bool> func, Func<T, T> replace)
    {
      for (int i = 0; i < list.Count; i++)
      {
        var element = list[i];
        if (func(element))
        {
          list[i] = replace(element);
          return list;
        }
      }
      list.Add(replace(default!));
      return list;
    }

    public static T FindOrAdd<T>(this List<T> list, Func<T, bool> searchFunc, Func<T> constructor)
    {
      foreach (var element in list)
      {
        if (searchFunc(element))
        {
          return element;
        }
      }
      var newElement = constructor();
      list.Add(newElement);
      return newElement;
    }

    public static string ReplaceAt(this string source, int index, int len, string newString)
    {
      return source.Remove(index, len).Insert(index, newString);
    }

    public static bool HasIndex<T>(this T[] arr, int index)
    {
      return index >= 0 && index < arr.Length;
    }

    public static T To<T>(this object value)
    {
      if (value is T output)
      {
        return output;
      }
      throw new NullReferenceException();
    }
  }
}
