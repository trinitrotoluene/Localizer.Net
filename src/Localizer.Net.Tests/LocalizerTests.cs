using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Localizer.Net.Tests
{
    public class LocalizerTests
    {
        [Test]
        public void AssertCanLoadJsonLocalisations()
        {
            var builder = new LocalizationBuilder();
            builder.UseJsonFiles("locales");
            var localization = builder.Build();

            Assert.AreEqual("world", localization.Resolve("en-US", "hello"));
            Assert.AreEqual("told me", localization.Resolve("en-US", "somebody.once"));
        }

        [Test]
        public void AssertThrowsInvalidDirectory()
        {
            var builder = new LocalizationBuilder();
            Assert.Throws<DirectoryNotFoundException>(() => builder.UseJsonFiles("xyz"));
        }

        [Test]
        public void AssertIgnoresNonJsonFiles()
        {
            var builder = new LocalizationBuilder();
            builder.UseJsonFiles("locales-nonjson");
        }

        [Test]
        public void AssertLocalizationCanBeInjectedStatic()
        {
            var localization = new LocalizationBuilder()
                .UseJsonFiles("locales")
                .Build();

            Localizer.Inject(localization, Assembly.GetAssembly(typeof(LocalizableClass)));

            Assert.AreEqual("world", LocalizableClass.WorldString.Localize("en-US"));
            Assert.AreEqual("told me", LocalizableClass.SomebodyOnceString.Localize("en-US"));
        }

        [Test]
        public void AssertLocalizationCanBeInjectedInstance()
        {
            var localization = new LocalizationBuilder()
                .UseJsonFiles("locales")
                .Build();

            var instance = new LocalizableInstanceClass();

            Localizer.Inject(instance, localization);

            Assert.AreEqual("world", instance.WorldString.Localize("en-US"));
        }
    }

    class LocalizableClass
    {
        [Localize("hello")]
        public static LocalizedString WorldString;

        [Localize("somebody.once")]
        public static LocalizedString SomebodyOnceString;
    }

    class LocalizableInstanceClass
    {
        [Localize("hello")]
        public LocalizedString WorldString;
    }
}
