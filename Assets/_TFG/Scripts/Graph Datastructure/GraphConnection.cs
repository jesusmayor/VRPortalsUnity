using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFG;

public class GraphConnection<T> where T : GraphNode
{
    public T nodeA;
    public T nodeB;
    private Transform nodeAPortal;
    private Transform nodeBPortal;
    private bool ramificationConnection;

    public GraphConnection(T nodeA, T nodeB)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
    }

    public bool isRamicationType()
    {
        return ramificationConnection;
    }

    public void setRamificationConnection()
    {
        ramificationConnection = true;
        nodeAPortal = nodeA.getNotMainNodeHallwayPortal(); 
    }

    public Transform getNodeAPortal()
    {
        return nodeAPortal;
    }

    public Transform getNodeBPortal()
    {
        return nodeBPortal;
    }

    public void connectPortals()
    {
        if (ramificationConnection)
        {
            nodeAPortal = nodeA.getNotMainNodeHallwayPortal();
            Debug.Log("Portal set to not main");
        }
        else
        {
            nodeAPortal = nodeA.getMainNodeHallwayPortal();
            Debug.Log("Portal set to main");
        }

        nodeBPortal = nodeB.getEntryPortal();

        Debug.Log("nodeAPortal = " + nodeAPortal);
        Debug.Log("nodeBPortal = " + nodeBPortal);
        nodeAPortal.GetComponent<PortalRender>().connectedPortal = nodeBPortal;
        nodeBPortal.GetComponent<PortalRender>().connectedPortal = nodeAPortal;
    }

}
