using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameECS
{
    internal static class ReflectionUtils
    {
        private static readonly Type _oBJECT_TYPE = typeof(object);
        private static readonly Type _mONO_BEHAVIOUR_TYPE = typeof(MonoBehaviour);
        
        internal static List<MethodInfo> RetrieveMethods(Type targetType)
        {
            var result = new List<MethodInfo>();
            while (IsRetrievableType(targetType))
            {
                var methods = targetType.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic | 
                    BindingFlags.DeclaredOnly
                );

                result.AddRange(methods);
                targetType = targetType.BaseType;
            }

            return result;
        }
        
        internal static List<FieldInfo> RetrieveFields(Type targetType)
        {
            var result = new List<FieldInfo>();
            while (IsRetrievableType(targetType))
            {
                var fields = targetType.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly
                );

                result.AddRange(fields);
                targetType = targetType.BaseType;
            }

            return result;
        }
        
        private static bool IsRetrievableType(Type type)
        {
            return type != null && type != _oBJECT_TYPE && type != _mONO_BEHAVIOUR_TYPE;
        }
    }
}