using JetBrains.Annotations;

namespace Automate.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object instance)
        {
            return instance == null;
        }

        public static bool IsNotNull(this object instance)
        {
            return instance != null;
        }

        [ContractAnnotation("null => false; notnull => true")]
        public static bool Exists(this object instance)
        {
            return instance.IsNotNull();
        }

        [ContractAnnotation("null => true; notnull => false")]
        public static bool NotExists(this object instance)
        {
            return instance.IsNull();
        }
    }
}