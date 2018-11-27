using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

namespace PTrust.Services.ShapeManagerRouter
{
    public class SmsRouter : ISmsRouter
    {
        private const string Delimiter = ":";

        private readonly List<string> _nodeList;

        private readonly IPtLogger _ptLogger;

        private static int _roundRobinNodeIndex;

        private static readonly object SynLock = new object();

        public SmsRouter(IOptions<RouteSettings> routeSettings, IPtLogger ptLogger)
        {
            _ptLogger = ptLogger;

            if (_nodeList == null)
            {
                _nodeList = new List<string>();
            }

            foreach (var node in routeSettings.Value.NodeList)
            {
                _nodeList.Add($"{node.Host.Trim().ToUpper()}:{node.Port}");
            }
        }

        public void AddNode(SmsNode newNode)
        {
            var node = _nodeList.Find(n => n.Equals($"{newNode.Host.Trim().ToUpper()}{Delimiter}{newNode.Port}"));
            if (node != null)
            {
                return;
            }

            _nodeList.Add($"{newNode.Host.Trim().ToUpper()}:{newNode.Port}");
            _ptLogger.LogInfo($"{newNode.Host.Trim().ToUpper()}:{newNode.Port} was added");
        }

        public List<SmsNode> GetRoutingTable()
        {
            if (_nodeList == null)
            {
                return null;
            }

            var nodeList = new List<SmsNode>();
            foreach (var node in _nodeList)
            {
                var routeParts = node.Split(Delimiter);
                nodeList.Add(new SmsNode {Host = routeParts[0], Port = Convert.ToInt32(routeParts[1])});
            }

            return nodeList;
        }

        public void RemoveNode(SmsNode nodeToRemove)
        {
            var node = _nodeList.Find(n => n.Equals($"{nodeToRemove.Host.Trim().ToUpper()}{Delimiter}{nodeToRemove.Port}"));
            if (node == null)
            {
                return;
            }

            _nodeList.Remove($"{nodeToRemove.Host.Trim().ToUpper()}{Delimiter}{nodeToRemove.Port}");
            _ptLogger.LogInfo($"{nodeToRemove.Host.Trim().ToUpper()}{Delimiter}{nodeToRemove.Port} was removed");
        }

        public SmsRouteResponse Route(SmsRouteRequest routeRequest)
        {
            string route;
            string[] routeParts;

            if (routeRequest == null)
            {
                return null;
            }

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            //_ptLogger.LogInfo($"{routeRequest.HttpMethod} request processing started");

            // If username is provided, perform custom routing algorithm
            if (!string.IsNullOrEmpty(routeRequest.UserName))
            {
                var routingParameter = new StringBuilder(routeRequest.UserName.Trim().ToUpper());
                if (routeRequest.Id.HasValue && routeRequest.Id > 0)
                {
                    routingParameter.Append("|" + routeRequest.Id);
                }

                // Perform hashing
                var hash = CalculateMd5Hash(routingParameter.ToString());
                var idx = Math.Abs(BitConverter.ToInt32(hash, 0)) % _nodeList.Count;
                route = _nodeList[idx];

                _ptLogger.LogInfo($"{routeRequest.HttpMethod} request route parameter(s): {routingParameter}, route: {route}");
                routeParts = route.Split(Delimiter);

                //stopwatch.Stop();
                //_ptLogger.LogInfo($"{routeRequest.HttpMethod} request, round robin route: {route} in {stopwatch.ElapsedMilliseconds} ms");

                return new SmsRouteResponse { Host = routeParts[0], Port = Convert.ToInt32(routeParts[1]) };
            }

            // Else, round robin
            lock (SynLock)
            {
                // Reset index back to 0 when it reaches maximum data type value
                if (_roundRobinNodeIndex == int.MaxValue)
                {
                    _roundRobinNodeIndex = 0;
                }

                route = _nodeList[_roundRobinNodeIndex++ % _nodeList.Count];
            }                

            //stopwatch.Stop();
            //_ptLogger.LogInfo($"{routeRequest.HttpMethod} request, round robin route: {route} in {stopwatch.ElapsedMilliseconds} ms");
            routeParts = route.Split(Delimiter);

            return new SmsRouteResponse { Host = routeParts[0], Port = Convert.ToInt32(routeParts[1]) };            
        }

        private static byte[] CalculateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            return md5.ComputeHash(inputBytes);

            //var sb = new StringBuilder();
            //foreach (var c in hash)
            //{
            //    sb.Append(c.ToString("x2"));
            //}

            //return sb.ToString();
        }
    }
}