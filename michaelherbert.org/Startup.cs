using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Data.Entity;
using System.Data;


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
