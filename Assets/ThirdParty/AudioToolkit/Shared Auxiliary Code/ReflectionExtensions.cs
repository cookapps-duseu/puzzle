using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Concurrent;
using CS.Essentials;

public static class ReflectionExtensions
{
    public struct AdvancedFieldInfo
    {
        public FieldInfo field;
        public bool inherited;
        public bool Valid => field != null;
    }

    public struct AdvancedPropertyInfo
    {
        public PropertyInfo property;
        public bool inherited;
        public bool Valid => property != null;
    }

    public struct AdvancedMethodInfo
    {
        public MethodInfo method;
        public bool inherited;
        public bool Valid => method != null;
    }

    public struct FieldAttributePair<T> where T : Attribute
    {
        public FieldInfo field;
        public bool inherited;
        public T attribute;
    }

    public struct PropertyAttributePair<T> where T : Attribute
    {
        public PropertyInfo property;
        public bool inherited;
        public T attribute;
    }

    public struct MethodAttributePair<T> where T : Attribute
    {
        public MethodInfo method;
        public bool inherited;
        public T attribute;
    }

    const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    // Cache key structs
    struct FieldCacheKey : IEquatable<FieldCacheKey>
    {
        public Type Type { get; }
        public string FieldName { get; }
        public bool Inheritance { get; }

        private readonly int _hashCode;

        public FieldCacheKey( Type type, string fieldName, bool inheritance )
        {
            Type = type;
            FieldName = fieldName;
            Inheritance = inheritance;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ ( FieldName?.GetHashCode() ?? 0 );
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( FieldCacheKey other ) =>
            Type == other.Type &&
            FieldName == other.FieldName &&
            Inheritance == other.Inheritance;

        public override bool Equals( object obj ) => obj is FieldCacheKey other && Equals( other );
    }

    struct FieldsCacheKey : IEquatable<FieldsCacheKey>
    {
        public Type Type { get; }
        public bool Inheritance { get; }

        private readonly int _hashCode;

        public FieldsCacheKey( Type type, bool inheritance )
        {
            Type = type;
            Inheritance = inheritance;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( FieldsCacheKey other ) =>
            Type == other.Type &&
            Inheritance == other.Inheritance;

        public override bool Equals( object obj ) => obj is FieldsCacheKey other && Equals( other );
    }

    struct FieldsWithAttributeCacheKey : IEquatable<FieldsWithAttributeCacheKey>
    {
        public Type Type { get; }
        public bool Inheritance { get; }
        public Type AttributeType { get; }

        private readonly int _hashCode;

        public FieldsWithAttributeCacheKey( Type type, bool inheritance, Type attributeType )
        {
            Type = type;
            Inheritance = inheritance;
            AttributeType = attributeType;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            hash = ( hash * 397 ) ^ ( AttributeType?.GetHashCode() ?? 0 );
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( FieldsWithAttributeCacheKey other ) =>
            Type == other.Type &&
            Inheritance == other.Inheritance &&
            AttributeType == other.AttributeType;

        public override bool Equals( object obj ) => obj is FieldsWithAttributeCacheKey other && Equals( other );
    }

    struct PropertyCacheKey : IEquatable<PropertyCacheKey>
    {
        public Type Type { get; }
        public string PropertyName { get; }
        public bool Inheritance { get; }

        private readonly int _hashCode;

        public PropertyCacheKey( Type type, string propertyName, bool inheritance )
        {
            Type = type;
            PropertyName = propertyName;
            Inheritance = inheritance;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ ( PropertyName?.GetHashCode() ?? 0 );
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( PropertyCacheKey other ) =>
            Type == other.Type &&
            PropertyName == other.PropertyName &&
            Inheritance == other.Inheritance;

        public override bool Equals( object obj ) => obj is PropertyCacheKey other && Equals( other );
    }

    struct PropertiesCacheKey : IEquatable<PropertiesCacheKey>
    {
        public Type Type { get; }
        public bool Inheritance { get; }

        private readonly int _hashCode;

