using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFG;

public class GraphConnection<T> where T : GraphNode
{
    private T nodeA;
    private T nodeB;
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
        if (ramificationConnection)//If its a ramification connection, set the leave portal to the one connected to the ramification start node
        {
            nodeAPortal = nodeA.getNotMainNodeHallwayPortal();
        }
        else//Set the leave portal to the main one
        {
            nodeAPortal = nodeA.getMainNodeHallwayPortal();        }

        nodeBPortal = nodeB.getEntryPortal();

        Debug.Log("nodeAPortal = " + nodeAPortal);
        Debug.Log("nodeBPortal = " + nodeBPortal);
        nodeAPortal.GetComponent<PortalRender>().connectedPortal = nodeBPortal;
        nodeBPortal.GetComponent<PortalRender>().connectedPortal = nodeAPortal;
    }

    public T getNodeA()
    {
        return nodeA;
    }

    public T getNodeB()
    {
        return nodeB;
    }
}
