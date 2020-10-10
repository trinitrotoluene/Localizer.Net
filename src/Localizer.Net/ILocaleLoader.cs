namespace Localizer.Net
{
    public interface ILocaleLoader
    {
        bool Supports(string tag);

        Locale Load(string tag);
    }
}
