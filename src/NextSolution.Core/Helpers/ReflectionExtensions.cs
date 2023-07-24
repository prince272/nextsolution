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
    }
}
