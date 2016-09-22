using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DeependAncestry.Web.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace DeependAncestry.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}
