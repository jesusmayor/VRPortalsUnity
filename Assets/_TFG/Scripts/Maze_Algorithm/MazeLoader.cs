using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEngine;

public class MazeLoader : MonoBehaviour
{
    public int mazeLength;
    Object prefab;
    // Start is called before the first frame update
    void Start()
    {
        generateMaze(mazeLength);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GameObject Hallway(Vector3 position)
    {
        return (GameObject)Instantiate(prefab, position, Quaternion.identity);
    }

    private void generateMaze(int numNodes)
    {
        prefab = Resources.Load("PortalHallway");
        Vector3 position = Vector3.zero;
        //Initialize the graph
        Graph<GraphNode> graph = new Graph<GraphNode>();

        //Create two base nodes that are hallways
        for(int i = 0; i < numNodes; i++)
        {
            int currentNode = graph.GetNodes().Count;
            graph.AddNode(Hallway(position));
            position.x += 10;

            //If the current node isnt the first one, we connect it to the node that is before
            if (currentNode != 0)
            {
                graph.ConnectNodes(graph.GetNodes()[currentNode - 1], graph.GetNodes()[currentNode]);
            }
        }

        //Print the number of nodes in the graph
        Debug.Log("Number of nodes = " + graph.GetNodes().Count);
        Debug.Log("Number of connections" + graph.GetConnections().Count);
    }
}
