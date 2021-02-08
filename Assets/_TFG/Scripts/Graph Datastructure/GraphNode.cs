using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEditor;
using UnityEngine;

public class GraphNode
{
    protected List<GraphNode> connectedNodes;
    GameObject prefab;
    public Transform entryPortal;
    public Transform leavePortal;


    public GraphNode()
    {
        connectedNodes = new List<GraphNode>();
    }

    public GraphNode(GameObject prefab)
    {
        connectedNodes = new List<GraphNode>();
        this.prefab = prefab;
        this.entryPortal = prefab.transform.Find("EntryPortal");
        this.leavePortal = prefab.transform.Find("LeavePortal");
    }

    public void AddConnectedNode(GraphNode node)
    {
        if (node != this && !connectedNodes.Contains(node))
        {
            connectedNodes.Add(node);
        }
    }

    public void RemoveConnectedNode(GraphNode node)
    {
        connectedNodes.Remove(node);
    }

    public bool IsConnectedTo(GraphNode node)
    {
        return connectedNodes.Contains(node);
    }

    public List<GraphNode> GetConnectedNodes()
    {
        return connectedNodes;
    }
}
