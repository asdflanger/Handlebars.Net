using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HandlebarsDotNet.Collections;
using HandlebarsDotNet.EqualityComparers;
using HandlebarsDotNet.Iterators;
using HandlebarsDotNet.MemberAccessors.DictionaryAccessors;
using HandlebarsDotNet.Polyfills;
using HandlebarsDotNet.Runtime;

namespace HandlebarsDotNet.ObjectDescriptors
{
    public sealed class ReadOnlyStringDictionaryObjectDescriptorProvider : IObjectDescriptorProvider
    {
        private static readonly object[] EmptyArray = ArrayEx.Empty<object>();
        private static readonly MethodInfo CreateClassDescriptorMethodInfo = typeof(ReadOnlyStringDictionaryObjectDescriptorProvider)
            .GetMethod(nameof(CreateDescriptor), BindingFlags.NonPublic | BindingFlags.Static);

        private readonly LookupSlim<Type, DeferredValue<Type, Type>, TypeEqualityComparer> _typeCache = new LookupSlim<Type, DeferredValue<Type, Type>, TypeEqualityComparer>(new TypeEqualityComparer());

        public bool TryGetDescriptor(Type type, out ObjectDescriptor value)
        {
            var interfaceType = _typeCache.GetOrAdd(type, InterfaceTypeValueFactory).Value;
            if (interfaceType == null)
            {
                value = ObjectDescriptor.Empty;
                return false;
            }

            var typeArgument = interfaceType.GetGenericArguments()[1];
            var factory = CreateClassDescriptorMethodInfo;
            
            var descriptorCreator = factory
                .MakeGenericMethod(type, typeArgument);

            value = (ObjectDescriptor) descriptorCreator.Invoke(null, EmptyArray);
            return true;
        }
        
        private static readonly Func<Type, DeferredValue<Type, Type>> InterfaceTypeValueFactory = 
            key => new DeferredValue<Type, Type>(key, type =>
            {
                return type.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) &&
                        i.GetGenericArguments()[0] == typeof(string));
            });

        private static ObjectDescriptor CreateDescriptor<T, TV>() 
            where T : class, IReadOnlyDictionary<string, TV>
        {
            return new ObjectDescriptor(
                typeof(IDictionary<string, TV>),
                new ReadOnlyStringDictionaryAccessor<T, TV>(),
                (descriptor, o) => ((T) o).Keys,
                self => new ReadOnlyDictionaryIterator<T, string, TV>()
            );
        }
    }
}