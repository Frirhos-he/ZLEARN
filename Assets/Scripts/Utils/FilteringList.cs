using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface IIdentifier<I>
{
    I Name { get; }
    bool Matches(I identifier);
}
#nullable enable

/// <summary>
/// This is a utility class that represents a filtered list, having a list backing itself
/// Its main intended use is for UIToolkit's ListView
/// </summary>
/// <typeparam name="T">Inner type of the list</typeparam>
/// <typeparam name="I">Type of identifier of each element of the list</typeparam>
public class FilteringList<T, I> : IList where T : IIdentifier<I> where I : class
{
    private readonly List<T> values;
    private List<int> filteredValuesIndexes;
    private I? filter;

    private List<int> UpdateFilter(I? filter)
    {
        if (filter == null)
        {
            return values.Select((_, index) => index).ToList();
        }
        else
        {
            return values.Select((v, index) => (v, index)).Where(vi => vi.v.Matches(filter)).Select(vi => vi.index).ToList();
        }
    }

    public I? Filter
    {
        get => filter; set
        {
            filter = value;
            filteredValuesIndexes = UpdateFilter(filter);
        }
    }
    public FilteringList(List<T> values, I? filter)
    {
        this.values = values;
        this.filter = filter;
        filteredValuesIndexes = UpdateFilter(filter);
    }

    public int Count => filteredValuesIndexes.Count;

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public bool IsSynchronized => false;

    public object SyncRoot => false;

    object IList.this[int index] { get => values[filteredValuesIndexes[index]]; set => values[filteredValuesIndexes[index]] = (T)value; }
    public T this[int index] { get => values[filteredValuesIndexes[index]]; set => values[filteredValuesIndexes[index]] = value; }

    public void Clear()
    {
        filteredValuesIndexes.Clear();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return filteredValuesIndexes.Select((i) => values[i]).GetEnumerator();
    }

    /// !!!! METHODS BELOW ARE REQUIRED BY THE INTERFACE BUT NOT USED BY UNITY !!!!
    public int Add(object value)
    {
        throw new NotImplementedException();
    }

    public bool Contains(object value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(object value)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, object value)
    {
        throw new NotImplementedException();
    }

    public void Remove(object value)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }
}
#nullable disable
