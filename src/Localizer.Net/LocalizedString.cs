namespace Localizer.Net
{
    public class LocalizedString
    {
        private readonly ILocalization _localization;

        private readonly string _path;

        public LocalizedString(string path, ILocalization localization)
        {
            _localization = localization;
            _path = path;
        }

        public string Localize(string locale, params (string, object)[] context)
        {
            return _localization.Resolve(locale, _path, context);
        }
    }
}
