using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphConnection<T> where T : GraphNode
{
    public T nodeA;
    public T nodeB;
    Transform entryPortalA;
    Transform leavePortalA;
    Transform entryPortalB;
    Transform leavePortalB;
    public GraphConnection(T nodeA, T nodeB)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        entryPortalA = this.nodeA.entryPortal;
        leavePortalA = this.nodeA.leavePortal;
        entryPortalB = this.nodeB.entryPortal;
        leavePortalB = this.nodeB.leavePortal;
    }

    public void connectPortals()
    {
        //Connects entry and leave portals of the nodes in the connection
        leavePortalA.GetComponent<TFG.PortalRender>().connectedPortal = entryPortalB;
        entryPortalB.GetComponent<TFG.PortalRender>().connectedPortal = leavePortalA;
    }

    public void disconnectPortals()
    {
        //Disconnects entry and leave portals of the nodes in the connection
        leavePortalA.GetComponent<TFG.PortalRender>().connectedPortal = null;
        entryPortalB.GetComponent<TFG.PortalRender>().connectedPortal = null;
    }
}
