using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;


[assembly: OwinStartup(typeof(demomvc.Services.Hubs.Startup))]
namespace demomvc.Services.Hubs
{
	public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }

    }
}