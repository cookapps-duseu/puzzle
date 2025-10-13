/*************************************************************
 *           Unity Object Pool (c) by ClockStone 2024        *
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

using CS.Essentials.PlayerLoopInjections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

#pragma warning disable 1591 // undocumented XML code warning

namespace CS.Essentials
{
    /// <summary>
    /// A static class used to create and destroy poolable objects.
    /// </summary>
    /// <remarks>
    /// What is pooling? <para/>
    /// GameObject.Instantiate(...) calls are relatively time expensive. If objects of the same
    /// type are frequently created and destroyed it is good practice to use object pools, particularly on mobile
    /// devices. This can greatly reduce the performance impact for object creation and garbage collection. <para/>
    /// How does pooling work?<para/>
    /// Instead of actually destroying object instances, they are just set inactive and moved to an object "pool".
    /// If a new object is requested it can then simply be pulled from the pool, instead of creating a new instance. <para/>
    /// Awake(), Start() and OnDestroy() are called if objects are retrieved from or moved to the pool like they 
    /// were instantiated and destroyed normally.
    /// </remarks>
    /// <example>
    /// How to set up a prefab for pooling:
    /// <list type="number">
    /// <item>Add the PoolableObject script component to the prefab to be pooled.
    /// You can set the maximum number of objects to be be stored in the pool from within the inspector.</item>
    /// <item> Replace all <c>Instantiate( myPrefab )</c> calls with <c>ObjectPoolController.Instantiate( myPrefab )</c></item>
    /// <item> Replace all <c>Destroy( myObjectInstance )</c> calls with <c>ObjectPoolController.Destroy( myObjectInstance )</c></item>
    /// </list>
    /// Attention: Be aware that:
    /// <list type="bullet">
    /// <item>All data must get initialized in the Awake() or Start() function</item>
    /// <item><c>OnDestroy()</c> will get called a second time once the object really gets destroyed by Unity</item>
    /// <item>If a poolable objects gets parented to none-poolable object, the parent must
    /// be destroyed using <c>ObjectPoolController.Destroy( ... )</c> even if it is none-poolable itself.</item>
    /// <item>If you store a reference to a poolable object then this reference does not evaluate to <c>null</c> after <c>ObjectPoolController.Destroy( ... )</c>
    /// was called like other references to Unity objects normally would. This is because the object still exists - it is just in the pool. 
    /// To make sure that a stored reference to a poolable object is still valid you must use <see cref="PoolableReference{T}"/>.</item>
    /// </list>
    /// </example>
    /// <seealso cref="PoolableObject"/>
    static public class ObjectPoolController
    {
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSplashScreen )]
        public static void Init()
        {
            Application.quitting += () => ApplicationQuitting = true;

#if !CS_ESSENTIALS_ASSETSTORE
            //When we manually restart the App all poolableObjects get destroyed - so we have to clear possible
            //queued elements to prevent null-ref exceptions when queues run the next time
            AppRestarter.OnBeforeAppRestart += ClearQueues;
#endif

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            //this will inject a callback in the unity playerLoop after PostLateUpdate/ScriptRunDelayedDynamicFrameRate. This is where MonoBehaviour.Invoke() callbacks get called too.
            //we want to get close to that timing within the frame.
            CSPlayerLoopExtensions.InjectSubSystem( ref playerLoop, ObjectPoolController_DelayedQueuesSystem.GetSystem(), new string[] { "PostLateUpdate" }, "ScriptRunDelayedDynamicFrameRate" );

            //this will inject a callback in the unity playerLoop after EarlyUpdate/ScriptRunDelayedStartupFrame. This is where MonoBehaviour.Start() callbacks get called too.
            //we want to get close to that timing within the frame.
            CSPlayerLoopExtensions.InjectSubSystem( ref playerLoop, ObjectPoolController_StartSystem.GetSystem(), new string[] { "EarlyUpdate" }, "ScriptRunDelayedStartupFrame" );

            PlayerLoop.SetPlayerLoop( playerLoop );

            //Debug.Log( "Injected \"ObjectPoolController_DelayedQueuesSystem\" into PlayerLoop" );

            ObjectPoolController_DelayedQueuesSystem.DelayedQueue += ProcessQueues;
        }

        internal static class ObjectPoolController_StartSystem
        {
            //called once then cleared
            internal static event Action NextStartCallback;

            private static PlayerLoopSystem _system;

            internal static PlayerLoopSystem GetSystem()
            {
                if ( _system.type == null )
                {
                    _system = new PlayerLoopSystem
                    {
                        type = typeof( ObjectPoolController_StartSystem ),
                        updateDelegate = () =>
                        {
                            NextStartCallback?.Invoke();
                            NextStartCallback = null;
                        }
                    };
                }

                return _system;
            }
        }

        private static class ObjectPoolController_DelayedQueuesSystem
        {
            internal static event Action DelayedQueue;

            private static PlayerLoopSystem _system;

            internal static PlayerLoopSystem GetSystem()
            {
                if ( _system.type == null )
                {
                    _system = new PlayerLoopSystem
                    {
                        type = typeof( ObjectPoolController_DelayedQueuesSystem ),
                        updateDelegate = () => DelayedQueue?.Invoke()
                    };
                }

                return _system;
            }
        }

        internal static bool ApplicationQuitting { get; private set; }

        // -- stuff for delayed deletion
        private static GenericObjectPool<List<PoolableObject>> PoolableObjectListPool = new GenericObjectPool<List<PoolableObject>>( 100, 0, ( x ) => x.Clear(), ( x ) => x.Clear() );
        private static GenericObjectPool<List<PoolableObject>> PoolableObjectLateReturnToPoolListPool = new GenericObjectPool<List<PoolableObject>>( 100, 0, ( x ) => x.Clear(), ( x ) => x.Clear() );

        private static Queue<List<PoolableObject>> LateReturnToPoolQueue = new Queue<List<PoolableObject>>();
        private static Queue<GameObject> LateDestroyObjectsQueue = new Queue<GameObject>();

        private static void ClearQueues()
        {
            LateReturnToPoolQueue.Clear();
            LateDestroyObjectsQueue.Clear();
        }

        private static void ProcessQueues()
        {
            if ( ApplicationQuitting )
                return;

            while ( LateReturnToPoolQueue.Count > 0 )
            {
                var lateReturnList = LateReturnToPoolQueue.Dequeue();
                try
                {
                    for ( int i = 0; i < lateReturnList.Count; i++ )
                    {
                        try
                        {
                            var returnElement = lateReturnList[i];

                            if( returnElement == null )
                                continue;

                            if ( !returnElement._markedForLateReturnToPoolParent )
                                continue;

                            returnElement._MoveToPoolParent();
                        }
                        catch ( Exception e )
                        {
#if CS_ESSENTIALS_ASSETSTORE
                            Debug.LogError( e.Message );
#else
                            CSDebug.LogError( e.Message );
#endif
                            continue;
                        }
                    }
                }
                finally
                {
                    PoolableObjectLateReturnToPoolListPool.ReturnObject( lateReturnList );
                }
            }

            while ( LateDestroyObjectsQueue.Count > 0 )
            {
                var objectToDestroy = LateDestroyObjectsQueue.Dequeue();
                DestroyImmediate( objectToDestroy );
            }

            ClearQueues();
        }

        //---

        const string objectPoolsParentName = "ObjectPools";
        const string persistentObjectPoolsParentName = "PersistentObjectPools";

        static Transform poolsParent;
        static Transform persistentPoolsParent;

        public static Transform PersistentPoolsParent => persistentPoolsParent;

        static public bool isDuringPreload
        {
            get;
            private set;
        }

        // **************************************************************************************************/
        //          public functions
        // **************************************************************************************************/

        /// <summary>
        /// Retrieves an instance of the specified prefab. Either returns a new instance or it claims an instance 
        /// from the pool.
        /// </summary>
        /// <param name="prefab">The prefab to be instantiated.</param>
        /// <returns>
        /// An instance of the prefab.
        /// </returns>
        /// <remarks>
        /// Can be used on none-poolable objects as well. It is good practice to use <c>ObjectPoolController.Instantiate</c>
        /// whenever you may possibly make your prefab poolable in the future.
        /// </remarks>
        /// <seealso cref="Destroy(GameObject)"/>
        static public GameObject Instantiate( GameObject prefab, Transform parent = null )
        {
            PoolableObject prefabPool = prefab.GetComponent<PoolableObject>();

            if ( prefabPool == null ) // prefab not pooled, instantiate normally
                return ( GameObject ) _InstantiateGameObject( prefab, Vector3.zero, Quaternion.identity, parent );

            var pool = _GetPool( prefabPool );

            if ( pool != null )
            {
                GameObject go = pool.GetPooledInstance( null, null, prefab.activeSelf, parent );

                if ( go != null )
                    return go;
            }

            return InstantiateWithoutPool( prefab, parent );
        }

        /// <summary>
        /// Retrieves an instance of the specified prefab. Either returns a new instance or it claims an instance
        /// from the pool.
        /// </summary>
        /// <param name="prefab">The prefab to be instantiated.</param>
        /// <param name="position">The position in world coordinates.</param>
        /// <param name="quaternion">The rotation quaternion.</param>
        /// <returns>
        /// An instance of the prefab.
        /// </returns>
        /// <remarks>
        /// Can be used on none-poolable objects as well. It is good practice to use <c>ObjectPoolController.Instantiate</c>
        /// whenever you may possibly make your prefab poolable in the future.
        /// </remarks>
        /// <seealso cref="Destroy(GameObject)"/>
        static public GameObject Instantiate( GameObject prefab, Vector3 position, Quaternion quaternion, Transform parent = null )
        {
            PoolableObject prefabPool = prefab.GetComponent<PoolableObject>();

            if ( prefabPool == null ) // prefab not pooled, instantiate normally
                return ( GameObject ) _InstantiateGameObject( prefab, position, quaternion, parent );

            var pool = _GetPool( prefabPool );

            if ( pool != null )
            {
                GameObject go = pool.GetPooledInstance( position, quaternion, prefab.activeSelf, parent );

                if ( go != null )
                    return go;
            }

            return InstantiateWithoutPool( prefab, position, quaternion, parent );
        }

        /// <summary>
        /// Instantiates the specified prefab without using pooling.
        /// from the pool.
        /// </summary>
        /// <param name="prefab">The prefab to be instantiated.</param>
        /// <returns>
        /// An instance of the prefab.
        /// </returns>
        /// <remarks>
        /// If the prefab is poolable, the <see cref="PoolableObject"/> component will be removed.
        /// This way no warning is generated that a poolable object was created without pooling.
        /// </remarks>
        static public GameObject InstantiateWithoutPool( GameObject prefab, Transform parent = null )
        {
            return InstantiateWithoutPool( prefab, Vector3.zero, Quaternion.identity, parent );
        }

        /// <summary>
        /// Instantiates the specified prefab without using pooling.
        /// from the pool.
        /// </summary>
        /// <param name="prefab">The prefab to be instantiated.</param>
        /// <param name="position">The position in world coordinates.</param>
        /// <param name="quaternion">The rotation quaternion.</param>
        /// <returns>
        /// An instance of the prefab.
        /// </returns>
        /// <remarks>
        /// If the prefab is poolable, the <see cref="PoolableObject"/> component will be removed.
        /// This way no warning is generated that a poolable object was created without pooling.
        /// </remarks>
        static public GameObject InstantiateWithoutPool( GameObject prefab, Vector3 position, Quaternion quaternion, Transform parent = null )
        {
            GameObject instance;

            try
            {
                _instantiateContextCounter++;

                instance = _InstantiateGameObject( prefab, position, quaternion, parent ); // prefab not pooled, instantiate normally
            }
            finally
            {
                _instantiateContextCounter--;
            }

            PoolableObject poolableObject = instance.GetComponent<PoolableObject>();

            if ( poolableObject != null )
            {
#if UNITY_EDITOR
                //we set this here despite destroying the Component afterwards.
                //this is because the Awake function might still get called and there we log an error on non-pooled objects
                //we use this flag to prevent this error-log
                poolableObject._wasInstantiatedByObjectPoolController = true;
#endif
                //Dont use DestroyImmediate here because this is not allowed e.g. during Phyiscs Callbacks
                Component.Destroy( poolableObject );
            }

            return instance;
        }

        /// <summary>
        /// Destroys the specified game object, respectively sets the object inactive and adds it to the pool.
        /// </summary>
        /// <param name="obj">The game object.</param>
        /// <remarks>
        /// Can be used on none-poolable objects as well. It is good practice to use <c>ObjectPoolController.Destroy</c>
        /// whenever you may possibly make your prefab poolable in the future. <para/>
        /// Must also be used on none-poolable objects with poolable child objects so the poolable child objects are correctly
        /// moved to the pool.
        /// </remarks>
        /// <seealso cref="Instantiate(GameObject)"/>
        /// <returns>If Object was moved from Transform or not. -> when put into pool then this is true because Transform is no longer child of old parent.
        /// false only when no PoolableObject because then OnDestroy is called - but later and so Transform remains child of old parent till end of frame.
        /// Is used when iterating childs to know if next child is at index i or i+1</returns>
        static public bool Destroy( GameObject obj ) // destroys poolable and none-poolable objects. Destroys poolable children correctly
        {
            return _DetachChildrenAndDestroy( obj.transform, false );
        }

        /// <summary>
        /// Destroys the specified game object, respectively sets the object inactive and adds it to the pool.
        /// </summary>
        /// <param name="obj">The game object.</param>
        /// <remarks>
        /// Can be used on none-poolable objects as well. It is good practice to use <c>ObjectPoolController.Destroy</c>
        /// whenever you may possibly make your prefab poolable in the future. <para/>
        /// Must also be used on none-poolable objects with poolable child objects so the poolable child objects are correctly
        /// moved to the pool.
        /// </remarks>
        /// <seealso cref="Instantiate(GameObject)"/>
        static public void DestroyImmediate( GameObject obj ) // destroys poolable and none-poolable objects. Destroys poolable children correctly
        {
            _DetachChildrenAndDestroy( obj.transform, true );
        }

        /// <summary>
        /// Preloads as many instances to the pool so that there are at least as many as
        /// specified in <see cref="PoolableObject.preloadCount"/>. 
        /// </summary>
        /// <param name="prefab">The prefab.</param>
        /// <remarks>
        /// Use ObjectPoolController.isDuringPreload to check if an object is preloaded in the <c>Awake()</c> function.
        /// If the pool already contains at least <see cref="PoolableObject.preloadCount"/> objects, the function does nothing. 
        /// </remarks>
        /// <seealso cref="PoolableObject.preloadCount"/>
        static public void Preload( GameObject prefab ) // adds as many instances to the prefab pool as specified in the PoolableObject
        {
            PoolableObject poolObj = prefab.GetComponent<PoolableObject>();

            if ( poolObj == null )
            {
                Debug.LogWarning( "Can not preload because prefab '" + prefab.name + "' is not poolable" );
                return;
            }

            var pool = _GetPool( poolObj );

            //check how much Objects need to be preloaded
            int delta = poolObj.preloadCount - pool.GetObjectCount();
            if ( delta <= 0 )
                return;

            isDuringPreload = true;

            bool preloadActive = prefab.activeSelf;

            try
            {
                for ( int i = 0; i < delta; i++ )
                {
                    //dont use prefab.activeSelf because this may change inside Preloadinstance. use the cached value "preloadActive"
                    pool.PreloadInstance( preloadActive );
                }
            }
            finally
            {
                isDuringPreload = false;
            }

            //Debug.Log( "preloaded: " + prefab.name + " " + poolObj.preloadCount + " times" );
        }

        /// <remark>
        /// Dont call this from Unity's SceneUnload callback (too late). You have to use custom
        /// code that calls this function before starting the scene-unload
        /// </remark>
        static public void ReturnAllObjectsOnSceneUnload( Scene unloadingScene )
        {
            if ( !unloadingScene.IsValid() )
            {
                Debug.LogWarning( "ReturnAllObjectsOnSceneUnload must be called with a valid Scene!" );
                return;
            }

            foreach ( var poolKV in _pools )
            {
                var pool = poolKV.Value;

                //no need to return objects when poolParent gets destroyed along with scene anyway
                //we use hasPoolParent here because pool.poolParent is a property which would create
                //a new poolParent if there is none.
                if ( pool.hasPoolParent && pool.poolParent.gameObject.scene == unloadingScene )
                    continue;

                pool._SetAllAvailable( true, unloadingScene );
            }
        }

        static public GameObject MakePoolable(
            GameObject source,
            int maxPoolSize = 10,
            int preloadCount = 1,
            bool dontDestroyOnLoad = false,
            bool sendAwakeStartOnDestroyMessage = true,
            bool useReflectionInsteadOfMessages = false,
            bool preloadImmediately = false )
        {
            //maybe the instance GameObject is already a poolable itself so we create another
            //clean instance to work with.
            var wasActive = source.activeSelf;
            source.SetActive( false );

            var newInstance = GameObject.Instantiate( source );
            newInstance.name = source.name;

            //afterwards we delete the given instance if not a prefab
            //thus putting it into its corresponding pool (if poolable Instance)
            //or just delete the GameObject itself (if gameobject instance)
            if ( source.scene.IsValid() ) //prefabs dont have a valid scene
                ObjectPoolController.DestroyImmediate( source );
            else
                source.SetActive( wasActive );

            source = newInstance;

            //it it was a poolableObject we delete the PoolableObject reference of our clone
            //so there is no evidence left.
            var poolableObject = source.GetComponent<PoolableObject>();

            if ( poolableObject != null )
                GameObject.DestroyImmediate( poolableObject );

            //put instance as new "runtime-generated-prefab" into pool hierarchy so it stays
            //persistent. this instance will never be given out by the pool and serves as the 
            //"prefab" reference used for _GetPool lookup
            var prefabParent = new GameObject( $"PREFAB_{source.name}" );
            prefabParent.SetActive( false );
            prefabParent.transform.SetParent( persistentPoolsParent );
            source.transform.SetParent( prefabParent.transform );

            //add new poolableObjectScript and configure
            poolableObject = source.AddComponent<PoolableObject>();

            poolableObject.preloadCount = preloadCount;
            poolableObject.maxPoolSize = maxPoolSize;
            poolableObject.doNotDestroyOnLoad = dontDestroyOnLoad;
            poolableObject.sendAwakeStartOnDestroyMessage = sendAwakeStartOnDestroyMessage;

            //call _GetPool to create a new pool for the object. our instance will
            //be the prefab reference of the created pool
            var pool = _GetPool( poolableObject );

            //preload if demanded
            if ( preloadImmediately )
                ObjectPoolController.Preload( pool.prefab );

            //return this prefab reference so new instances can be created from it
            return pool.prefab;
        }

        // **************************************************************************************************/
        //          protected / private  functions
        // **************************************************************************************************/

        internal static int _globalSerialNumber = 0;

        private static int _instantiateContextCounter = 0;
        internal static bool IsDuringInstantiate => _instantiateContextCounter > 0;

        internal class ObjectPool
        {
            private List<PoolableObject> _pool;
            private GameObject _prefab;
            internal GameObject prefab
            {
                get
                {
                    return _prefab;
                }
            }

            private PoolableObject _poolableObjectComponent;

            private Transform _poolParent;

            internal bool hasPoolParent => _poolParent != null;

            internal Transform poolParent
            {
                get
                {
                    _ValidatePoolParentDummy();
                    return _poolParent;
                }
            }

            public ObjectPool( GameObject prefab )
            {
                this._prefab = prefab;
                this._poolableObjectComponent = prefab.GetComponent<PoolableObject>();
            }

            private void _ValidatePooledObjectDataContainer()
            {
                if ( _pool == null )
                {
                    _pool = new List<PoolableObject>();
                    _ValidatePoolParentDummy();
                }
            }

            private void _ValidatePoolParentDummy()
            {
                if ( _poolParent )
                    return;

                var isPersistent = _poolableObjectComponent.doNotDestroyOnLoad;

                if ( poolsParent == null && !isPersistent )
                {
                    var poolsParentGameObject = GameObject.Find( objectPoolsParentName );

                    if ( poolsParentGameObject == null )
                        poolsParentGameObject = new GameObject( objectPoolsParentName );

                    poolsParent = poolsParentGameObject.transform;
                }

                if ( persistentPoolsParent == null && isPersistent )
                {
                    var persistentPoolsParentGameObject = GameObject.Find( persistentObjectPoolsParentName );

                    if ( persistentPoolsParentGameObject == null )
                        persistentPoolsParentGameObject = new GameObject( persistentObjectPoolsParentName );

                    GameObject.DontDestroyOnLoad( persistentPoolsParentGameObject );

                    persistentPoolsParent = persistentPoolsParentGameObject.transform;
                }

                var relevantPoolsParent = poolsParent;

                if ( isPersistent )
                    relevantPoolsParent = persistentPoolsParent;

                var poolParentDummyGameObject = new GameObject( "POOL:" + _poolableObjectComponent.name );

                _poolParent = poolParentDummyGameObject.transform;
                _poolParent.SetParent( relevantPoolsParent, true );

                poolParentDummyGameObject.SetActive( false );
            }

            internal void Remove( PoolableObject poolObj )
            {
                _pool.Remove( poolObj );
            }

            internal int GetObjectCount()
            {
                return _pool == null ? 0 : _pool.Count;
            }

            internal GameObject GetPooledInstance( Vector3? position, Quaternion? rotation, bool activateObject, Transform parent = null )
            {
                _ValidatePooledObjectDataContainer();

                PoolableObject instance = null;

                for ( int i = 0; i < _pool.Count; i++ )
                {
                    var pooledElement = _pool[ i ];

                    if ( pooledElement == null ) // can happen e.g. at scene loads, so we need to clean up
                    {
                        _pool.RemoveAt( i-- );
                        continue;
                    }

                    if ( pooledElement._isInPool )
                    {
                        instance = pooledElement;

                        try
                        {
                            var transform = pooledElement.transform;
                            transform.position = ( position != null ) ? ( Vector3 ) position : _poolableObjectComponent.transform.position;
                            transform.rotation = ( rotation != null ) ? ( Quaternion ) rotation : _poolableObjectComponent.transform.rotation;
                            transform.localScale = _poolableObjectComponent.transform.localScale;
                            break;
                        }
                        catch
                        {
                            Debug.LogError( "[ObjectPoolController] Error taking element from pool. Was the element destroyed without ObjectPoolController before?" );
                        }
                    }
                }

                if ( instance == null && _pool.Count < _poolableObjectComponent.maxPoolSize ) //create and return new element
                {
                    instance = _NewPooledInstance( position, rotation, activateObject, false, parent );
                    return instance.gameObject;
                }

                if ( instance != null )
                {
                    instance.TakeFromPool( parent, activateObject );
                    return instance.gameObject;
                }

                return null;
            }

            internal PoolableObject PreloadInstance( bool preloadActive )
            {
                _ValidatePooledObjectDataContainer();

                PoolableObject poolObj = _NewPooledInstance( null, null, preloadActive, true, null );

                return poolObj;
            }

            private PoolableObject _NewPooledInstance( Vector3? position, Quaternion? rotation, bool createActive, bool addToPool, Transform notAddToPoolParent )
            {
                try
                {
                    _instantiateContextCounter++;

                    var wasActive = _prefab.activeSelf;

                    if ( wasActive )
                        _prefab.SetActive( false );

                    GameObject go = ( GameObject ) GameObject.Instantiate(
                        _prefab,
                        position ?? Vector3.zero,
                        rotation ?? Quaternion.identity
                    );

                    if ( wasActive )
                        _prefab.SetActive( true );

                    PoolableObject poolObj = go.GetComponent<PoolableObject>();

                    poolObj._pool = this;
                    poolObj._serialNumber = ++_globalSerialNumber;
                    poolObj.name += poolObj._serialNumber;

#if UNITY_EDITOR
                    poolObj._wasInstantiatedByObjectPoolController = true;
#endif

                    _pool.Add( poolObj );

                    if ( addToPool )
                    {
                        poolObj._PutIntoPool();
                        poolObj._MoveToPoolParent();
                    }
                    else
                    {
                        poolObj._isInPool = true;
                        poolObj.TakeFromPool( notAddToPoolParent, createActive );
                    }

                    return poolObj;
                }
                finally
                {
                    _instantiateContextCounter--;
                }
            }

            /// <summary>
            /// Deactivate all active pooled objects
            /// </summary>
            internal int _SetAllAvailable( bool moveToPoolParentImmediately, Scene scene = default )
            {
                _ValidatePooledObjectDataContainer();

                int count = 0;

                for ( int i = 0; i < _pool.Count; i++ )
                {
                    var element = _pool[ i ];

                    if ( element == null )
                        continue;

                    if ( !element._isInPool )
                    {
                        if ( scene.IsValid() && element.gameObject.scene != scene )
                            continue;

                        element._PutIntoPool();
                    }

                    if ( moveToPoolParentImmediately )
                        element._MoveToPoolParent(); //TODO: else cache and move later

                    count++;
                }

                return count;
            }
        }

        static private Dictionary<int, ObjectPool> _pools = new Dictionary<int, ObjectPool>();

        static private GameObject _InstantiateGameObject( GameObject prefab, Vector3 position, Quaternion rotation, Transform parent )
        {
            return GameObject.Instantiate( prefab, position, rotation, parent );
        }

        static internal ObjectPool _GetPool( PoolableObject prefabPoolComponent )
        {
            GameObject prefab;

            //when prefabPoolComponent is from prefab the _pool will always be null
            //when prefabPoolComponent is from instance the _pool will never be null
            //this allows instantiating from an instance object as well as from original prefab.
            if ( prefabPoolComponent._pool != null )
                prefab = prefabPoolComponent._pool.prefab;
            else
                prefab = prefabPoolComponent.gameObject;

            var instanceID = prefab.GetInstanceID();

            if ( !_pools.TryGetValue( instanceID, out ObjectPool pool ) )
            {
                pool = new ObjectPool( prefab );
                _pools.Add( instanceID, pool );
            }

            return pool;
        }

        /// <returns>If old Hierarchy was changed. To prevent Problems when iterating children.</returns>
        static private bool _DetachChildrenAndDestroy( Transform transform, bool destroyImmediate )
        {
            if ( !PoolableObjectListPool.GetObject( out var poolableChildren ) )
                Debug.LogWarning( "PoolableObjectListPool is empty. Object got created without pooling. Consider increasing pool size" );

            List<PoolableObject> lateReturnList = null;

            try
            {
                transform.GetComponentsInChildren<PoolableObject>( true, poolableChildren );

                PoolableObject po = null;

                //from bottom to top so the deepest nested children get returned first
                for ( int i = poolableChildren.Count - 1; i >= 0; i-- )
                {
                    po = poolableChildren[ i ];

                    //an object may have a poolableObject component on it but is no pooledInstance
                    //this can happen because when an object gets created without pool the poolableObject component
                    //gets destroyed using GameObject.Destroy(). But if this is called in the same frame
                    //the po is not.
                    //We cant use DestroyImmediate in InstantiateWithoutPool because this would cause other problems
                    //i.e. when an object gets instantiated in a physics callback.
                    if ( !po.IsPooledInstance )
                    {
                        //reset po - if its the last entry in the list po must be null afterwards if invalid
                        po = null;
                        continue;
                    }

                    //po might already be in its pool.
                    //this can happen when a poolableObject gets Destroyed (not immediate) and is already set to pool in this frame
                    //but then the parent gets DestroyedImmediate in the same frame - then the po is still in the hierarchy but
                    //already in pool. In that case we only have to move the element to its pool-parent now immediately but not
                    //into pool again (_PutIntoPool checks internally anyway but then it also produces a warning)
                    if ( !po._isInPool )
                        po._PutIntoPool();

                    //only when destroyImmediate we actually reparent the child poolableObject to the pool-parent
                    if ( destroyImmediate )
                    {
                        po._MoveToPoolParent();
                    }
                    else
                    {
                        if ( lateReturnList == null )
                        {
                            if ( !PoolableObjectLateReturnToPoolListPool.GetObject( out lateReturnList ) )
                                Debug.LogWarning( "PoolableObjectLateReturnToPoolListPool is empty. Object got created without pooling. Consider increasing pool size" );

#if !CS_ESSENTIALS_ASSETSTORE
                            if ( !AppRestarter.Restarting )
#endif
                                LateReturnToPoolQueue.Enqueue( lateReturnList );
                        }

                        po._markedForLateReturnToPoolParent = true;

                        lateReturnList.Add( po );
                    }
                }

                //po is one of the following
                //  case 1: null (no valid PoolableObject script found on self or childs)
                //  case 2: not null and po.transform == transform -> root object to destroy is poolable itself
                //  case 3: not null and po.transform != transform -> root object is a "normal" gameObject to be destroyed

                if ( destroyImmediate )
                {
                    // case 1 || case 3
                    if ( po == null || po.transform != transform )
                        GameObject.DestroyImmediate( transform.gameObject );

                    // case 2 - nothing left to do since when destroyImmediate the code above should've already moved all objects to their pool

                    return true;
                }

                //when lateReturnList is null here means that no valid (.IsPooledInstance) PoolableObject was found in the whole destroy-hierarchy
                if ( lateReturnList == null )
                {
                    // case 1
                    GameObject.Destroy( transform.gameObject );
                    return false;
                }

                //when here -> lateReturnList must NOT be null

                //  case 3
                if ( po.transform != transform )
#if !CS_ESSENTIALS_ASSETSTORE
                    if ( !AppRestarter.Restarting )
#endif
                        LateDestroyObjectsQueue.Enqueue( transform.gameObject );

                return false;
            }
            finally
            {
                PoolableObjectListPool.ReturnObject( poolableChildren );
            }
        }
    }
}