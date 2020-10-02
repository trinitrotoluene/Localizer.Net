using System;
using System.Linq;
using System.Reflection;

namespace Localizer.Net
{
    public static class Localizer
    {
        public static void Inject<TTarget>(TTarget target, ILocalization localization)
        {
            var fields = typeof(TTarget).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType != typeof(LocalizedString))
                {
                    continue;
                }

                var localizedString = CreateLocalizedString(localization, field);
                field.SetValue(target, localizedString);
            }
        }

        public static void Inject(ILocalization localization, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                Inject(localization, type);
            }
        }

        public static void Inject(ILocalization localization, Type type)
        {
            var staticFields = GetStaticFields(type).Where(x => x.FieldType == typeof(LocalizedString));

            foreach (var field in staticFields)
            {
                var localizedString = CreateLocalizedString(localization, field);
                field.SetValue(null, localizedString);
            }
        }

        private static LocalizedString CreateLocalizedString(ILocalization localization, FieldInfo field)
        {
            var localizeAttribute = field.GetCustomAttribute<LocalizeAttribute>();
            if (localizeAttribute == null)
            {
                throw new LocalizerException($"A static ${nameof(LocalizedString)} field must be decorated with ${nameof(LocalizeAttribute)} when injecting a localizer into an assembly.");
            }

            var localizedString = new LocalizedString(localizeAttribute.Path, localization);
            return localizedString;
        }

        private static FieldInfo[] GetStaticFields(Type type)
        {
            return type.GetFields(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );
        }
    }
}
