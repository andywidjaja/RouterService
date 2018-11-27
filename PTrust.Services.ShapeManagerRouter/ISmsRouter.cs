using System.Collections.Generic;

namespace PTrust.Services.ShapeManagerRouter
{
    public interface ISmsRouter
    {
        void AddNode(SmsNode newNode);
        List<SmsNode> GetRoutingTable();
        void RemoveNode(SmsNode nodeToRemove);
        SmsRouteResponse Route(SmsRouteRequest routeRequest);
    }
}