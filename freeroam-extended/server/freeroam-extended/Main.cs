using AltV.Net;

namespace Freeroam_Extended
{
    public class Main : Resource
    {
        public override void OnStart()
        {
            Alt.Server.LogColored("~g~ Freeroam-Extended Started!");
        }

        public override void OnStop()
        {
            Alt.Server.LogColored("~g~ Freeroam-Extended Stopped!");
        }
    }
}