        public PropertiesCacheKey( Type type, bool inheritance )
        {
            Type = type;
            Inheritance = inheritance;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( PropertiesCacheKey other ) =>
            Type == other.Type &&
            Inheritance == other.Inheritance;

        public override bool Equals( object obj ) => obj is PropertiesCacheKey other && Equals( other );
    }

    struct PropertiesWithAttributeCacheKey : IEquatable<PropertiesWithAttributeCacheKey>
    {
        public Type Type { get; }
        public bool Inheritance { get; }
        public Type AttributeType { get; }

        private readonly int _hashCode;

        public PropertiesWithAttributeCacheKey( Type type, bool inheritance, Type attributeType )
        {
            Type = type;
            Inheritance = inheritance;
            AttributeType = attributeType;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            hash = ( hash * 397 ) ^ ( AttributeType?.GetHashCode() ?? 0 );
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( PropertiesWithAttributeCacheKey other ) =>
            Type == other.Type &&
            Inheritance == other.Inheritance &&
            AttributeType == other.AttributeType;

        public override bool Equals( object obj ) => obj is PropertiesWithAttributeCacheKey other && Equals( other );
    }

    struct MethodCacheKey : IEquatable<MethodCacheKey>
    {
        public Type Type { get; }
        public string MethodName { get; }
        public Type[] ParameterTypes { get; }
        public bool Inheritance { get; }

        private readonly int _hashCode;

        public MethodCacheKey( Type type, string methodName, Type[] parameterTypes, bool inheritance )
        {
            Type = type;
            MethodName = methodName;
            ParameterTypes = parameterTypes ?? Array.Empty<Type>();
            Inheritance = inheritance;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            unchecked
            {
                int hash = Type.GetHashCode();
                hash = ( hash * 397 ) ^ ( MethodName?.GetHashCode() ?? 0 );
                hash = ( hash * 397 ) ^ Inheritance.GetHashCode();

                foreach ( var paramType in ParameterTypes )
                {
                    hash = ( hash * 397 ) ^ ( paramType?.GetHashCode() ?? 0 );
                }

                return hash;
            }
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( MethodCacheKey other )
        {
            if ( !Type.Equals( other.Type ) ) return false;
            if ( !string.Equals( MethodName, other.MethodName ) ) return false;
            if ( Inheritance != other.Inheritance ) return false;
            if ( ParameterTypes.Length != other.ParameterTypes.Length ) return false;
            for ( int i = 0; i < ParameterTypes.Length; i++ )
            {
                if ( !ParameterTypes[ i ].Equals( other.ParameterTypes[ i ] ) ) return false;
            }
            return true;
        }

        public override bool Equals( object obj ) => obj is MethodCacheKey other && Equals( other );
    }

    struct MethodsCacheKey : IEquatable<MethodsCacheKey>
    {
        public Type Type { get; }
        public string MethodName { get; } // Can be null
        public bool Inheritance { get; }

        private readonly int _hashCode;

        public MethodsCacheKey( Type type, string methodName, bool inheritance )
        {
            Type = type;
            MethodName = methodName;
            Inheritance = inheritance;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            int hash = Type.GetHashCode();
            hash = ( hash * 397 ) ^ ( MethodName?.GetHashCode() ?? 0 );
            hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
            return hash;
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( MethodsCacheKey other ) =>
            Type == other.Type &&
            MethodName == other.MethodName &&
            Inheritance == other.Inheritance;

        public override bool Equals( object obj ) => obj is MethodsCacheKey other && Equals( other );
    }

    struct MethodsWithAttributeCacheKey : IEquatable<MethodsWithAttributeCacheKey>
    {
        public Type Type { get; }
        public bool Inheritance { get; }
        public Type AttributeType { get; }

        private readonly int _hashCode;

        public MethodsWithAttributeCacheKey( Type type, bool inheritance, Type attributeType )
        {
            Type = type;
            Inheritance = inheritance;
            AttributeType = attributeType;
            _hashCode = 0;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            unchecked
            {
                int hash = Type.GetHashCode();
                hash = ( hash * 397 ) ^ Inheritance.GetHashCode();
                hash = ( hash * 397 ) ^ ( AttributeType?.GetHashCode() ?? 0 );
                return hash;
            }
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals( MethodsWithAttributeCacheKey other ) =>
            Type == other.Type &&
            Inheritance == other.Inheritance &&
            AttributeType == other.AttributeType;

        public override bool Equals( object obj ) => obj is MethodsWithAttributeCacheKey other && Equals( other );
    }

