using System;
using System.Collections.Generic;

public class ReactiveList<T>
{
    private readonly List<T> _list = [];
    
    public Action<T> OnAdded { get; set; }
    public Action<T> OnRemoved { get; set; }

    public void Add(T item)
    {
        _list.Add(item);
        OnAdded?.Invoke(item);
    }

    public void Remove(T item)
    {
        _list.Remove(item);
        OnRemoved?.Invoke(item);
    }

    public bool Contains(T item) => _list.Contains(item);
    public IReadOnlyList<T> Items => _list;
}