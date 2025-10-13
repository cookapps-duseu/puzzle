using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool<T> where T : new()
{
    public event Action<T> OnObjectTaken;
    public event Action<T> OnObjectReturned;

    private GenericObjectPool() { }

    private List<T> objectsInPool = default;
    private List<T> objectsOutOfPool = default;

    private int maxObjectCount = -1;
    private int constructedObjectCount = 0;

    /// <param name="size">Total size of the Pool</param>
    /// <param name="preConstructCount">Amount of Items to PreConstruct in Constructor. -1: all, 0: none, >0: amount up to size</param>
    /// <param name="onObjectTaken">Callback that gets registered on global OnObjectTaken to i.e. initialize object before returned by GetObject Function</param>
    /// <param name="onObjectReturned">Callback that gets registered on global OnObjectReturned to i.e. DeInitialize object when returned by ReturnObject</param>
    public GenericObjectPool( int size, int preConstructCount = -1, Action<T> onObjectTaken = null, Action<T> onObjectReturned = null )
    {
        maxObjectCount = size;

        objectsInPool = new List<T>( maxObjectCount );
        objectsOutOfPool = new List<T>( maxObjectCount );

        if ( onObjectTaken != null )
            OnObjectTaken += onObjectTaken;

        if ( onObjectReturned != null )
            OnObjectReturned += onObjectReturned;

        if ( preConstructCount != 0 )
        {
            if ( preConstructCount == -1 )
                preConstructCount = maxObjectCount;
            else
                preConstructCount = Mathf.Min( maxObjectCount, preConstructCount );

            for ( int i = 0; i < preConstructCount; i++ )
                objectsInPool.Add( new T() );

            constructedObjectCount = preConstructCount;
        }
    }

    /// <summary>
    /// Get an instance of T from pool
    /// </summary>
    /// <returns>false if object was created and there was no space in pool left - can be used to e.g. log a warning</returns>
    public bool GetObject( out T objectToReturn )
    {
        var pooled = _GetObject( out objectToReturn );

        OnObjectTaken?.Invoke( objectToReturn );

        return pooled;
    }

    private bool _GetObject( out T objectToReturn )
    {
        if ( objectsInPool.Count == 0 )
        {
            if ( constructedObjectCount < maxObjectCount )
            {
                objectToReturn = new T();

                objectsOutOfPool.Add( objectToReturn );

                constructedObjectCount++;

                return true;
            }

            objectToReturn = new T();

            return false;
        }

        objectToReturn = objectsInPool[ 0 ];

        objectsOutOfPool.Add( objectToReturn );

        objectsInPool.RemoveAt( 0 );

        return true;
    }

    public void ReturnObject( T objectToReturn )
    {
        _ReturnObject( objectToReturn );

        OnObjectReturned?.Invoke( objectToReturn );
    }

    private void _ReturnObject( T objectToReturn )
    {
        var idx = objectsOutOfPool.IndexOf( objectToReturn );

        if ( idx == -1 )
            return;

        objectsOutOfPool.RemoveAt( idx );

        objectsInPool.Add( objectToReturn );
    }
}