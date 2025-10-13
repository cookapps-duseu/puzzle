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

#if !UNITY_2023_1_OR_NEWER
#define POOLABLEOBJECT_LEGACYEXECUTIONORDER
#endif

using MessengerExtensions;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 1591 // undocumented XML code warning

namespace CS.Essentials
{
    /// <summary>
    /// Add this component to your prefab to make it poolable. 
    /// </summary>
    /// <remarks>
    /// See <see cref="ObjectPoolController"/> for an explanation how to set up a prefab for pooling.
    /// The following messages are sent to a poolable object:
    /// <list type="bullet">
    /// <item> 
    ///   <c>Awake()</c>, <c>Start()</c> and <c>OnDestroy()</c> whenever a poolable object is activated 
    ///   or deactivated from the pool.
    ///   This way the same behaviour is simulated as if the object was instantiated respectively destroyed.
    ///   These messages are only sent when <see cref="sendAwakeStartOnDestroyMessage"/> is enabled.
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="ObjectPoolController"/>
    [AddComponentMenu( "ClockStone/PoolableObject" )]
    //to call OnEnable early before all other OnEnable so our manualy Awake OnDestroy callbacks get called before any OnEnable
    [DefaultExecutionOrder( -32100 )]
    public class PoolableObject : MonoBehaviour
    {
        [Tooltip("Specifies the maximum number of objects on the pool")]
        /// <summary>
        /// The maximum number of instances of this prefab to get stored in the pool.
        /// </summary>
        public int maxPoolSize = 10;

        [Tooltip("Specifies the number of objects that will be created on the pool at program start (improves speed of later instantiation)")]
        /// <summary>
        /// This number of instances will be preloaded to the pool if <see cref="ObjectPoolController.Preload(GameObject)"/> is called.
        /// </summary>
        public int preloadCount = 0;

        [Tooltip("If enabled the pool of deactivated objects will surivive a scene change")]
        /// <summary>
        /// If enabled the object will not get destroyed if a new scene is loaded
        /// </summary>
        public bool doNotDestroyOnLoad = false;

        /// <summary>
        /// If enabled Awake(), Start(), and OnDestroy() messages are sent to the poolable object if the object is set 
        /// active respectively inactive whenever <see cref="ObjectPoolController.Destroy(GameObject)"/> or 
        /// <see cref="ObjectPoolController.Instantiate(GameObject)"/> is called. <para/>
        /// This way it is simulated that the object really gets instantiated respectively destroyed.
        /// </summary>
        /// <remarks>
        /// The Start() function is called immedialtely after Awake() by <see cref="ObjectPoolController.Instantiate(GameObject)"/> 
        /// and not next frame. So do not set data after <see cref="ObjectPoolController.Instantiate(GameObject)"/> that Start()
        /// relies on. In some cases you may not want the  Awake(), Start(), and OnDestroy() messages to be sent for performance 
        /// reasons because it may not be necessary to fully reinitialize a game object each time it is activated from the pool.
        /// </remarks>
        public bool sendAwakeStartOnDestroyMessage = true;

#if POOLABLEOBJECT_LEGACYEXECUTIONORDER
        public bool sendAwakeToInactive = false;

        //needed when an object gets instantiated deactivated to prevent double awake
        internal bool _awakeJustCalledByUnity = false;
#endif

        public bool IsPooledInstance => _pool != null;

        /// <summary>
        /// if null - Object was not created from ObjectPoolController
        /// </summary>
        internal ObjectPoolController.ObjectPool _pool = null;

        internal bool _isInPool = false;
        internal bool _markedForLateReturnToPoolParent = false;

        internal int _serialNumber = 0;
        internal int _usageCount = 0;

        private bool _justInvokingOnDestroy = false;

#if POOLABLEOBJECT_LEGACYEXECUTIONORDER
        private bool _objectWasJustActiveBeforeOnDestroy = false;
        private bool _handleManualCallbacksInOnEnable = false;
#else
        private bool _callAwakeInOnEnable = false;
        private bool _scheduleStartInOnEnable = false;
#endif

#if UNITY_EDITOR
        internal bool _wasInstantiatedByObjectPoolController = false;
#endif

