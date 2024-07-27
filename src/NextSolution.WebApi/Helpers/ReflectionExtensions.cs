namespace NextSolution.WebApi.Helpers
{
    public static class ReflectionExtensions
    {
        public static bool IsCompatibleWith(this Type type, Type otherType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (otherType == null) throw new ArgumentNullException(nameof(otherType));

            if (otherType.IsGenericTypeDefinition)
            {
                return type.IsAssignableToGenericTypeDefinition(otherType);
            }

            return otherType.IsAssignableFrom(type);
        }

        private static bool IsAssignableToGenericTypeDefinition(this Type type, Type genericType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (genericType == null) throw new ArgumentNullException(nameof(genericType));

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
