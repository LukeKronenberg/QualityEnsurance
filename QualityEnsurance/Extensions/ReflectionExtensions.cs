using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QualityEnsurance.Extensions
{
    public static class ReflectionExtensions
    {
        public static object Value(this FieldInfo field)
        {
            if (field.IsLiteral && !field.IsInitOnly)
                return field.GetRawConstantValue();
            else
                return field.GetValue(field.FieldType);
        }
        public static T Value<T>(this FieldInfo field) => (T) field.Value();
    }
}
