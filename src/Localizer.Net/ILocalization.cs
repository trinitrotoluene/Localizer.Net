namespace Localizer.Net
{
    public interface ILocalization
    {
        string Resolve(string path, params (string name, object value)[] context);

        string Resolve(string locale, string path, params (string name, object value)[] context);
    }
}
