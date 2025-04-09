namespace GenParse.Util
{
  public class Thunk<T>
  {
    readonly T value;

    public Thunk(Func<T> generator)
    {
      value = generator();
    }

    public static implicit operator T(Thunk<T> thunk) => thunk.value;
  }
}
