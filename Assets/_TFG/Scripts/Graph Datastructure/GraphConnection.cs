using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphConnection<T> where T : GraphNode
{
    public T nodeA;
    public T nodeB;

    public GraphConnection(T nodeA, T nodeB)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
    }
}