        [Flags]
        private enum EMessageTypes
        {
            Awake = 1 << 0,
            Start = 1 << 2,
        }

#if UNITY_EDITOR || POOLABLEOBJECT_LEGACYEXECUTIONORDER
        protected void Awake()
        {
#if POOLABLEOBJECT_LEGACYEXECUTIONORDER
            _awakeJustCalledByUnity = true;
#endif

#if UNITY_EDITOR
            if ( !_wasInstantiatedByObjectPoolController && !IsPooledInstance && !ObjectPoolController.IsDuringInstantiate )
                Debug.LogWarning( "Poolable object " + name + " was instantiated without ObjectPoolController", gameObject );
#endif
        }
#endif

        protected void OnEnable()
        {
            if ( _isInPool )
            {
                //Objects must not be set enabled when in pool
                //But maybe the object is not yet returned to pool parent when another script tries to enable it in it's old location
                //in that case we just leave the object disabled and prevent the warning log.

                if ( !_markedForLateReturnToPoolParent )
                    Debug.LogWarning( "Poolable object " + name + " got enabled while in pool", gameObject );

                gameObject.SetActive( false );

                return;
            }

            HandleOnEnableCallbacks();
        }

        protected void OnDestroy()
        {
            //only if destroy message comes from unity and not from invocation
            if ( !_justInvokingOnDestroy && _pool != null )
            {
                // Poolable object was destroyed by using the default Unity Destroy() function -> Use ObjectPoolController.Destroy() instead
                // This can also happen if objects are automatically deleted by Unity e.g. due to level change or if an object is parented to an object that gets destroyed
                _pool.Remove( this );
            }
        }

        /// <summary>
        /// Gets the object's pool serial number. Each object has a unique serial number. Can be useful for debugging purposes.
        /// </summary>
        /// <returns>
        /// The serial number (starting with 1 for each pool). Each new instance receives a unique serial number
        /// </returns>
        public int GetSerialNumber() => _serialNumber;

        /// <summary>
        /// Gets the usage counter which gets increased each time an object is re-used from the pool.
        /// </summary>
        /// <returns>
        /// The usage counter
        /// </returns>
        public int GetUsageCount() => _usageCount;

        /// <summary>
        /// Checks if the object is deactivated and in the pool.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object is in the pool of deactivated objects, otherwise <c>false</c>.
        /// </returns>
        public bool IsDeactivated() => _isInPool;

        /// <summary>
        /// Moves all poolable objects of this kind (instantiated from the same prefab as this instance) back to the pool. 
        /// </summary>
        /// <returns>
        /// The number of instances deactivated and moved back to its pool.
        /// </returns>
        public int DeactivateAllPoolableObjectsOfMyKind()
        {
            if ( _pool != null )
                return _pool._SetAllAvailable( false );

            return 0;
        }

        internal void _PutIntoPool()
        {
            if ( ObjectPoolController.ApplicationQuitting )
                return;

            if ( _pool == null )
            {
                Debug.LogError( "Tried to put object into pool which was not created with ObjectPoolController", this );
                return;
            }

            if ( _isInPool )
            {
                Debug.LogWarning( "Object is already in Pool", this );
                return;
            }

#if POOLABLEOBJECT_LEGACYEXECUTIONORDER
            _objectWasJustActiveBeforeOnDestroy = isActiveAndEnabled;
#endif

            gameObject.SetActive( false );

            //dont fire callbacks when object is put into pool initially
            if ( !ObjectPoolController.IsDuringInstantiate )
            {
                if ( sendAwakeStartOnDestroyMessage )
                {
                    _justInvokingOnDestroy = true;

                    gameObject.InvokeMethodInChildren( "OnDestroy", true, WasAwakeCalledByUnity );

                    _justInvokingOnDestroy = false;
                }
            }

#if POOLABLEOBJECT_LEGACYEXECUTIONORDER
            _objectWasJustActiveBeforeOnDestroy = false;
#endif

            _isInPool = true;
        }

        internal void _MoveToPoolParent()
        {
            if ( _pool == null )
            {
                Debug.LogError( "Tried to move object which was not created with ObjectPoolController to Pool-Parent ", this );
                return;
            }

            if ( transform.parent == _pool.poolParent )
                return;

            transform.SetParent( _pool.poolParent, true );

            _markedForLateReturnToPoolParent = false;
        }

