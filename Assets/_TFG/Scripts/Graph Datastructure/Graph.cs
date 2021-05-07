using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph<T> where T : GraphNode, new()
{
    protected List<GraphNode> nodes;
    protected List<GraphConnection<T>> connections;

    public Graph()
    {
        nodes = new List<GraphNode>();
        connections = new List<GraphConnection<T>>();
    }

    public List<GraphNode> getNodes() 
    {
        return nodes;
    }

    public int getNumberOfNodes()
    {
        return getNodes().Count;
    }

    public void addNode(GraphNode node)
    {
        nodes.Add(node);
    }

    public List<GraphConnection<T>> getConnections()
    {
        return connections;
    }

    public bool connectNodes(T nodeA, T nodeB)
    {
        GraphConnection<T> connection = new GraphConnection<T>(nodeA, nodeB);
        if (nodeA != nodeB && findConnection(connection) == null)
        {
            nodeA.addConnectedNode(nodeB);
            nodeB.addConnectedNode(nodeA);
            connections.Add(connection);
        }
        else
            return false;
        return true;
    }

    public bool disconnectNodes(GraphConnection<T> connection) //Return boolean
    {
        if (connection != null)
        {
            connections.Remove(findConnection(connection));
            connection.nodeA.removeConnectedNode(connection.nodeB);
            connection.nodeB.removeConnectedNode(connection.nodeA);
        }
        else
            return false;
        return true;
    }

    public void disconnectNodes(T nodeA, T nodeB)
    {
        GraphConnection<T> connection = new GraphConnection<T>(nodeA, nodeB);
        disconnectNodes(connection);
    }

    public GraphConnection<T> findConnection(GraphConnection<T> connection)
    {
        foreach (GraphConnection<T> existingConnection in connections)
        {
            if (existingConnection.nodeA == connection.nodeA &&
                existingConnection.nodeB == connection.nodeB)
            {
                return existingConnection;
            }

            if (existingConnection.nodeA == connection.nodeB &&
                  existingConnection.nodeB == connection.nodeA)
            {
                return existingConnection;
            }
        }

        return null;
    }

    public GraphConnection<T> findConnection(T nodeA, T nodeB)
    {
        GraphConnection<T> connection = new GraphConnection<T>(nodeA, nodeB);
        return findConnection(connection);
    }
}