    // Caches with new key types
    private static readonly ConcurrentDictionary<FieldCacheKey, AdvancedFieldInfo> _fieldCache = new ConcurrentDictionary<FieldCacheKey, AdvancedFieldInfo>();
    private static readonly ConcurrentDictionary<FieldsCacheKey, IEnumerable<AdvancedFieldInfo>> _fieldsCache = new ConcurrentDictionary<FieldsCacheKey, IEnumerable<AdvancedFieldInfo>>();

    private static readonly ConcurrentDictionary<FieldsWithAttributeCacheKey, IEnumerable<object>> _fieldsWithAttributeCache = new ConcurrentDictionary<FieldsWithAttributeCacheKey, IEnumerable<object>>();

    private static readonly ConcurrentDictionary<PropertyCacheKey, AdvancedPropertyInfo> _propertyCache = new ConcurrentDictionary<PropertyCacheKey, AdvancedPropertyInfo>();
    private static readonly ConcurrentDictionary<PropertiesCacheKey, IEnumerable<AdvancedPropertyInfo>> _propertiesCache = new ConcurrentDictionary<PropertiesCacheKey, IEnumerable<AdvancedPropertyInfo>>();

    private static readonly ConcurrentDictionary<PropertiesWithAttributeCacheKey, IEnumerable<object>> _propertiesWithAttributeCache = new ConcurrentDictionary<PropertiesWithAttributeCacheKey, IEnumerable<object>>();

    private static readonly ConcurrentDictionary<MethodCacheKey, AdvancedMethodInfo> _methodCache = new ConcurrentDictionary<MethodCacheKey, AdvancedMethodInfo>();
    private static readonly ConcurrentDictionary<MethodsCacheKey, IEnumerable<AdvancedMethodInfo>> _methodsCache = new ConcurrentDictionary<MethodsCacheKey, IEnumerable<AdvancedMethodInfo>>();

    private static readonly ConcurrentDictionary<MethodsWithAttributeCacheKey, IEnumerable<object>> _methodsWithAttributeCache = new ConcurrentDictionary<MethodsWithAttributeCacheKey, IEnumerable<object>>();

    // Updated methods

    public static AdvancedFieldInfo GetField( this Type type, string fieldName, bool inheritance = true )
    {
        if ( type == null )
            return default;

        var key = new FieldCacheKey(type, fieldName, inheritance);

        if ( _fieldCache.TryGetValue( key, out var cachedField ) )
            return cachedField;

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetField {fieldName.Quoted()} ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var field = type.GetField(fieldName, bindingFlags);

            if ( field == null && inheritance )
            {
                var baseField = GetField(type.BaseType, fieldName, inheritance);
                _fieldCache[ key ] = baseField;
                return baseField;
            }

            var fieldInfo = new AdvancedFieldInfo { field = field, inherited = false };
            _fieldCache[ key ] = fieldInfo;

            return fieldInfo;
        }
    }

    public static IEnumerable<AdvancedFieldInfo> GetFields( this Type type, bool inheritance = true )
    {
        if ( type == null )
            return Enumerable.Empty<AdvancedFieldInfo>();

        var key = new FieldsCacheKey(type, inheritance);

        if ( _fieldsCache.TryGetValue( key, out var cachedFields ) )
            return cachedFields;

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetFields ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var fields = new List<AdvancedFieldInfo>();
            Type currentType = type;
            bool currentInherited = false;

            while ( currentType != null )
            {
                var typeFields = currentType.GetFields(bindingFlags);

                foreach ( var field in typeFields )
                    fields.Add( new AdvancedFieldInfo { field = field, inherited = currentInherited } );

                if ( !inheritance )
                    break;

                currentType = currentType.BaseType;
                currentInherited = true;
            }

            _fieldsCache[ key ] = fields;

            return fields;
        }
    }

