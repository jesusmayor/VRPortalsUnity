using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEngine;

public class MazeLoader : MonoBehaviour
{
    public int mazeLength;
    Object straightHallwayprefab;
    Object LHallwayPrefab;
    
    void Start()
    {
        //Load possible prefab instances
        straightHallwayprefab = Resources.Load("StraightHallway");
        LHallwayPrefab = Resources.Load("LHallway");

        //Generate the maze
        Graph<GraphNode> maze = generateMaze(mazeLength);

        //Print the number of nodes in the graph that represents the maze
        Debug.Log("Number of nodes = " + maze.GetNodes().Count);
        Debug.Log("Number of connections = " + maze.GetConnections().Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Graph<GraphNode> generateMaze(int numNodes)
    {
        int currentNode = 0;
        int nodeOffset = 15;
        Vector3 position = Vector3.zero;

        //Initialize the graph
        Graph<GraphNode> graph = new Graph<GraphNode>();

        //Create the number of nodes specified
        for(int i = 0; i < numNodes; i++)
        {
            currentNode = graph.GetNodes().Count;
            graph.AddNode(instantiateRandomPrefab(position));
            position.x += nodeOffset;

            //If the current node isnt the first one, we connect it to the node that is before
            if (currentNode != 0)
            {
                graph.ConnectNodes(graph.GetNodes()[currentNode - 1], graph.GetNodes()[currentNode]);
            }
        }
        return graph;
    }

    private GameObject instantiateRandomPrefab(Vector3 position) //Instantiates a random prefab
    {
        int random = Random.Range(0, 2);
        GameObject prefab = null;

        switch(random)
        {
            case 0:
                prefab = straightHallway(position);
                break;

            case 1:
                prefab = LHallway(position);
                break;
        }
        return prefab;
    }

    private GameObject straightHallway(Vector3 position) //Instantiates the prefab "Hallway" at the position specified and returns it
    {
        return (GameObject)Instantiate(straightHallwayprefab, position, Quaternion.identity);
    }

    private GameObject LHallway(Vector3 position)
    {
        return (GameObject)Instantiate(LHallwayPrefab, position, Quaternion.identity);
    }
}
