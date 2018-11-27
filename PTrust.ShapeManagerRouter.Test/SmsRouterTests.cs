using System;
using System.Collections.Generic;
using System.Net.Http;

using Microsoft.Extensions.Options;
using Moq;
using PTrust.Services.ShapeManagerRouter;
using Xunit;

namespace PTrust.ShapeManagerRouter.Test
{
    public class SmsRouterTests : IDisposable
    {
        private readonly Mock<IPtLogger> _ptLoggerMock = new Mock<IPtLogger>();

        private readonly IOptions<RouteSettings> _routeSettings;

        public SmsRouterTests()
        {
            _routeSettings = Options.Create(new RouteSettings());
            _routeSettings.Value.NodeList = new List<SmsNode>
            {
                new SmsNode {Host = "DEVWEB1", Port = 10000},
                new SmsNode {Host = "DEVWEB2", Port = 20000}
            };
        }

        public void Dispose()
        {
        }

        public IOptions<RouteSettings> RouteSettings { get; set; }

        [Theory]
        [MemberData(nameof(TestData))]
        public void AddRoute_ValidRoute_RouteGetsAddedSuccessfully(string host, int port)
        {
            var smsRouter = new SmsRouter(_routeSettings, _ptLoggerMock.Object);

            var routingTableSize = smsRouter.GetRoutingTable().Count;

            smsRouter.AddNode(new SmsNode {Host = host, Port = port});

            Assert.True(smsRouter.GetRoutingTable().Count == routingTableSize + 1);
        }        

        [Theory]
        [InlineData("DEVWEB1", 10000)]
        public void RemoveRoute_ValidRoute_RouteGetsRemovedSuccessfully(string host, int port)
        {
            var smsRouter = new SmsRouter(_routeSettings, _ptLoggerMock.Object);

            var routingTableSize = smsRouter.GetRoutingTable().Count;

            smsRouter.RemoveNode(new SmsNode {Host = host, Port = port});

            Assert.True(smsRouter.GetRoutingTable().Count == routingTableSize - 1);
        }

        [Theory]
        [MemberData(nameof(HttpGetRoutingTestData))]
        public void Route_ValidGetRequest_ReturnsValidRoundRobinRoute(HttpMethod httpMethod, string expectedHost, int expectedPort)
        {
            var smsRouter = new SmsRouter(_routeSettings, _ptLoggerMock.Object);

            var routeRequest = new SmsRouteRequest
            {
                HttpMethod = httpMethod
            };

            var routeResponse = smsRouter.Route(routeRequest);

            Assert.True(routeResponse.Host == expectedHost);
            Assert.True(routeResponse.Port == expectedPort);
        }

        [Theory]
        [MemberData(nameof(HttpPostRoutingTestData))]
        public void Route_ValidPostRequest_ReturnsValidRoute(HttpMethod httpMethod, string userName, int jobId, string expectedHost, int expectedPort)
        {
            var smsRouter = new SmsRouter(_routeSettings, _ptLoggerMock.Object);

            var routeRequest = new SmsRouteRequest
            {
                HttpMethod = httpMethod,
                UserName = userName,
                Id = jobId
            };

            var routeResponse = smsRouter.Route(routeRequest);

            Assert.True(routeResponse.Host == expectedHost);
            Assert.True(routeResponse.Port == expectedPort);
        }

        public static IEnumerable<object[]> HttpGetRoutingTestData => new List<object[]>
        {
            new object[] { HttpMethod.Get, "DEVWEB1", 10000 },
            new object[] { HttpMethod.Get, "DEVWEB2", 20000 },
            new object[] { HttpMethod.Get, "DEVWEB1", 10000 },
            new object[] { HttpMethod.Get, "DEVWEB2", 20000 }
        };

        public static IEnumerable<object[]> HttpPostRoutingTestData => new List<object[]>
        {
            new object[] { HttpMethod.Post, "PT\\User1", 111, "DEVWEB2", 20000 },
            new object[] { HttpMethod.Post, "PT\\User1", 222, "DEVWEB1", 10000 },
            new object[] { HttpMethod.Post, "PT\\User2", 333, "DEVWEB1", 10000 }
        };

        public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] { "DEVWEB3", 30000 },
            new object[] { "DEVWEB4", 40000 }
        };
    }
}