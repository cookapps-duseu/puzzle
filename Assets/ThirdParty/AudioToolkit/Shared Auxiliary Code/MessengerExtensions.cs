using UnityEngine;
using System;
using System.Reflection;

namespace MessengerExtensions
{
    /// <summary>
    /// Broadcast messages between objects and components, including inactive ones (which Unity doesn't do)
    /// </summary>
    public static class MessengerThatIncludesInactiveElements
    {
        /// <summary>
        /// Determine if the object has the given method and invoke it with parameters
        /// </summary>
        private static void InvokeIfExists( this object objectToCheck, string methodName, params object[] parameters )
        {
            var methods = ReflectionExtensions.GetMethods(objectToCheck.GetType(), methodName, true);

            MethodInfo methodToInvoke = null;

            foreach ( var advancedMethod in methods )
            {
                var method = advancedMethod.method;

                if ( MethodMatches( method, parameters ) )
                {
                    methodToInvoke = method;
                    break;
                }
            }

            if ( methodToInvoke == null )
                return;

            methodToInvoke.Invoke( objectToCheck, parameters );
        }

        private static bool MethodMatches( MethodInfo method, object[] parameters )
        {
            var methodParams = method.GetParameters();

            if ( methodParams.Length != parameters.Length )
                return false;

            for ( int i = 0; i < methodParams.Length; i++ )
            {
                var paramType = methodParams[i].ParameterType;
                var arg = parameters[i];

                if ( arg == null )
                {
                    // Null can be assigned to reference types or nullable value types
                    if ( paramType.IsValueType && Nullable.GetUnderlyingType( paramType ) == null )
                        return false;
                }
                else
                {
                    if ( !paramType.IsAssignableFrom( arg.GetType() ) )
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determine if the object has the given method without parameters
        /// </summary>
        private static void InvokeIfExists( this object objectToCheck, string methodName )
        {
            var methodInfo = ReflectionExtensions.GetMethod(objectToCheck.GetType(), methodName, true).method;

            if ( methodInfo == null )
                return;

            methodInfo.Invoke( objectToCheck, null );
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object, even if they are inactive
        /// </summary>
        public static void InvokeMethod( this GameObject gameobject, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null, params object[] parameters )
        {
            MonoBehaviour[] components = gameobject.GetComponents<MonoBehaviour> ();

            for ( int i = 0; i < components.Length; i++ )
            {
                var m = components[i];

                if ( predicate != null && !predicate.Invoke( m ) )
                    continue;

                if ( includeInactive || m.isActiveAndEnabled )
                    m.InvokeIfExists( methodName, parameters );
            }
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object, even if they are inactive
        /// </summary>
        public static void InvokeMethod( this GameObject gameobject, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null )
        {
            MonoBehaviour[] components = gameobject.GetComponents<MonoBehaviour> ();

            for ( int i = 0; i < components.Length; i++ )
            {
                var m = components[i];

                if ( predicate != null && !predicate.Invoke( m ) )
                    continue;

                if ( includeInactive || m.isActiveAndEnabled )
                    m.InvokeIfExists( methodName );
            }
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object, even if they are inactive
        /// </summary>
        public static void InvokeMethod( this Component component, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null, params object[] parameters )
        {
            component.gameObject.InvokeMethod( methodName, includeInactive, predicate, parameters );
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object, even if they are inactive
        /// </summary>
        public static void InvokeMethod( this Component component, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null )
        {
            component.gameObject.InvokeMethod( methodName, includeInactive, predicate );
        }


        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its children, even if they are inactive
        /// </summary>
        public static void InvokeMethodInChildren( this GameObject gameobject, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null, params object[] parameters )
        {
            MonoBehaviour[] components = gameobject.GetComponentsInChildren<MonoBehaviour> ( includeInactive );

            for ( int i = 0; i < components.Length; i++ )
            {
                var m = components[i];

                if ( predicate != null && !predicate.Invoke( m ) )
                    continue;

                if ( includeInactive || m.isActiveAndEnabled )
                    m.InvokeIfExists( methodName, parameters );
            }
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its children, even if they are inactive
        /// </summary>
        public static void InvokeMethodInChildren( this GameObject gameobject, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null )
        {
            MonoBehaviour[] components = gameobject.GetComponentsInChildren<MonoBehaviour> ( includeInactive );

            for ( int i = 0; i < components.Length; i++ )
            {
                var m = components[i];

                if ( predicate != null && !predicate.Invoke( m ) )
                    continue;

                if ( includeInactive || m.isActiveAndEnabled )
                    m.InvokeIfExists( methodName );
            }
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its children, even if they are inactive
        /// </summary>
        public static void InvokeMethodInChildren( this Component component, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null, params object[] parameters )
        {
            component.gameObject.InvokeMethodInChildren( methodName, includeInactive, predicate, parameters );
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its children, even if they are inactive
        /// </summary>
        public static void InvokeMethodInChildren( this Component component, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null )
        {
            component.gameObject.InvokeMethodInChildren( methodName, includeInactive, predicate );
        }

        /// <summary>
        /// Invoke the method if it exists in any component of the game object and its ancestors, even if they are inactive
        /// </summary>
        public static void SendMessageUpwardsToAll( this GameObject gameobject, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null, params object[] parameters )
        {
            Transform tranform = gameobject.transform;

            while ( tranform != null )
            {
                tranform.gameObject.InvokeMethod( methodName, includeInactive, predicate, parameters );
                tranform = tranform.parent;
            }
        }
        /// <summary>
        /// Invoke the method if it exists in any component of the component's game object and its ancestors, even if they are inactive
        /// </summary>
        public static void SendMessageUpwardsToAll( this Component component, string methodName, bool includeInactive, Predicate<MonoBehaviour> predicate = null, params object[] parameters )
        {
            component.gameObject.SendMessageUpwardsToAll( methodName, includeInactive, predicate, parameters );
        }
    }
}