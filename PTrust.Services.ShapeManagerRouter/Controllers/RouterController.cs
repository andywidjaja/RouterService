using System;
using System.Collections.Generic;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;

namespace PTrust.Services.ShapeManagerRouter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouterController : ControllerBase
    {
        private readonly ISmsRouter _smsRouter;

        public RouterController(ISmsRouter smsRouter)
        {
            _smsRouter = smsRouter;
        }

        [Route("")]
        [HttpGet]
        public string Get()
        {
            return $"[{Environment.MachineName}]: Hello SMS router service consumer!";
        }

        [Route("getroutingtable")]
        [HttpGet]
        public List<SmsNode> GetRoutingTable()
        {
            return _smsRouter.GetRoutingTable();
        }

        [Route("addroute")]
        [HttpPost]
        public IActionResult AddRoute([FromBody] SmsNode node)
        {
            if (node == null)
            {
                return BadRequest("Route change request cannot be null");
            }

            _smsRouter.AddNode(node);

            return Ok(node);
        }

        [Route("removeroute")]
        [HttpPost]
        public IActionResult RemoveRoute([FromBody] SmsNode node)
        {
            if (node == null)
            {
                return BadRequest("Route change request cannot be null");
            }

            _smsRouter.RemoveNode(new SmsNode { Host = node.Host, Port = node.Port });

            return Ok(node);
        }

        [Route("route")]
        [HttpPost]
        public SmsRouteResponse Route([FromBody] SmsRouteRequest routingRequest)
        {
            if (routingRequest.HttpMethod == HttpMethod.Post && (routingRequest.Id <= 0 || string.IsNullOrEmpty(routingRequest.UserName)))
            {
                return null;
            }

            return _smsRouter.Route(routingRequest);
        }
    }
}