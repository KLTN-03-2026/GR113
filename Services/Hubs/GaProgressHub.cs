using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
namespace demomvc.Services.GA
{

    public class GaProgressHub : Hub
    {
        public static void Send(string message)
        {
            var context = GlobalHost.ConnectionManager
                .GetHubContext<GaProgressHub>();

            context.Clients.All.updateProgress(message);
        }
    }

}