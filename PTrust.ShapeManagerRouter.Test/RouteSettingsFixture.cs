using System;
using System.Collections.Generic;

using Microsoft.Extensions.Options;
using PTrust.Services.ShapeManagerRouter;

namespace PTrust.ShapeManagerRouter.Test
{
    public class RouteSettingsFixture : IDisposable
    {
        public RouteSettingsFixture()
        {
            RouteSettings = Options.Create(new RouteSettings());
            RouteSettings.Value.NodeList = new List<SmsNode>
            {
                new SmsNode {Host = "DEVWEB1", Port = 10000},
                new SmsNode {Host = "DEVWEB2", Port = 20000}
            };
        }

        public void Dispose()
        {
        }

        public IOptions<RouteSettings> RouteSettings { get; set; }
    }
}