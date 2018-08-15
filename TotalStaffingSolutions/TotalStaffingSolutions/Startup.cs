using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TotalStaffingSolutions.Startup))]
namespace TotalStaffingSolutions
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
