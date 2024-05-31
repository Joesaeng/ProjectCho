using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBuilder<TKey, TData, TEntity>
{
    private readonly Dictionary<TKey, TEntity> _dataDict = new();
    private readonly Func<TData, TEntity> _entityFactory;

    public DataBuilder(Func<TData, TEntity> entityFactory)
    {
        _entityFactory = entityFactory;
    }

    public DataBuilder<TKey, TData, TEntity> AddData(TKey key, TData data)
    {
        _dataDict.Add(key, _entityFactory(data));
        return this;
    }

    public Dictionary<TKey, TEntity> Build() => _dataDict;
}
