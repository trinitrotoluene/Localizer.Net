namespace Localizer.Net
{
    /// <summary>
    /// Describes a type capable of resolving localisation strings from a path and optional context arguments.
    /// The "path" of a localisation is how it is uniquely identified, and its derivation depends on the source of the
    /// localisation strings, for example the JSON { "a": { "b": "c" } } yields the path "a.b" = "c"
    /// </summary>
    public interface ILocalization
    {
        /// <summary>
        /// Resolves and formats a localised string from the localisation, using the default locale.
        /// </summary>
        /// <param name="path">The unique path of the string in the locale.</param>
        /// <param name="context">Optional context arguments to enrich the formatting context with.</param>
        /// <returns>A localised string.</returns>
        string Resolve(string path, params (string name, object value)[] context);

        /// <summary>
        /// Resolves and formats a localised string from the localisation.
        /// </summary>
        /// <param name="locale">The tag of the locale to use when resolving this string.</param>
        /// <param name="path">The unique path of the string in the locale.</param>
        /// <param name="context">Optional context arguments to enrich the formatting context with.</param>
        /// <returns>A localised string.</returns>
        string Resolve(string locale, string path, params (string name, object value)[] context);
    }
}
