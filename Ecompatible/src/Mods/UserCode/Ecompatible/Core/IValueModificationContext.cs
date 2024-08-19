using Eco.Core.Utils;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ecompatible
{
    public interface IContext
    {
    }
    public class ContextKey
    {
        internal ContextKey(PropertyInfo property) : this(property.PropertyType, property.Name) { }
        internal ContextKey(Type dataType, string propertyName)
        {
            DataType = dataType;
            PropertyName = propertyName;
        }

        public Type DataType { get; }
        public string PropertyName { get; }

        public override bool Equals(object obj)
        {
            return obj is ContextKey key &&
                   EqualityComparer<Type>.Default.Equals(DataType, key.DataType) &&
                   PropertyName == key.PropertyName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DataType, PropertyName);
        }

        public static bool operator ==(ContextKey left, ContextKey right)
        {
            return EqualityComparer<ContextKey>.Default.Equals(left, right);
        }

        public static bool operator !=(ContextKey left, ContextKey right)
        {
            return !(left == right);
        }
    }
    public class ContextKey<T> : ContextKey
    {
        public ContextKey(string propertyName) : base(typeof(T), propertyName) { }
    }
    public partial class Context
    {
        public static IContext CreateContext(params (ContextKey, object)[] values)
        {
            return new GenericContext(values);
        }
    }
    public static class ContextExtensions
    {
        public static bool TryGet<T>(this IContext context, ContextKey key, out T value)
        {
            value = default;
            if (context == null) return false;
            if (context is GenericContext genericContext) return genericContext.TryGet(key, out value);
            return context.TryGetPropertyValueByName(key.PropertyName, out value);
        }
        /// <summary>
        /// Get the value of the property stored in the context if it is non-null
        /// </summary>
        /// <typeparam name="T">The output type, which the stored value must be assignable to</typeparam>
        /// <param name="context">/param>
        /// <param name="key">The key for the property</param>
        /// <param name="value">The value of the property</param>
        /// <returns>True if the property was found and has a non-null value of the required type</returns>
        public static bool TryGetNonNull<T>(this IContext context, ContextKey key, out T value)
        {
            bool hasProperty = TryGet(context, key, out value);
            return hasProperty && value != null;
        }
    }
    /// <summary>
    /// Resolver context that can store data against property-like keys, essentially a dictionary of properties to values.
    /// Allows context data to be determined at run-time instead of actual interface properties at compile-time.
    /// This means in code where create contexts are created (to resolve values) it doesn't need to populate any information it doesn't have, which specific classes could require.
    /// </summary>
    internal class GenericContext : IContext
    {
        private IDictionary<ContextKey, object> Values { get; } = new ThreadSafeDictionary<ContextKey, object> ();
        public GenericContext(params (ContextKey, object)[] values)
        {
            foreach(var value in values)
            {
                TryAdd(value.Item1, value.Item2);
            }
        }
        public GenericContext(IEnumerable<KeyValuePair<ContextKey, object>> pseudoProperties)
        {
            foreach(KeyValuePair<ContextKey, object> property in pseudoProperties)
            {
                TryAdd(property.Key, property.Value);
            }
        }
        private bool TryAdd(ContextKey key, object value)
        {
            if (value == null && key.DataType.IsValueType) throw new InvalidCastException($"{value?.GetType()} is not assignable to {key.DataType}");
            return Values.TryAdd (key, value);
        }
        public bool TryGet<T>(ContextKey key, out T value)
        {
            if (Values.TryGetValue(key, out object v) && v is T)
            {
                value = (T)v;
                return true;
            }
            value = default;
            return false;
        }
        
        public static GenericContext ConvertContext(IContext context)
        {
            if (context is GenericContext dynamicContext) return dynamicContext;
            Dictionary<ContextKey, object> values = new Dictionary<ContextKey, object>();
            foreach(Type contextInterface in context.GetType().GetInterfaces().Where(@interface => @interface.DerivesFrom<IContext>()))
            {
                foreach(PropertyInfo property in contextInterface.GetProperties())
                {
                    object value = property.GetValue(context);
                    values.TryAdd(new ContextKey(property), value);
                }
            }
            return new GenericContext(values);
        }
    }
}
