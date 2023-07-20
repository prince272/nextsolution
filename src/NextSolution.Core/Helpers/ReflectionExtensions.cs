using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Helpers
{
    public static class ReflectionExtensions
    {
        public static bool IsCompatibleWith(this Type type, Type otherType)
        {
            if (otherType.IsGenericTypeDefinition)
            {
                return type.IsAssignableToGenericTypeDefinition(otherType);
            }

            return otherType.IsAssignableFrom(type);
        }

        private static bool IsAssignableToGenericTypeDefinition(this Type type, Type genericType)
        {
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType)
                {
                    var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == genericType)
                    {
                        return true;
                    }
                }
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == genericType)
                {
                    return true;
                }
            }

            var baseType = type.BaseType;
            if (baseType is null)
            {
                return false;
            }

            return baseType.IsAssignableToGenericTypeDefinition(genericType);
        }

        public static IEnumerable<Type> FindMatchingInterfaces(this Type type)
        {
            var matchingInterfaceName = $"I{type.Name}";
            var matchedInterfaces = GetImplementedInterfacesToMap(type).Where(x => string.Equals(x.Name, matchingInterfaceName, StringComparison.Ordinal)).ToArray();
            return matchedInterfaces;
        }

        private static IEnumerable<Type> GetImplementedInterfacesToMap(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.GetInterfaces();
            }

            if (!type.IsGenericTypeDefinition)
            {
                return type.GetInterfaces();
            }

            return FilterMatchingGenericInterfaces(type);
        }

        private static IEnumerable<Type> FilterMatchingGenericInterfaces(Type type)
        {
            var genericArguments = type.GetGenericArguments();

            foreach (var current in type.GetInterfaces())
            {
                if (current.IsGenericType && current.ContainsGenericParameters && GenericParametersMatch(genericArguments, current.GetGenericArguments()))
                {
                    yield return current.GetGenericTypeDefinition();
                }
            }
        }

        private static bool GenericParametersMatch(IReadOnlyList<Type> parameters, IReadOnlyList<Type> interfaceArguments)
        {
            if (parameters.Count != interfaceArguments.Count)
            {
                return false;
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] != interfaceArguments[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
