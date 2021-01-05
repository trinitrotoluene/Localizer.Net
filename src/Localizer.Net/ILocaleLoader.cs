namespace Localizer.Net
{
    /// <summary>
    /// Describes a type capable of loading and resolving locales (collections of localisation data).
    /// Tags are the root of any locale, e.g. "en-US", "de-DE", etc.
    /// </summary>
    public interface ILocaleLoader
    {
        /// <summary>
        /// Verifies whether this loader is capable of loading a given locale.
        /// </summary>
        /// <param name="tag">The tag of the locale to test for.</param>
        /// <returns>If the locale is supported by this loader.</returns>
        bool Supports(string tag);

        /// <summary>
        /// Synchronously loads a locale from the backing format.
        /// This method is specifically general in order to support many different storage options.
        /// </summary>
        /// <param name="tag">The tag of the locale to search for.</param>
        /// <returns>The loaded locale.</returns>
        Locale Load(string tag);
    }
}
