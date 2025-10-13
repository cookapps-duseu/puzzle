using System.Linq;
using UnityEngine.LowLevel;

#if CS_ESSENTIALS_ASSETSTORE
using UnityEngine;
#endif

namespace CS.Essentials.PlayerLoopInjections
{
    public static class CSPlayerLoopExtensions
    {
        public static string GetPlayerLoopDebugLog( PlayerLoopSystem playerLoop, int indentationLevel, string message = "" )
        {
            string indentation = new string('\t', indentationLevel);

            foreach ( var subSystem in playerLoop.subSystemList )
            {
                message += $"{indentation}\t{subSystem.type.Name}\n";

                if ( subSystem.subSystemList != null && subSystem.subSystemList.Length > 0 )
                    message = GetPlayerLoopDebugLog( subSystem, indentationLevel + 1, message );
            }

            return message;
        }

        public static void InjectSubSystem( ref PlayerLoopSystem playerLoop, PlayerLoopSystem injectSystem, string[] subsystemParentPath, string preceedingSiblingSystemName, int pathIdx = 0 )
        {
            if ( subsystemParentPath == null || pathIdx >= subsystemParentPath.Length )
            {
                var l = playerLoop.subSystemList.ToList();

                int i = 0;

                if ( !string.IsNullOrWhiteSpace( preceedingSiblingSystemName ) )
                {
                    var length = playerLoop.subSystemList.Length;

                    for ( ; i < length; i++ )
                    {
                        if ( playerLoop.subSystemList[ i ].ToString() == preceedingSiblingSystemName )
                        {
                            i++;
                            break;
                        }
                    }
                }

                if ( i >= l.Count )
                    l.Add( injectSystem );
                else
                    l.Insert( i, injectSystem );

                playerLoop.subSystemList = l.ToArray();

                return;
            }
            else
            {
                var p = subsystemParentPath[pathIdx];
                var length = playerLoop.subSystemList.Length;

                for ( int i = 0; i < length; i++ )
                {
                    if ( playerLoop.subSystemList[ i ].ToString() == p )
                    {
                        InjectSubSystem( ref playerLoop.subSystemList[ i ], injectSystem, subsystemParentPath, preceedingSiblingSystemName, ++pathIdx );
                        return;
                    }
                }
#if CS_ESSENTIALS_ASSETSTORE
                Debug.LogError( $"Unable to inject '{injectSystem}' into PlayerLoopSystem. Can't find the following part of the path \"{p}\"" );
#else
                CSDebug.LogError( $"Unable to inject {injectSystem.Quoted()} into PlayerLoopSystem. Can't find the following part of the path \"{p}\"" );
#endif
            }
        }
    }
}