        internal void TakeFromPool( Transform parent, bool activateObject )
        {
            if ( !_isInPool )
            {
                Debug.LogError( "Tried to take an object from Pool which is not available!", this );
                return;
            }

            _isInPool = false;

            _markedForLateReturnToPoolParent = false;

            _usageCount++;

            transform.SetParent( parent, true );

            if ( parent == null )
            {
                // make sure that the object is not in the DontDestroyOnLoadScene when taken from pool
                SceneManager.MoveGameObjectToScene( gameObject, SceneManager.GetActiveScene() );
            }

            HandleTakeFromPoolCallbacks( activateObject );
        }

#if POOLABLEOBJECT_LEGACYEXECUTIONORDER
        private void HandleOnEnableCallbacks()
        {
            if ( _handleManualCallbacksInOnEnable )
            {
                _handleManualCallbacksInOnEnable = false;

                //Pre Unity 2023 when _handleActivateMessagesInOnEnable is set we just call all 3 callbacks (if flags are set)
                //Awake, Start, Poolable.
                //Usually Unity calls Awake and OnEnable on all scripts before calling Start on any Script
                //here in pre Unity 2023 we Sent Awake and Start one after another which could lead to problems where scripts
                //e.g. checked .isActiveAndEnabled in the Start callback.
                InvokeCallbacks();
            }
        }

        // in old Unity versions we don't know
        bool WasAwakeCalledByUnity( MonoBehaviour behaviour ) => sendAwakeToInactive || behaviour.isActiveAndEnabled || _objectWasJustActiveBeforeOnDestroy;

        private void InvokeCallbacks()
        {
            if ( !sendAwakeStartOnDestroyMessage )
                return;

            if ( !_awakeJustCalledByUnity )
                gameObject.InvokeMethodInChildren( "Awake", true, WasAwakeCalledByUnity );

            if ( gameObject.activeInHierarchy ) // Awake could deactivate object
                gameObject.InvokeMethodInChildren( "Start", false );
        }

        private void HandleTakeFromPoolCallbacks( bool activateObject )
        {
            //this may be set to true when unity calls Awake after gameObject.SetActive(true);
            _awakeJustCalledByUnity = false;

            if ( !activateObject )
            {
                _handleManualCallbacksInOnEnable = true;
                return;
            }

            _handleManualCallbacksInOnEnable = false;

            //We set the gameObject active what might call a lot of OnEnable at that time
            gameObject.SetActive( true );

            //Afterwards we call Awake and OnEnable (if flag is set)
            //This behaviour is not as Unity does stuff (Awake, OnEnable, Start)
            //Unity 2023 onwards we fix this issue by handling the messages in OnEnable
            InvokeCallbacks();
        }
#else
        private void HandleOnEnableCallbacks()
        {
            //We call Awake now and register for Start callback so it gets called later (when unity usually calls start functions)

            if ( _callAwakeInOnEnable )
            {
                _callAwakeInOnEnable = false;

                InvokeCallbacks( EMessageTypes.Awake );
            }

            if ( _scheduleStartInOnEnable )
                ObjectPoolController.ObjectPoolController_StartSystem.NextStartCallback += ObjectPoolController_StartSystem_NextStartCallback;
        }

        private void ObjectPoolController_StartSystem_NextStartCallback()
        {
            //object got destroyed between registering and actual call? just return
            if ( this == null )
                return;

            //object got disabled between registering and actual call? return and leave _scheduleStartInOnEnable so "Start" can be called after next OnEnable
            if ( !this.isActiveAndEnabled )
                return;

            _scheduleStartInOnEnable = false;

            InvokeCallbacks( EMessageTypes.Start );
        }

        // didAwake is also true when the behaviour has no Awake() function.
        bool WasAwakeCalledByUnity( MonoBehaviour behaviour ) => behaviour.didAwake;
        // didStart is only true when the behaviour actually has a Start() function. Otherwise it's always false
        bool WasStartCalledByUnity( MonoBehaviour behaviour ) => behaviour.didStart;

        private void InvokeCallbacks( EMessageTypes messageTypes = EMessageTypes.Awake | EMessageTypes.Start )
        {
            if ( !sendAwakeStartOnDestroyMessage )
                return;

            if ( messageTypes.HasFlag( EMessageTypes.Awake ) )
                gameObject.InvokeMethodInChildren( "Awake", true, WasAwakeCalledByUnity );

            if ( gameObject.activeInHierarchy && messageTypes.HasFlag( EMessageTypes.Start ) ) // Awake could deactivate object
                gameObject.InvokeMethodInChildren( "Start", true, WasStartCalledByUnity );
        }

        private void HandleTakeFromPoolCallbacks( bool activateObject )
        {
            _callAwakeInOnEnable = true;
            _scheduleStartInOnEnable = true;

            if ( activateObject )
                gameObject.SetActive( true );
        }
#endif
    }
}