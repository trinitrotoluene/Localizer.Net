using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Localizer.Net.Tests
{
    public class LocalizerTests
    {
        private readonly ILocalization _localization = new LocalizationBuilder()
            .UseJsonFiles("locales")
            .Build();

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
            Assert.Throws<DirectoryNotFoundException>(() => builder.UseJsonFiles("xyz").Build());
        }

        [Test]
        public void AssertThrowsDuplicateLocalization()
        {
            var builder = new LocalizationBuilder()
                .UseJsonFiles("locales");

            Assert.Throws<LocalizerException>(() => builder.UseJsonFiles("locales"));
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

            Localizer.Inject(_localization, Assembly.GetAssembly(typeof(LocalizableClass)));

            Assert.AreEqual("world", LocalizableClass.WorldString.Localize("en-US"));
            Assert.AreEqual("told me", LocalizableClass.SomebodyOnceString.Localize("en-US"));
        }

        [Test]
        public void AssertLocalizationCanBeInjectedInstance()
        {
            var instance = new LocalizableInstanceClass();

            Localizer.Inject(instance, _localization);

            Assert.AreEqual("world", instance.WorldString.Localize("en-US"));
        }

        [Test]
        public void AssertExecuteSimpleReplacement()
        {
            var test1 = _localization.Resolve("en-US", "context-tests.test1", ("drill", "test"));
            Assert.AreEqual("This is not a test!", test1);
        }

        [Test]
        public void AssertExecuteMultipleReplacement()
        {
            var test2 = _localization.Resolve("en-US", "context-tests.test2", ("drill", "test"), ("drill2", "test2"));
            Assert.AreEqual("This is not a test test2", test2);
        }

        [Test]
        public void AssertExecuteStatementReplacement()
        {
            var test3 = _localization.Resolve("en-US", "context-tests.test3", ("isDrill", true), ("drill", "drill"));
            Assert.AreEqual("This is not a drill!", test3);
        }

        [Test]
        public void AssertExecuteStatementReplacementAtStart()
        {
            var test4 = _localization.Resolve("en-US", "context-tests.test4", ("isKieran", true));
            Assert.AreEqual("Kieran is cool", test4);
        }

        [Test]
        public void AssertThrowsUnclosedBrace()
        {
            Assert.Throws<LocalizerException>(() => _localization.Resolve("en-US", "context-tests.test5"));
        }

        [Test]
        public void AssertThrowsEmptyScript()
        {
            Assert.Throws<LocalizerException>(() => _localization.Resolve("en-US", "context-tests.test6"));
        }

        [Test]
        public void AssertThrowsMissingKey()
        {
            Assert.Throws<LocalizerException>(() => _localization.Resolve("en-US", "I don't exist"));
        }

        [Test]
        public void AssertThrowsBadDefaultLocale()
        {
            Assert.Throws<LocalizerException>(() => new LocalizationBuilder().WithDefaultLocale("blah").Build());
        }

        [Test]
        public void AssertFallbackToDefaultLocale()
        {
            var localization = new LocalizationBuilder()
                .WithDefaultLocale("en-US")
                .UseJsonFiles("locales")
                .Build();

            Assert.AreEqual("world", localization.Resolve("de-DE", "hello"));
        }

        [Test]
        public void AssertDefaultLocaleOverload()
        {
            var localization = new LocalizationBuilder()
                .WithDefaultLocale("en-US")
                .UseJsonFiles("locales")
                .Build();

            Assert.AreEqual("world", localization.Resolve("hello"));
        }

        [Test]
        public void AssertOverloadThrowsWhenNoDefaultLocaleSet()
        {
            var localization = new LocalizationBuilder()
                .UseJsonFiles("locales")
                .Build();

            Assert.Throws<InvalidOperationException>(() => localization.Resolve("hello"));
        }

        [Test]
        public void AssertThrowsBadLocale()
        {
            Assert.Throws<LocalizerException>(() => new LocalizationBuilder().Build().Resolve("blah", "blah"));
        }

        [Test]
        public void AssertThrowsScriptWhenDisabled()
        {
            var localization = new LocalizationBuilder()
                .UseJsonFiles("locales")
                .WithScriptOptions(null)
                .Build();

            Assert.Throws<InvalidOperationException>(() => localization.Resolve("en-US", "context-tests.test3"));
        }
    }

    class LocalizableClass
    {
        [Localize("hello")]
        public static LocalizedString WorldString = default;

        [Localize("somebody.once")]
        public static LocalizedString SomebodyOnceString = default;
    }

    class LocalizableInstanceClass
    {
        [Localize("hello")]
        public LocalizedString WorldString = default;
    }
}
