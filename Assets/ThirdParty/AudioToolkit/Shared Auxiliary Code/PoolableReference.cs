/*************************************************************
 *           Unity Object Pool (c) by ClockStone 2017        *
 *
 * Allows to "pool" prefab objects to avoid large number of
 * Instantiate() calls.
 *
 * Usage:
 *
 * Add the PoolableObject script component to the prefab to be pooled.
 * You can set the maximum number of objects to be be stored in the
 * pool from within the inspector.
 *
 * Replace all Instantiate( myPrefab ) calls with
 * ObjectPoolController.Instantiate( myPrefab)
 *
 * Replace all Destroy( myObjectInstance ) calls with
 * ObjectPoolController.Destroy( myObjectInstance )
 *
 * Replace all DestroyImmediate( myObjectInstance ) calls with
 * ObjectPoolController.DestroyImmediate( myObjectInstance )
 *
 * Note that Awake(), and OnDestroy() get called correctly for
 * pooled objects. However, make sure that all component data that could
 * possibly get changed during its lifetime get reinitialized by the
 * Awake() function.
 * The Start() function gets also called, but just after the Awake() function
 * during ObjectPoolController.Instantiate(...)
 *
 * If a poolable objects gets parented to none-poolable object, the parent must
 * be destroyed using ObjectPoolController.Destroy( ... )
 *
 * Be aware that OnDestroy() will get called multiple times:
 *   a) the time ObjectPoolController.Destroy() is called when the object is added
 *      to the pool
 *   b) when the object really gets destroyed (e.g. if a new scene is loaded)
 *
 * References to pooled objects will not change to null anymore once an object has
 * been "destroyed" and moved to the pool. Use PoolableReference if you need such checks.
 *
 * ********************************************************************
*/

using UnityEngine;

#pragma warning disable 1591 // undocumented XML code warning

namespace CS.Essentials
{
    /// <summary>
    /// Auxiliary class to overcome the problem of references to pooled objects that should become <c>null</c> when
    /// objects are moved back to the pool after calling <see cref="ObjectPoolController.Destroy(GameObject)"/>.
    /// </summary>
    /// <typeparam name="T">A <c>UnityEngine.Component</c></typeparam>
    /// <example>
    /// Instead of a normal reference to a script component on a poolable object use
    /// <code>
    /// MyScriptComponent scriptComponent = PoolableObjectController.Instantiate( prefab ).GetComponent&lt;MyScriptComponent&gt;();
    /// var myReference = new PoolableReference&lt;MyScriptComponent&gt;( scriptComponent );
    /// if( myReference.Get() != null ) // will check if poolable instance still belongs to the original object
    /// {
    ///     myReference.Get().MyComponentFunction();
    /// }
    /// </code>
    /// </example>
    public class PoolableReference<T>
    {
        private int _initialUsageCount;

        private T _componentOrData;

        private PoolableObject _poolableObjectOfComponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolableReference&lt;T&gt;"/> class with <c>default</c> _componentOrData.
        /// </summary>
        public PoolableReference()
        {
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolableReference&lt;T&gt;"/> class with the specified _componentOrData.
        /// </summary>
        public PoolableReference( T data, bool allowNonePoolable = false )
        {
            Set( data, allowNonePoolable );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolableReference&lt;T&gt;"/> class from
        /// a given <see cref="PoolableReference&lt;T&gt;"/>.
        /// </summary>
        /// <param name="poolableReference">The poolable reference.</param>
        public PoolableReference( PoolableReference<T> poolableReference )
        {
            _componentOrData = poolableReference._componentOrData;
            _poolableObjectOfComponent = poolableReference._poolableObjectOfComponent;
            _initialUsageCount = poolableReference._initialUsageCount;
        }

        /// <summary>
        /// Resets the reference to <c>default</c>.
        /// </summary>
        public void Reset()
        {
            _poolableObjectOfComponent = null;
            _componentOrData = default;
            _initialUsageCount = 0;
        }

        /// <summary>
        /// Gets the data.
        /// For T is ReferenceType this returns the reference or <c>null</c> if the object was
        /// already destroyed or moved to the pool.
        /// For T is ValueType this just returns data
        /// </summary>
        /// <returns>
        /// The reference to <c>T</c> or null
        /// </returns>
        public T Get()
        {
            if ( _poolableObjectOfComponent != null ) // could be set to a none-poolable object
            {
                if ( _poolableObjectOfComponent._usageCount != _initialUsageCount || _poolableObjectOfComponent._isInPool )
                {
                    _componentOrData = default;
                    _poolableObjectOfComponent = null;
                    return default;
                }
            }

            return _componentOrData;
        }

        PoolableObject GetPoolableObjectInParent( Component tComp )
        {
#if UNITY_2021_3_OR_NEWER
            return tComp.GetComponentInParent<PoolableObject>( true );
#else
            // warning: finding inactive PoolableObjects was not supported in Unity 2021 or later
            return tComp.GetComponentInParent<PoolableObject>();
#endif
        }

        /// <summary>
        /// Sets the reference to a poolable object with the specified component.
        /// </summary>
        /// <param name="data">The component of the poolable object.</param>
        /// <param name="allowNonePoolable">If set to false an error is output if the object does not have the <see cref="PoolableObject"/> component.</param>
        public void Set( T data, bool allowNonePoolable = false )
        {
            if ( data == null || ( data is Object tObj && tObj == null ) )
            {
                Reset();
                return;
            }

            if ( data is Component tComp )
                _poolableObjectOfComponent = GetPoolableObjectInParent( tComp );
            else
                _poolableObjectOfComponent = default;

            if ( _poolableObjectOfComponent == null )
            {
                if ( allowNonePoolable )
                {
                    _initialUsageCount = 0;
                }
                else
                {
                    Debug.LogError( "Object for PoolableReference must be poolable", _componentOrData as Component );
                    return;
                }
            }
            else
            {
                _initialUsageCount = _poolableObjectOfComponent._usageCount;
            }

            _componentOrData = data;
        }
    }
}