    public static IEnumerable<FieldAttributePair<T>> GetFieldsWithAttribute<T>( this Type type, bool inheritance = true ) where T : Attribute
    {
        if ( type == null )
            return Enumerable.Empty<FieldAttributePair<T>>();

        var key = new FieldsWithAttributeCacheKey(type, inheritance, typeof(T));

        if ( _fieldsWithAttributeCache.TryGetValue( key, out var cachedFields ) )
            return cachedFields.Cast<FieldAttributePair<T>>();

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetFieldsWithAttribute ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var fields = new List<FieldAttributePair<T>>();
            Type currentType = type;
            bool currentInherited = false;

            while ( currentType != null )
            {
                var typeFields = currentType.GetFields(bindingFlags);

                foreach ( var field in typeFields )
                {
                    var attribute = field.GetCustomAttribute<T>(false);

                    if ( attribute != null )
                        fields.Add( new FieldAttributePair<T> { field = field, inherited = currentInherited, attribute = attribute } );
                }

                if ( !inheritance )
                    break;

                currentType = currentType.BaseType;
                currentInherited = true;
            }

            _fieldsWithAttributeCache[ key ] = fields.Cast<object>().ToList();

            return fields;
        }
    }

    public static AdvancedPropertyInfo GetProperty( this Type type, string propertyName, bool inheritance = true )
    {
        if ( type == null )
            return default;

        var key = new PropertyCacheKey(type, propertyName, inheritance);

        if ( _propertyCache.TryGetValue( key, out var cachedProperty ) )
            return cachedProperty;
            
#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetProperty {propertyName.Quoted()} ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var property = type.GetProperty(propertyName, bindingFlags);

            if ( property == null && inheritance )
            {
                var baseProperty = GetProperty(type.BaseType, propertyName, inheritance);
                _propertyCache[ key ] = baseProperty;
                return baseProperty;
            }

            var propertyInfo = new AdvancedPropertyInfo { property = property, inherited = false };
            _propertyCache[ key ] = propertyInfo;

            return propertyInfo;
        }
    }

    public static IEnumerable<AdvancedPropertyInfo> GetProperties( this Type type, bool inheritance = true )
    {
        if ( type == null )
            return Enumerable.Empty<AdvancedPropertyInfo>();

        var key = new PropertiesCacheKey(type, inheritance);

        if ( _propertiesCache.TryGetValue( key, out var cachedProperties ) )
            return cachedProperties;

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetProperties ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var properties = new List<AdvancedPropertyInfo>();
            Type currentType = type;
            bool currentInherited = false;

            while ( currentType != null )
            {
                var typeProperties = currentType.GetProperties(bindingFlags);

                foreach ( var property in typeProperties )
                    properties.Add( new AdvancedPropertyInfo { property = property, inherited = currentInherited } );

                if ( !inheritance )
                    break;

                currentType = currentType.BaseType;
                currentInherited = true;
            }

            _propertiesCache[ key ] = properties;

            return properties;
        }
    }

    public static IEnumerable<PropertyAttributePair<T>> GetPropertiesWithAttribute<T>( this Type type, bool inheritance = true ) where T : Attribute
    {
        if ( type == null )
            return Enumerable.Empty<PropertyAttributePair<T>>();

        var key = new PropertiesWithAttributeCacheKey(type, inheritance, typeof(T));

        if ( _propertiesWithAttributeCache.TryGetValue( key, out var cachedProperties ) )
            return cachedProperties.Cast<PropertyAttributePair<T>>();

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetPropertiesWithAttribute ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var properties = new List<PropertyAttributePair<T>>();
            Type currentType = type;
            bool currentInherited = false;

            while ( currentType != null )
            {
                var typeProperties = currentType.GetProperties(bindingFlags);

                foreach ( var property in typeProperties )
                {
                    var attribute = property.GetCustomAttribute<T>(false);

                    if ( attribute != null )
                        properties.Add( new PropertyAttributePair<T> { property = property, inherited = currentInherited, attribute = attribute } );
                }

                if ( !inheritance )
                    break;

                currentType = currentType.BaseType;
                currentInherited = true;
            }

            _propertiesWithAttributeCache[ key ] = properties.Cast<object>().ToList();

            return properties;
        }
    }

