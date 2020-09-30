using System;

namespace Localizer.Net
{
    public interface ILocalization
    {
        string Resolve(string locale, string path, params (string name, object value)[] context);
    }
}
