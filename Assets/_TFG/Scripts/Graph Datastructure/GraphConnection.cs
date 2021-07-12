using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFG;

//This class is used to store connections between nodes and connect portals between the sections.

public class GraphConnection<T> where T : GraphNode
{
    protected T nodeA;
    protected T nodeB;
    protected Transform nodeAPortal;
    protected Transform nodeBPortal;
    protected bool ramificationConnection;

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

    public void connectPortals()//Connects the portal of nodeA with the portal of nodeB.
    {
        if (nodeA != null && nodeB != null)
        {
            if (ramificationConnection)//If its a ramification connection, set the leave portal to the one connected to the ramification start node
            {
                nodeAPortal = nodeA.getNotMainHallwayPortal();
            }
            else//Set the leave portal to the main one
            {
                nodeAPortal = nodeA.getMainHallwayPortal();
            }

            nodeBPortal = nodeB.getEntryPortal();

            Debug.Log("nodeAPortal = " + nodeAPortal);
            Debug.Log("nodeBPortal = " + nodeBPortal);
            nodeAPortal.GetComponent<PortalRender>().connectedPortal = nodeBPortal;
            nodeBPortal.GetComponent<PortalRender>().connectedPortal = nodeAPortal;
        }
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
