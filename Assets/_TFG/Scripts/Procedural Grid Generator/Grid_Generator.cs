using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

//This class is used to generate all of the grid in the scene. It generates it depending on the workSpace of the user to keep him inside its limits.
public class Grid_Generator : MonoBehaviour
{
    public Vector3 playerStartPoint;
    public int mazeLength;
    private Vector3 playerCoordinates;//Used to track the position of the player
    private Vector3 globalCoordinates;//Used to track the global coordinates of the nodes
    int nodeOffset;//X axis offset between nodes
    int workSpace;//Dimensions of the work space
    enum nodeDirection { Up, Right, Left, Bottom };//Possible directions of a node
    nodeDirection currentNodeDirection;//Used to keep track of the direction of the nodes
    bool first;//Used to know if a node is the first one or not

    private void Start()
    {
        //Global variables set
        workSpace = getWorkSpace();
        playerCoordinates = playerStartPoint;//Sets the coordinates to the starting point
        globalCoordinates = Vector3.zero;
        nodeOffset = 10;
        currentNodeDirection = nodeDirection.Up;//First node is always in the "up" direction
        first = true;

        Graph<GraphNode> maze = generateMaze(mazeLength);

        Debug.Log("Number of nodes in the graph = " + maze.getNodes().Count);

    }

    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(10);
    }

    private Graph<GraphNode> generateMaze(int length)//Generates a maze of a determined length
    {    
        Graph<GraphNode> maze = new Graph<GraphNode>();
        int[] nodeValues;
        //############################################################################################################################################################################
        //#                                                            Create the nodes                                                                                               #
        //############################################################################################################################################################################
        for (int i = 0; i < length; i++)
        {
            Debug.Log("Node number " + i + " Player Coordinates = " + playerCoordinates.x + "," + playerCoordinates.y);
            if (playerCoordinates.x < 1 || playerCoordinates.x > workSpace || playerCoordinates.y < 1 || playerCoordinates.y > workSpace)
                Debug.LogError("Out of work space");
            nodeValues = randomNode();
            //printNodeValues(nodeValues);
            GraphNode node = new GraphNode(globalCoordinates, nodeValues[0], nodeValues[1], nodeValues[2], nodeValues[3], nodeValues[4]);
            maze.addNode(node);
            if (maze.getNumberOfNodes() != 1) //If the node isnt the first one, connect it to the node right before itself
            {
                maze.connectNodes(maze.getNodes()[i - 1], node);
            }
            node.render();
            globalCoordinates.x += nodeOffset;
        }
        //############################################################################################################################################################################
        //#                                                            Connect the portals                                                                                           #
        //############################################################################################################################################################################
        List<GraphNode> nodes = maze.getNodes();

        for (int i = 0; i < length - 1; i++)
        {
            nodes[i].connectTo(nodes[i + 1]);
        }

        return maze;
    }

    private int[] randomNode()//Takes work space limitation into account
    {
        int[] resul = new int[5];
        int x = 0;//x movement generated
        int y = 0;//y movemement generated
        int turnDecision = -1;//Controls whether a node turns left or right if both options are possible

        if (canTurnLeft() && canTurnRight())//If node can turn in both directions, turn to a random one
        {
            turnDecision = Random.Range(0, 2);
        }

        resul[0] = generateRandomHallwayLength();//Generate the main hallway length

        //Debugs to see how conditions are working
        Debug.Log("Main hallway = " + resul[0]);
        Debug.Log("CurrentNodeDirection = "+ currentNodeDirection);
        Debug.Log("CanTurnLeft = " + canTurnLeft());
        Debug.Log("CanTurnRight = " + canTurnRight());
        Debug.Log("turnDecision = " + turnDecision);

        if ((canTurnLeft() && !canTurnRight()) || turnDecision == 0) //Turn to the left
        {
            Debug.Log("L");
            //First 2 positions of the array are 0 because the node turned left, so it doesn´t turn right
            resul[1] = 0;
            resul[2] = 0;
            resul[3] = generateRandomSideHallwayIndex(resul[0]);//Decides where the side hallway is created
            resul[4] = generateRandomSideHallwayLength();//Generates the lenght of the side hallway

            x = resul[4];//x generated equals the length of the side hallway
            y = resul[3];//y generated is equal to the point where the side hallway is created

            if (!first)//If its not the first node, set the coordinates depending on the direction
            {
                if (currentNodeDirection == nodeDirection.Up)
                {
                    playerCoordinates.x -= x;
                    playerCoordinates.y += y;
                    currentNodeDirection = nodeDirection.Left;
                }
                else if (currentNodeDirection == nodeDirection.Right)
                {
                    playerCoordinates.x += y;
                    playerCoordinates.y += x;
                    currentNodeDirection = nodeDirection.Up;
                }
                else if (currentNodeDirection == nodeDirection.Left)
                {
                    playerCoordinates.x -= y;
                    playerCoordinates.y -= x;
                    currentNodeDirection = nodeDirection.Bottom;
                }
                else if (currentNodeDirection == nodeDirection.Bottom)
                {
                    playerCoordinates.x += x;
                    playerCoordinates.y -= y;
                    currentNodeDirection = nodeDirection.Right;
                }
            }
            else//If its the first node, substract 1 to the y value since its already in the coordinates
            {
                playerCoordinates.x -= x;
                playerCoordinates.y += y - 1;
                first = false;
                currentNodeDirection = nodeDirection.Left;
            }
        }
        else if((canTurnRight() && !canTurnLeft()) || turnDecision == 1)//Turn to the right
        {
            Debug.Log("R");
            resul[1] = generateRandomSideHallwayIndex(resul[0]);
            resul[2] = generateRandomSideHallwayLength();
            resul[3] = 0;
            resul[4] = 0;

            x = resul[2];
            y = resul[1];

            if (!first)
            {
                if (currentNodeDirection == nodeDirection.Up)
                {
                    playerCoordinates.x += x;
                    playerCoordinates.y += y;
                    currentNodeDirection = nodeDirection.Right;
                }
                else if (currentNodeDirection == nodeDirection.Right)
                {
                    playerCoordinates.x += y;
                    playerCoordinates.y -= x;
                    currentNodeDirection = nodeDirection.Bottom;
                }
                else if (currentNodeDirection == nodeDirection.Left)
                {
                    playerCoordinates.x -= y;
                    playerCoordinates.y += x;
                    currentNodeDirection = nodeDirection.Up;
                }
                else if (currentNodeDirection == nodeDirection.Bottom)
                {
                    playerCoordinates.x -= x;
                    playerCoordinates.y -= y;
                    currentNodeDirection = nodeDirection.Left;
                }
            }
            else
            {
                playerCoordinates.x += x;
                playerCoordinates.y += y - 1;
                first = false;
                currentNodeDirection = nodeDirection.Right;
            }
        }
        Debug.Log("x = " + x);
        Debug.Log("y = " + y);
        return resul;
    }

    private bool canTurnLeft()
    {
        bool resul = false;

        switch (currentNodeDirection)
        {
            case nodeDirection.Up:
                resul = (int)playerCoordinates.x > 2;
                break;
            case nodeDirection.Right:
                resul = workSpace - (int)playerCoordinates.y + 1 > 2;
                break;
            case nodeDirection.Left:
                resul = playerCoordinates.y > 2;
                break;
            case nodeDirection.Bottom:
                resul = workSpace - (int)playerCoordinates.x + 1> 2;
                break;
        }
        return resul;
    }

    private bool canTurnRight()
    {
        bool resul = false;


        switch (currentNodeDirection)
        {
            case nodeDirection.Up:
                resul = workSpace - playerCoordinates.x + 1 > 2;
                break;
            case nodeDirection.Right:
                resul = (int)playerCoordinates.y > 2;
                break;
            case nodeDirection.Left:
                resul = workSpace - playerCoordinates.y + 1 > 2;
                break;
            case nodeDirection.Bottom:
                resul = (int)playerCoordinates.x > 2;
                break;
        }
        return resul;
    }


    private int generateRandomSideHallwayLength()//Generates the length of the side hallway
    {
        return 1;
    }

    private int generateRandomHallwayLength()//Generates the length of the main hallway
    {
        int resul = -1;
        switch (currentNodeDirection)
            {
              case nodeDirection.Up:
                resul = Random.Range(1, workSpace - (int)playerCoordinates.y + 1);
                break;
              case nodeDirection.Right:
                resul = Random.Range(1, workSpace - (int)playerCoordinates.x + 1);
                break;
              case nodeDirection.Left:
                resul = Random.Range(1, (int)playerCoordinates.x);
                break;
              case nodeDirection.Bottom:
                resul = Random.Range(1,(int)playerCoordinates.y);
                break;
            }
        return resul;
    }

    private int generateRandomSideHallwayIndex(int straightHallwayLength)//Generate the point where the hallway turns, always less than the straight hallway length
    {
        return Random.Range(1, straightHallwayLength + 1); //Side hallways have to be instantiated between the first and the last floor of the straight hallway
    }

    private int getWorkSpace()//Returns the dimensions of the workSpace
    {
        return 8;
    }
}
    