    public static AdvancedMethodInfo GetMethod( this Type type, string methodName, bool inheritance = true )
    {
        return GetMethodInternal( type, methodName, null, false, inheritance );
    }

    public static AdvancedMethodInfo GetMethod( this Type type, string methodName, Type[] parameterTypes, bool inheritance = true )
    {
        return GetMethodInternal( type, methodName, parameterTypes, false, inheritance );
    }

    private static AdvancedMethodInfo GetMethodInternal( this Type type, string methodName, Type[] parameterTypes, bool inherited, bool inheritance )
    {
        if ( type == null )
            return default;

        var key = new MethodCacheKey(type, methodName, parameterTypes, inheritance);

        if ( _methodCache.TryGetValue( key, out var cachedMethod ) )
            return cachedMethod;

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetMethod {methodName.Quoted()} ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var method = parameterTypes == null
                ? type.GetMethod(methodName, bindingFlags)
                : type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);

            if ( method == null && inheritance )
            {
                var baseMethod = GetMethodInternal(type.BaseType, methodName, parameterTypes, true, inheritance);
                _methodCache[ key ] = baseMethod;
                return baseMethod;
            }

            var methodInfo = new AdvancedMethodInfo { method = method, inherited = inherited };
            _methodCache[ key ] = methodInfo;

            return methodInfo;
        }
    }

    public static IEnumerable<AdvancedMethodInfo> GetMethods( this Type type, string name, bool inheritance = true )
    {
        if ( type == null )
            return Enumerable.Empty<AdvancedMethodInfo>();

        var key = new MethodsCacheKey(type, name, inheritance);

        if ( _methodsCache.TryGetValue( key, out var cachedMethods ) )
            return cachedMethods;

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetMethods ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var methods = new List<AdvancedMethodInfo>();
            Type currentType = type;
            bool currentInherited = false;

            while ( currentType != null )
            {
                var typeMethods = currentType.GetMethods(bindingFlags);

                if ( !string.IsNullOrEmpty( name ) )
                {
                    typeMethods = typeMethods.Where( m => m.Name == name ).ToArray();
                }

                foreach ( var method in typeMethods )
                    methods.Add( new AdvancedMethodInfo { method = method, inherited = currentInherited } );

                if ( !inheritance )
                    break;

                currentType = currentType.BaseType;
                currentInherited = true;
            }

            _methodsCache[ key ] = methods;

            return methods;
        }
    }

    public static IEnumerable<AdvancedMethodInfo> GetMethods( this Type type, bool inheritance = true )
    {
        return GetMethods( type, null, inheritance );
    }

    public static IEnumerable<MethodAttributePair<T>> GetMethodsWithAttribute<T>( this Type type, bool inheritance = true ) where T : Attribute
    {
        if ( type == null )
            return Enumerable.Empty<MethodAttributePair<T>>();

        var key = new MethodsWithAttributeCacheKey(type, inheritance, typeof(T));

        if ( _methodsWithAttributeCache.TryGetValue( key, out var cachedMethods ) )
            return cachedMethods.Cast<MethodAttributePair<T>>();

#if !CS_ESSENTIALS_ASSETSTORE
        using ( ClockStoneProfiling.Profile( $"[ReflectionExtensions] GetMethodsWithAttribute ({type.AssemblyQualifiedName})" ) )
#endif
        {
            var methods = new List<MethodAttributePair<T>>();
            Type currentType = type;
            bool currentInherited = false;

            while ( currentType != null )
            {
                var typeMethods = currentType.GetMethods(bindingFlags);

                foreach ( var method in typeMethods )
                {
                    var attribute = method.GetCustomAttribute<T>(false);

                    if ( attribute != null )
                        methods.Add( new MethodAttributePair<T> { method = method, inherited = currentInherited, attribute = attribute } );
                }

                if ( !inheritance )
                    break;

                currentType = currentType.BaseType;
                currentInherited = true;
            }

            _methodsWithAttributeCache[ key ] = methods.Cast<object>().ToList();

            return methods;
        }
    }
}
