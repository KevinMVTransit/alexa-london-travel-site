// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A class used to fetch properties from objects using reflection. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Based on <c>https://github.com/Microsoft/ApplicationInsights-dotnet-server/blob/db53106c5d85546a2508ecb9dcd6bf1106fb29fa/Src/DependencyCollector/Shared/Implementation/PropertyFetcher.cs</c>.
    /// </remarks>
    internal sealed class PropertyFetcher
    {
        private readonly string _propertyName;
        private PropertyFetch _innerFetcher;

        internal PropertyFetcher(string propertyName)
        {
            _propertyName = propertyName;
        }

        internal object Fetch(object obj)
        {
            if (_innerFetcher == null)
            {
                var property = obj
                    .GetType()
                    .GetTypeInfo()
                    .GetDeclaredProperty(_propertyName);

                _innerFetcher = PropertyFetch.FetcherForProperty(property);
            }

            return _innerFetcher?.Fetch(obj);
        }

        private class PropertyFetch
        {
            internal static PropertyFetch FetcherForProperty(PropertyInfo propertyInfo)
            {
                if (propertyInfo == null)
                {
                    // returns null on any fetch.
                    return new PropertyFetch();
                }

                var typedPropertyFetcher = typeof(TypedFetchProperty<,>);

                var instantiatedTypedPropertyFetcher = typedPropertyFetcher
                    .GetTypeInfo()
                    .MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);

                return (PropertyFetch)Activator.CreateInstance(instantiatedTypedPropertyFetcher, propertyInfo);
            }

            internal virtual object Fetch(object obj)
            {
                return null;
            }

            private class TypedFetchProperty<TObject, TProperty> : PropertyFetch
            {
                private readonly Func<TObject, TProperty> _propertyFetch;

                internal TypedFetchProperty(PropertyInfo property)
                {
                    _propertyFetch = (Func<TObject, TProperty>)property.GetMethod.CreateDelegate(typeof(Func<TObject, TProperty>));
                }

                internal override object Fetch(object obj)
                {
                    return _propertyFetch((TObject)obj);
                }
            }
        }
    }
}
