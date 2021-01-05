using System.Collections.Generic;

namespace Localizer.Net
{
    public class Globals
    {
        public Dictionary<string, object> Args { get; }

        public Globals(Dictionary<string, object> args)
        {
            Args = args;
        }
    }
}
