using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEngine;

public class MazeLoader : MonoBehaviour
{

    Object prefab;
    // Start is called before the first frame update
    void Start()
    {
        prefab = Resources.Load("PortalHallway");
        GameObject prefabA = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        GameObject prefabB = (GameObject)Instantiate(prefab, new Vector3 (10,0,0), Quaternion.identity);

        //Initialize the graph
        Graph<GraphNode> graph = new Graph<GraphNode>();
        //Create two base nodes
        graph.AddNode(prefabA);
        graph.AddNode(prefabB);
        
        //Create the connection between the nodes
        graph.ConnectNodes(graph.GetNodes()[0], graph.GetNodes()[1]);

        //Print the number of nodes in the graph
        Debug.Log(graph.GetNodes().Count);
        Debug.Log(graph.GetConnections().Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
