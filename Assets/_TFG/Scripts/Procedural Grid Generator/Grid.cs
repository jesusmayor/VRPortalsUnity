using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int mazeLength;
    int nodeOffset;
    //Data structure to store all nodes and their connections
    private void Start()
    {
        nodeOffset = 10;

        Graph<GraphNode> maze = generateMaze(mazeLength);

        Debug.Log("Number of nodes in the graph = " + maze.getNodes().Count);

    }

    private Graph<GraphNode> generateMaze(int length)//Control hallway lengths to make sense
    {
        Vector3 currentGlobalPosition = Vector3.zero;
        Graph<GraphNode> maze = new Graph<GraphNode>();
        int[] nodeValues;
        for(int i = 0; i < length; i++)
        {
            nodeValues = generateRandomNodeValues();
            Debug.Log("Node number " + i);
            printNodeValues(nodeValues);
            GraphNode node = new GraphNode(currentGlobalPosition, nodeValues[0],nodeValues[1],nodeValues[2],nodeValues[3],nodeValues[4]);
            maze.addNode(node);
            if (maze.getNumberOfNodes() != 1) //If the node isnt the first one, connect it to the node right before itself
            {
                maze.connectNodes(maze.getNodes()[i - 1], node);
            }
            node.render();
            currentGlobalPosition.x += nodeOffset;
        }
        return maze;
    }

    private void printNodeValues(int[] array)
    {
        for(int i = 0; i < array.Length; i++)
        {
            Debug.Log(array[i]);
        }
    }

    private int[] generateRandomNodeValues()//Generates the random values to instantiate a node of the graph
    {
        int[] resul = new int[5];
        int num;

        resul[0] = generateRandomHallwayLength("straight");
        for(int i = 1; i < resul.Length; i++)
        {
            if(i%2 == 0)
            {
                num = generateRandomHallwayLength("side");
            }
            else
            {
                num = generateRandomSideHallwayIndex(resul[0]);
            }
            resul[i] = num;
        }       

        //Temporal fix to avoid straight hallways
        if(resul[1] == resul[3])
        {
            int rand = Random.Range(1,3);
            if (rand == 1)
            {
                resul[3] = 0;
                resul[4] = 0;
            }
            else
            {
                resul[1] = 0;
                resul[2] = 0;
            }
        }
        return resul;
    }

    private int generateRandomHallwayLength(string type)//Generates the length of a specific hallway, either the straight one or the side ones
    {
        int resul;
        if (type == "straight")//Make sure the straight hallway exists
        {
            resul = Random.Range(2, 5); //Main hallway must be at least 2 floors in length
        }
        else
        {
            resul = Random.Range(0, 5); //Side Hallways have none to 4 floors
        }
        return resul;
    }


    private int generateRandomSideHallwayIndex(int straightHallwayLength)//Generate the point where the hallway turns, always less than the straight hallway length
    {
        int resul;
        resul = Random.Range(1,straightHallwayLength + 1); //Side hallways have to be instantiated between the first floor and the last one
        return resul;
    }
}
    
