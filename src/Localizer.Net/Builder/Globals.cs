using System.Collections.Generic;

namespace Localizer.Net
{
    internal class Globals
    {
        public Dictionary<string, object> Args { get; }

        public Globals(Dictionary<string, object> args)
        {
            Args = args;
        }
    }
}
