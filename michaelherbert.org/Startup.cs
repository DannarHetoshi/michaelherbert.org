using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(michaelherbert.org.Startup))]
namespace michaelherbert.org
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
