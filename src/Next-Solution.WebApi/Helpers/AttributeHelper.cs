using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace IdentityAudit.Utilities
{
    public static class AttributeHelper
    {
        private static readonly Dictionary<object, List<Attribute>> _attributeCache = new Dictionary<object, List<Attribute>>();

        // Types
        public static List<Attribute> GetTypeAttributes<TType>()
        {
            return GetTypeAttributes(typeof(TType));
        }

        public static List<Attribute> GetTypeAttributes(Type type)
        {
            return LockAndGetAttributes(type, tp => ((Type)tp).GetCustomAttributes(true));
        }

        public static List<TAttributeType> GetTypeAttributes<TAttributeType>(Type type, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetTypeAttributes(type)
                .OfType<TAttributeType>()
                .Where(attr => predicate is null || predicate(attr))
                .ToList();
        }

        public static List<TAttributeType> GetTypeAttributes<TType, TAttributeType>(Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetTypeAttributes(typeof(TType), predicate);
        }

        public static TAttributeType? GetTypeAttribute<TType, TAttributeType>(Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetTypeAttribute(typeof(TType), predicate);
        }

        public static TAttributeType? GetTypeAttribute<TAttributeType>(Type type, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetTypeAttributes(type, predicate).FirstOrDefault();
        }

        public static bool HasTypeAttribute<TType, TAttributeType>(Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return HasTypeAttribute(typeof(TType), predicate);
        }

        public static bool HasTypeAttribute<TAttributeType>(Type type, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetTypeAttribute(type, predicate) is not null;
        }

        // Enum
        public static List<Attribute> GetEnumAttributes<TEnum>() where TEnum : Enum
        {
            return GetTypeAttributes(typeof(TEnum));
        }

        public static List<TAttributeType> GetEnumAttributes<TEnum, TAttributeType>(Func<TAttributeType, bool>? predicate = null) where TEnum : Enum where TAttributeType : Attribute
        {
            return GetTypeAttributes(typeof(TEnum), predicate);
        }

        public static TAttributeType? GetEnumAttribute<TEnum, TAttributeType>(Func<TAttributeType, bool>? predicate = null) where TEnum : Enum where TAttributeType : Attribute
        {
            return GetTypeAttribute(typeof(TEnum), predicate);
        }

        public static bool HasEnumAttribute<TEnum, TAttributeType>(Func<TAttributeType, bool>? predicate = null) where TEnum : Enum where TAttributeType : Attribute
        {
            return HasTypeAttribute(typeof(TEnum), predicate);
        }

        // Enum Fields
        public static List<Attribute> GetEnumFieldAttributes<TEnum>(this TEnum value) where TEnum : Enum
        {
            return GetMemberAttributes(GetEnumField(value));
        }

        public static List<TAttributeType> GetEnumFieldAttributes<TEnum, TAttributeType>(this TEnum value, Func<TAttributeType, bool>? predicate = null) where TEnum : Enum where TAttributeType : Attribute
        {
            return GetMemberAttributes(GetEnumField(value), predicate);
        }

        public static TAttributeType? GetEnumFieldAttribute<TEnum, TAttributeType>(this TEnum value, Func<TAttributeType, bool>? predicate = null) where TEnum : Enum where TAttributeType : Attribute
        {
            return GetMemberAttribute(GetEnumField(value), predicate);
        }

        public static bool HasEnumFieldAttribute<TEnum, TAttributeType>(this TEnum value, Func<TAttributeType, bool>? predicate = null) where TEnum : Enum where TAttributeType : Attribute
        {
            return GetMemberAttribute(GetEnumField(value), predicate) is not null;
        }

        public static string GetEnumDisplayName<TEnum>(this TEnum value) where TEnum : Enum
        {
            var fieldInfo = GetEnumField(value);

            var displayName = fieldInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                            ?? fieldInfo.GetCustomAttribute<DisplayAttribute>()?.Name;

            return displayName ?? value.ToString();
        }

        // Members and properties
        public static List<Attribute> GetMemberAttributes<TType>(Expression<Func<TType, object>> action)
        {
            var memeber = GetMember(action);
            return memeber != null ? GetMemberAttributes(memeber) : new List<Attribute>();
        }

        public static List<TAttributeType> GetMemberAttributes<TType, TAttributeType>(
            Expression<Func<TType, object>> action,
            Func<TAttributeType, bool>? predicate = null)
            where TAttributeType : Attribute
        {
            var memeber = GetMember(action);
            return memeber != null ? GetMemberAttributes(memeber, predicate) : new List<TAttributeType>();
        }

        public static TAttributeType? GetMemberAttribute<TType, TAttributeType>(
            Expression<Func<TType, object>> action,
            Func<TAttributeType, bool>? predicate = null)
            where TAttributeType : Attribute
        {
            var memeber = GetMember(action);
            return memeber != null ? GetMemberAttribute(memeber, predicate) : null;
        }

        public static bool HasMemberAttribute<TType, TAttributeType>(Expression<Func<TType, object>> action, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            var memeber = GetMember(action);
            return memeber != null && (GetMemberAttribute(memeber, predicate) is not null);
        }

        // MemberInfo (and PropertyInfo since PropertyInfo inherits from MemberInfo)
        public static List<Attribute> GetMemberAttributes(this MemberInfo memberInfo)
        {
            return LockAndGetAttributes(memberInfo, mi => ((MemberInfo)mi).GetCustomAttributes(true));
        }

        public static List<TAttributeType> GetMemberAttributes<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetMemberAttributes(memberInfo)
                .OfType<TAttributeType>()
                .Where(attr => predicate is null || predicate(attr))
                .ToList();
        }

        public static TAttributeType? GetMemberAttribute<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return GetMemberAttributes(memberInfo, predicate).FirstOrDefault();
        }

        public static bool HasMemberAttribute<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool>? predicate = null) where TAttributeType : Attribute
        {
            return memberInfo.GetMemberAttribute(predicate) is not null;
        }


        private static List<Attribute> LockAndGetAttributes(object key, Func<object, object[]> retrieveValue)
        {
            return LockAndGet(_attributeCache, key, mi => retrieveValue(mi).Cast<Attribute>().ToList());
        }

        // Method for thread-safely executing slow method and storing the result in a dictionary
        private static TValue LockAndGet<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> retrieveValue) where TKey : notnull
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out var value))
                {
                    return value;
                }

                value = retrieveValue(key);
                dictionary[key] = value;
                return value;
            }
        }

        private static MemberInfo? GetMember<T>(Expression<Func<T, object>> expression)
        {
            switch (expression.Body)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member;

                case UnaryExpression unaryExpression:
                    switch (unaryExpression.Operand)
                    {
                        case MemberExpression unaryMemberExpression:
                            return unaryMemberExpression.Member;

                        case MethodCallExpression methodCallExpression:
                            return methodCallExpression.Method;
                    }
                    break;

                case MethodCallExpression methodCallExpression:
                    return methodCallExpression.Method;
            }

            return null;
        }


        private static MemberInfo GetEnumField<TEnum>(TEnum value) where TEnum : Enum
        {
            return typeof(TEnum).GetField(value.ToString())!;
        }
    }
}
