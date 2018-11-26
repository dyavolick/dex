using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace dex_webapp.Services.SignalR
{
    public class OrderSignalR : Hub
    {
        public class ChartSubscribeViewModel
        {
            public string GroupName { get; set; }
            public string PrevGroupName { get; set; }
        }
        public async Task ChartSubscribe(ChartSubscribeViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.PrevGroupName))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, model.PrevGroupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, model.GroupName);
        }
    }
}
