using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ObservableList<T> : IList<T>
{
    public List<T> values;

    public ObservableList()
    {
        values = new();
    }
    public ObservableList(List<T> init)
    {
        values = init;
    }

    public class ChangedEventArgs : EventArgs
    {
        public int OldSize { get; set; }
        public int NewSize { get; set; }
    }

    [NonSerialized]
    public EventHandler<ChangedEventArgs> Changed;

    public T this[int index]
    {
        get => values[index]; set
        {
            values[index] = value; Changed?.Invoke(this, new ChangedEventArgs
            {
                OldSize = values.Count,
                NewSize = values.Count
            });
        }
    }

    public int Count => values.Count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        values.Add(item);
        Changed?.Invoke(this, new()
        {
            NewSize = values.Count,
            OldSize = values.Count - 1
        });
    }

    public void Clear()
    {
        var oldSize = values.Count;
        values.Clear();
        Changed?.Invoke(this, new()
        {
            NewSize = 0,
            OldSize = oldSize
        });
    }

    public bool Contains(T item)
    {
        return values.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        values.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return values.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        values.Insert(index, item);
        Changed?.Invoke(this, new()
        {
            NewSize = values.Count,
            OldSize = values.Count - 1
        });
    }

    public bool Remove(T item)
    {
        var removed = values.Remove(item);
        Changed?.Invoke(this, new()
        {
            NewSize = values.Count,
            OldSize = values.Count + (removed ? 1 : 0)
        });
        return removed;
    }

    public void RemoveAt(int index)
    {
        var oldSize = values.Count;
        values.RemoveAt(index);
        Changed?.Invoke(this, new()
        {
            NewSize = values.Count,
            OldSize = oldSize
        });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return values.GetEnumerator();
    }
}

[Serializable]
public class Observable<T>
{
    public T value;

    public Observable(T init)
    {
        value = init;
    }

    public class ChangedEventArgs : EventArgs
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }

    [NonSerialized]
    public EventHandler<ChangedEventArgs> Changed;

    public T Value
    {
        get { return value; }
        set
        {
            if (!value.Equals(this.value))
            {
                T oldValue = this.value;
                this.value = value;
                Changed?.Invoke(this, new ChangedEventArgs
                {
                    OldValue = oldValue,
                    NewValue = value
                });
            }
        }
    }

    public void SetWithoutNotification(T value) { this.value = value; }

    public override string ToString()
    {
        return value.ToString();
    }

    public static implicit operator Observable<T>(T init)
    {
        return new Observable<T>(init);
    }
}