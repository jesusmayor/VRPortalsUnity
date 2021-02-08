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

   
    public void AddNode(GameObject prefab)
    {
        GraphNode node = new GraphNode(prefab);
        nodes.Add(node);
    }

    public List<GraphNode> GetNodes()
    {
        return nodes;
    }

    public List<GraphConnection<T>> GetConnections()
    {
        return connections;
    }

    public void ConnectNodes(T nodeA, T nodeB)
    {
        GraphConnection<T> connection = new GraphConnection<T>(nodeA, nodeB);
        connection.connectPortals();
        if (nodeA != nodeB && FindConnection(connection) == null)
        {
            nodeA.AddConnectedNode(nodeB);
            nodeB.AddConnectedNode(nodeA);
            connections.Add(connection);
        }
    }

    public void DisconnectNodes(GraphConnection<T> connection)
    {
        connection.disconnectPortals();
        connections.Remove(FindConnection(connection));
        connection.nodeA.RemoveConnectedNode(connection.nodeB);
        connection.nodeB.RemoveConnectedNode(connection.nodeA);
    }

    public void DisconnectNodes(T nodeA, T nodeB)
    {
        GraphConnection<T> connection = new GraphConnection<T>(nodeA, nodeB);
        DisconnectNodes(connection);
    }

    public GraphConnection<T> FindConnection(GraphConnection<T> connection)
    {
        foreach (GraphConnection<T> existingConnection in connections) //TODO - this would be better served by a custom equality operator?
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

    public GraphConnection<T> FindConnection(T nodeA, T nodeB)
    {
        GraphConnection<T> connection = new GraphConnection<T>(nodeA, nodeB);
        return FindConnection(connection);
    }
}

