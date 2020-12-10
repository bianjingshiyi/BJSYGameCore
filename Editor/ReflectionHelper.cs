using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace BJSYGameCore
{
    public static class ReflectionHelper
    {
        public static T getAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();
        }
        public static readonly Assembly[] _assemblies = loadAssemblies();
        public static Assembly[] assemblies
        {
            get { return _assemblies; }
        }
        public static Assembly[] loadAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
        public static readonly Type[] _types = loadTypes();
        public static Type[] types
        {
            get { return _types; }
        }
        public static Type[] loadTypes()
        {
            return getTypes(assemblies);
        }
        private static Type[] getTypes(Assembly[] assemblies)
        {
            List<Type> list = new List<Type>();
            foreach (var assembly in assemblies)
            {
                list.AddRange(assembly.GetTypes());
            }
            return list.ToArray();
        }
        public static Type getType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            Type type = Type.GetType(typeName);
            if (type != null)
                return type;
            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return type;
        }
        public static Type[] getSubclass(Assembly[] assemblies, Type baseType)
        {
            if (assemblies == null || assemblies.Length < 1)
                assemblies = ReflectionHelper.assemblies;
            return getTypes(assemblies).Where(t => t.BaseType == baseType || t.IsSubclassOf(baseType)).ToArray();
        }
    }
}