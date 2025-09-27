using System;
using System.Reflection;
using Autodesk.Revit.DB;

namespace RevitAddinCSharp.Utils
{
    internal static class ElementIdHelper
    {
        private static readonly PropertyInfo ValueProperty = typeof(ElementId).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        private static readonly PropertyInfo IntegerValueProperty = typeof(ElementId).GetProperty("IntegerValue", BindingFlags.Public | BindingFlags.Instance);
        private static readonly ConstructorInfo LongConstructor = typeof(ElementId).GetConstructor(new[] { typeof(long) });
        private static readonly ConstructorInfo IntConstructor = typeof(ElementId).GetConstructor(new[] { typeof(int) });

        public static long GetIdValue(ElementId id)
        {
            if (id == null)
            {
                return 0;
            }

            if (ValueProperty != null)
            {
                object value = ValueProperty.GetValue(id);
                return Convert.ToInt64(value);
            }

            if (IntegerValueProperty != null)
            {
                object value = IntegerValueProperty.GetValue(id);
                return Convert.ToInt64(value);
            }

            throw new InvalidOperationException("ElementId value property not found.");
        }

        public static ElementId Create(long value)
        {
            if (LongConstructor != null)
            {
                return (ElementId)LongConstructor.Invoke(new object[] { value });
            }

            if (IntConstructor != null)
            {
                return (ElementId)IntConstructor.Invoke(new object[] { checked((int)value) });
            }

            throw new InvalidOperationException("ElementId constructor not found.");
        }
    }
}
