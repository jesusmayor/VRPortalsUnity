using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;

//This class is used to generate all of the grid in the scene. It generates it depending on the workSpace of the user to keep him inside its limits.
public class Grid_Generator : MonoBehaviour
{
    public Vector2 playerStartPoint;
    public int mazeLength;
    private Vector3 playerCoordinates;//Used to track the position of the player
    private Vector3 startingCoordinates;//Used to track the global coordinates of the nodes
    private int nodeOffset;//X axis offset between nodes
    private int zAxisOffset;
    private int workSpace;//Dimensions of the work space
    private enum nodeDirection { Up, Right, Left, Bottom };//Possible directions of a node
    private nodeDirection currentNodeDirection;//Used to keep track of the direction of the nodes
    private nodeDirection nodeDirectionAux;
    GraphNode.nodeType currentNodeType;
    GraphNode.nodePosition currentNodePos;
    int currentNodeMainHallwayLength;
    bool first;//Used to know if a node is the first one or not
    int mainYIndex;
    int secondaryYIndex;
    private int currentMainSideHallway;
    private bool isCurrentMainSideHallwayBiggest;
    private GraphNode ramificationStart;
    private GraphNode currentNode;
    private Graph<GraphNode> maze;
    public bool leaveCollisionDetected;
    public bool entryCollisionDetected;
    public Transform currentPortalCollided;
    private Graph<GraphNode> auxRamificationGraph = new Graph<GraphNode>();
    public Transform currentNodeTransform;//Only used for debugging
    private GraphNode auxGraphNode;

    private void Start()
    {
        initializeGlobalVariables();

        maze = createLabyrinth(mazeLength,startingCoordinates);

        initializeMaze();

        updateNodesForward();
        connectCurrentNodes();

        Debug.Log("Number of nodes in the graph = " + maze.getNodes().Count);
        Debug.Log("Number of connections in the graph = " + maze.getConnections().Count);

        //exampleNode();
    }

    private void Update()
    {
        if (leaveCollisionDetected)
        {
            Debug.Log("Collision detected, generating next nodes...");
            //Debug.Log("AuxGraphNode = " + auxGraphNode);
            if(auxGraphNode != null && currentPortalCollided == currentNode.getMainHallwayPortal())//Solo debe entrar cuando el pasillo colisionado sea el main
            {
                Debug.Log("Entered auxGraphNode condition");
                currentNode = maze.getNodes()[maze.getNodes().IndexOf(auxGraphNode) + 1];
                auxGraphNode = null;
            }
            else if (currentNode.getNodeType() != GraphNode.nodeType.L)
            {
                if(currentNode.getMainHallwayPortal() == currentPortalCollided)//Main path connection
                {
                    Debug.Log("Main Portal of a ramification node");
                    currentNode = maze.getNodes()[maze.getNodes().IndexOf(currentNode) + 1];
                }
                else//Ramification connection
                {
                    Debug.Log("Ramification Portal");
                    foreach (GraphNode node in currentNode.getConnectedNodes())//Set the current node to the first node of the ramification
                    {
                        if(maze.findConnection(currentNode, node).getNodeA() == currentNode)
                        {
                            currentNode = node;
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Main Portal of an L node");
                if(currentNode.getNodePosition() != GraphNode.nodePosition.F)
                {
                    currentNode = maze.getNodes()[maze.getNodes().IndexOf(currentNode) + 1];
                }
            }
            updateNodesForward();
            connectCurrentNodes();
            leaveCollisionDetected = false;
        }

        if (entryCollisionDetected)
        {
            Debug.Log("Entry Collision Detected");
            updateNodesBackwards();
            connectCurrentNodes();
            entryCollisionDetected = false;
        }
    }

    private void updateNodesBackwards()
    {
        GraphNode aux = new GraphNode();
        int index;

        foreach (GraphNode node in currentNode.getConnectedNodes())//Get the reference to the node that starts a ramification
        {
            GraphConnection<GraphNode> con = maze.findConnection(node, currentNode);
            if (con.getNodeB() == currentNode)
            {
                aux = node;
                break;
            }
        }

        foreach (GraphNode node in currentNode.getConnectedNodes())
        {
            if (node != aux && node != currentNode)
                node.unrender();
        }


        GraphNode node1 = maze.getNodes()[maze.getNodes().IndexOf(currentNode)];
        GraphNode node2 = maze.getNodes()[maze.getNodes().IndexOf(currentNode) - 1];
        GraphConnection < GraphNode > connection = maze.findConnection(node1, node2);

        Debug.Log("Connection = " + connection);
        if(connection == null)
        {
            Debug.Log("Enters");
            auxGraphNode = aux;
        }

        if (maze.getNodes()[maze.getNodes().IndexOf(currentNode) - 1].isRendered() || currentNode.getNodePosition() == GraphNode.nodePosition.F)
            index = maze.getNodes().IndexOf(currentNode) - 1;
        else
            index = maze.getNodes().IndexOf(aux);

        foreach(GraphNode node in maze.getNodes()[index].getConnectedNodes())
        {
            node.render();
        }

        currentNode = aux;
        currentNodeTransform = currentNode.parent.transform;
    }

    private void updateNodesForward()
    {
        GraphNode aux = new GraphNode();
        currentNode.render();
        currentNodeTransform = currentNode.parent.transform;
        foreach(GraphNode node in currentNode.getConnectedNodes())
        {
            node.render();
        }

        foreach (GraphNode node in currentNode.getConnectedNodes())//Get the reference to the node that starts a ramification
        {
            GraphConnection<GraphNode> con = maze.findConnection(node, currentNode);
            if (con.getNodeB() == currentNode)
            {
                aux = node;
                break;
            }
        }

        if (currentNode != maze.getNodes()[0])
        {
            int index;
            if (aux == null)
            {
                Debug.Log("Aux no entra");
                index = maze.getNodes().IndexOf(currentNode) - 1;
            }
            else
            {
                Debug.Log("Aux entra");
                index = maze.getNodes().IndexOf(aux);
            }

            foreach (GraphNode node in maze.getNodes()[index].getConnectedNodes())//Unrender nodes left behind
            {
                if (node != currentNode)
                    node.unrender();
            }
        }
    }

    private void connectCurrentNodes()
    {
        List<GraphConnection<GraphNode>> currentNodeConnections = new List<GraphConnection<GraphNode>>();
        
        foreach(GraphNode node in currentNode.getConnectedNodes())
        {
            currentNodeConnections.Add(maze.findConnection(currentNode, node));
        }

        foreach(GraphConnection<GraphNode> connection in currentNodeConnections)
        {
            connection.connectPortals();
        }
    }

    private void exampleNode()
    {
        List<SideHallway> rightHallways = new List<SideHallway>();
        SideHallway rightHallway1 = new SideHallway("right", 3, 2);
        SideHallway rightHallway2 = new SideHallway("right", 5, 2);
        rightHallways.Add(rightHallway1);
        rightHallways.Add(rightHallway2);

        List<SideHallway> leftHallways = new List<SideHallway>();
        SideHallway leftHallway1 = new SideHallway("left", 5, 2);
        SideHallway leftHallway2 = new SideHallway("left", 7, 3);
        leftHallways.Add(leftHallway1);
        leftHallways.Add(leftHallway2);
        GraphNode node = new GraphNode(Vector3.zero, 7, rightHallways, leftHallways, GraphNode.nodePosition.S);
        node.render();
    }

    public Graph<GraphNode> createLabyrinth(int mazeLength, Vector3 startingCoordinates)//Unify the main path with the ramifications
    {
        Graph<GraphNode> labyrinth;

        labyrinth = generateMaze(mazeLength, startingCoordinates);

        labyrinth.mergeGraph(auxRamificationGraph);

        return labyrinth;
    }

    private Graph<GraphNode> generateMaze(int length, Vector3 startingpoint)//Generates a maze of a determined length, starting at specific coordinates
    {
        Vector3 currentCoordinates = startingpoint;
        Graph<GraphNode> maze = new Graph<GraphNode>();
        Graph<GraphNode> ramificationAux = new Graph<GraphNode>();
        List<List<SideHallway>> sideHallways;
        bool currentNodeIsUnique = false;

        //############################################################################################################################################################################
        //#                                                            Create the nodes                                                                                               #
        //############################################################################################################################################################################
        for (int i = 0; i < length; i++)
        {
            GraphNode.nodePosition nodePos;
            if (i == 0 && first )
            {
                nodePos = GraphNode.nodePosition.S;
                if (length == 1)
                    currentNodeIsUnique = true;
            }
            else if (i == length - 1)
                nodePos = GraphNode.nodePosition.F;
            else
                nodePos = GraphNode.nodePosition.I;

            currentNodePos = nodePos;

            Debug.Log("Node number " + i + " Player Coordinates = " + playerCoordinates.x + "," + playerCoordinates.y);
            Debug.Log("Node position = " + nodePos);
            if (playerCoordinates.x < 1 || playerCoordinates.x > workSpace || playerCoordinates.y < 1 || playerCoordinates.y > workSpace)
                Debug.LogError("Out of work space");
            sideHallways = randomNode();
            Debug.Log("Node type = " + currentNodeType);
            //printNodeValues(nodeValues);
            GraphNode node = new GraphNode(currentCoordinates, currentNodeMainHallwayLength, sideHallways[0], sideHallways[1], nodePos);

            if (currentNodeIsUnique)
                node.setNodeAsUnique();

            if (ramificationStart != null)//This means a new ramification started
            {
                maze.connectNodes(ramificationStart, node);
                maze.findConnection(ramificationStart, node).setRamificationConnection();
                ramificationStart = null;
            }

            if (currentNodeType == GraphNode.nodeType.F)//If last node generated was an F type, generate the path connected to it
            {
                ramificationStart = node;
                ramificationAux = generateNewPathForFNode();
            }
            else if (currentNodeType == GraphNode.nodeType.T)//If last node generated was a T type, generate the path connected to it
            {
                ramificationStart = node;
                ramificationAux = generateNewPathForTNode();
            }

            maze.addNode(node);
            
            if (maze.getNumberOfNodes() != 1) //If the node isnt the first one, connect it to the previous one
            {
                maze.connectNodes(maze.getNodes()[i - 1], node);
            }

            if(ramificationAux != null)//If a ramification was just created, add its nodes to the maze
            {
                auxRamificationGraph.mergeGraph(ramificationAux);
                ramificationAux = null;
            }

            currentCoordinates.x += nodeOffset;//Add offset to the x axis to avoid overlapping
        }
        return maze;
    }

    private List<List<SideHallway>> randomNode()//Takes work space limitation into account
    {
        List<List<SideHallway>> resul = new List<List<SideHallway>>();
        int x = 0;//x movement generated
        int y = 0;//y movemement generated
        int turnDecision = -1;//Controls whether a node turns left or right if both options are possible
        List<SideHallway> rightHallways = new List<SideHallway>();
        List<SideHallway> leftHallways = new List<SideHallway>();
        int nodeTypeDecision;
        int straightHallwayLength;
        int mainSideHallway = -1;
        int secondaryHallway = -1;
        bool alreadyTurned = false;//Used to force algorythm stop after deciding a T node

        straightHallwayLength = generateRandomHallwayLength();//Generate the main hallway length
        currentNodeMainHallwayLength = straightHallwayLength;

        Debug.Log("Current node direction = " + currentNodeDirection);
        Debug.Log("CanTurnLeft = " + canTurnLeft());
        Debug.Log("CanTurnRight = " + canTurnRight());

        if (canTurnLeft() && canTurnRight())//If node can turn in both directions, turn to both or to a random one or both
        {
            if(Random.Range(0,2) == 0)
            {
                turnDecision = Random.Range(0, 2);
            }
            else
            {
                if (multipleSideHallwayNodeFits(straightHallwayLength))//Node generated is T form
                {
                    alreadyTurned = true;
                    int firstIndex = generateRandomSideHallwayIndex(straightHallwayLength);
                    int secondIndex;
                    mainSideHallway = Random.Range(0,2);
                    currentMainSideHallway = mainSideHallway;
                    currentNodeType = GraphNode.nodeType.T;
                    SideHallway leftHallway = new SideHallway("left", firstIndex, generateRandomSideHallwayLength());

                    //Create the second index for the second side hallway so it can never be the same as the first one
                    if (firstIndex != straightHallwayLength && firstIndex != 1)
                    {
                        if (Random.Range(0, 2) == 0)
                            secondIndex = Random.Range(1, firstIndex);
                        else
                            secondIndex = Random.Range(firstIndex + 1, straightHallwayLength + 1);
                    }
                    else if (firstIndex == 1)
                        secondIndex = Random.Range(firstIndex + 1, straightHallwayLength + 1);
                    else
                        secondIndex = Random.Range(1,straightHallwayLength);
                 

                    SideHallway rightHallway = new SideHallway("right", secondIndex, generateRandomSideHallwayLength());

                    leftHallways.Add(leftHallway);
                    rightHallways.Add(rightHallway);

                    nodeDirectionAux = currentNodeDirection;

                    if (mainSideHallway == 0)//Left side hallway is the main one
                    {
                        mainYIndex = firstIndex;
                        secondaryYIndex = secondIndex;
                        if (secondIndex > firstIndex)
                            isCurrentMainSideHallwayBiggest = true;
                        else
                            isCurrentMainSideHallwayBiggest = false;

                        x = leftHallways[0].getHallwayLength();//x generated equals the length of the side hallway
                        y = leftHallways[0].getTurnIndex();//y generated is equal to the point where the side hallway is created

                        leftHallways[0].setAsMain();

                        updateCoordinatesForLeftTurn(x, y);
                    }
                    else//Right side hallway is the main one
                    {
                        mainYIndex = secondIndex;
                        secondaryYIndex = firstIndex;
                        if (firstIndex > secondIndex)
                            isCurrentMainSideHallwayBiggest = true;
                        else
                            isCurrentMainSideHallwayBiggest = false;

                        x = rightHallways[0].getHallwayLength();//x generated equals the length of the side hallway
                        y = rightHallways[0].getTurnIndex();//y generated is equal to the point where the side hallway is created

                        rightHallways[0].setAsMain();

                        updateCoordinatesForRightTurn(x, y);
                    }
                }
                else
                {
                    turnDecision = Random.Range(0, 2);
                }
            }
        }

        //Debugs to see how conditions are working
        //Debug.Log("Main hallway = " + straightHallwayLength);
        //Debug.Log("CurrentNodeDirection = "+ currentNodeDirection);
        //Debug.Log("turnDecision = " + turnDecision);

        if ((canTurnLeft() && !canTurnRight()) && !alreadyTurned || turnDecision == 0) //Turn to the left
        {
            nodeTypeDecision = Random.Range(0, 2);
            if (nodeTypeDecision == 0 && multipleSideHallwayNodeFits(straightHallwayLength))//Node generated is F form
            {
                mainSideHallway = Random.Range(0, 2);
                if (mainSideHallway == 1)
                    secondaryHallway = 0;
                else
                    secondaryHallway = 1;
                Debug.Log("MainSideHallway = " + mainSideHallway);
                Debug.Log("SecondarySideHallway = " + secondaryHallway);
                currentMainSideHallway = mainSideHallway;
                currentNodeType = GraphNode.nodeType.F;
                leftHallways = generateFNodeSideHallways(straightHallwayLength, "left");
                leftHallways[mainSideHallway].setAsMain();
                mainYIndex = leftHallways[mainSideHallway].getTurnIndex();
                secondaryYIndex = leftHallways[secondaryHallway].getTurnIndex();
            }
            else//Node generated is L form
            {
                mainSideHallway = 0;
                currentMainSideHallway = 0;
                currentNodeType = GraphNode.nodeType.L;
                SideHallway leftHallway = new SideHallway("left", generateRandomSideHallwayIndex(straightHallwayLength), generateRandomSideHallwayLength());
                if (currentNodePos != GraphNode.nodePosition.F)
                    leftHallway.setAsMain();
                leftHallways.Add(leftHallway);
            }


            Debug.Log("Left");

            x = leftHallways[mainSideHallway].getHallwayLength();//x generated equals the length of the side hallway
            y = leftHallways[mainSideHallway].getTurnIndex();//y generated is equal to the point where the side hallway is created

            nodeDirectionAux = currentNodeDirection;

            updateCoordinatesForLeftTurn(x, y);
        }
        else if ((canTurnRight() && !canTurnLeft()) && !alreadyTurned || turnDecision == 1)//Turn to the right
        {

            nodeTypeDecision = Random.Range(1, 3);
            if (nodeTypeDecision == 1 && multipleSideHallwayNodeFits(straightHallwayLength))
            {
                mainSideHallway = Random.Range(0, 2);
                if (mainSideHallway == 1)
                    secondaryHallway = 0;
                else
                    secondaryHallway = 1;

                Debug.Log("MainSideHallway = " + mainSideHallway);
                Debug.Log("SecondarySideHallway = " + secondaryHallway);
                currentMainSideHallway = mainSideHallway;
                currentNodeType = GraphNode.nodeType.F;
                rightHallways = generateFNodeSideHallways(straightHallwayLength, "right");
                rightHallways[mainSideHallway].setAsMain();
                mainYIndex = rightHallways[mainSideHallway].getTurnIndex();
                secondaryYIndex = rightHallways[secondaryHallway].getTurnIndex();
            }
            else
            {
                mainSideHallway = 0;
                currentMainSideHallway = 0;
                currentNodeType = GraphNode.nodeType.L;
                SideHallway rightHallway = new SideHallway("right", generateRandomSideHallwayIndex(straightHallwayLength), generateRandomSideHallwayLength());
                if(currentNodePos != GraphNode.nodePosition.F)
                    rightHallway.setAsMain();
                rightHallways.Add(rightHallway);
            }


            Debug.Log("Right");

            x = rightHallways[mainSideHallway].getHallwayLength();//x generated equals the length of the side hallway
            y = rightHallways[mainSideHallway].getTurnIndex();

            nodeDirectionAux = currentNodeDirection;

            updateCoordinatesForRightTurn(x, y);
        }


        Debug.Log("Hallway Length = " + straightHallwayLength);
        Debug.Log("MainSideHallway = " + mainSideHallway);
        Debug.Log("x = " + x);
        Debug.Log("y = " + y);


        resul.Add(rightHallways);
        resul.Add(leftHallways);
        return resul;
    }

    private bool canTurnLeft()
    {
        bool resul = false;

        switch (currentNodeDirection)
        {
            case nodeDirection.Up:
                resul = (int)playerCoordinates.x  - 1> 2;
                break;
            case nodeDirection.Right:
                resul = workSpace - (int)playerCoordinates.y > 2;
                break;
            case nodeDirection.Left:
                resul = playerCoordinates.y - 1> 2;
                break;
            case nodeDirection.Bottom:
                resul = workSpace - (int)playerCoordinates.x > 2;
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
                resul = workSpace - playerCoordinates.x > 2;
                break;
            case nodeDirection.Right:
                resul = (int)playerCoordinates.y - 1> 2;
                break;
            case nodeDirection.Left:
                resul = workSpace - playerCoordinates.y > 2;
                break;
            case nodeDirection.Bottom:
                resul = (int)playerCoordinates.x  - 1> 2;
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
                resul = Random.Range(2, workSpace - (int)playerCoordinates.y);
                break;
            case nodeDirection.Right:
                resul = Random.Range(2, workSpace - (int)playerCoordinates.x);
                break;
            case nodeDirection.Left:
                resul = Random.Range(2, (int)playerCoordinates.x - 1);
                break;
            case nodeDirection.Bottom:
                resul = Random.Range(2, (int)playerCoordinates.y - 1);
                break;
        }
        return resul;
    }

    private int generateRandomSideHallwayIndex(int straightHallwayLength)//Generate the point where the hallway turns, always less than the straight hallway length
    {
        return Random.Range(2, straightHallwayLength + 1); //Side hallways have to be instantiated between the first and the last floor of the straight hallway
    }

    private List<SideHallway> generateFNodeSideHallways(int straightHallwayLength, string direction)
    {
        List<SideHallway> resul = new List<SideHallway>();

        if (straightHallwayLength != 3)
        {
            currentNodeType = GraphNode.nodeType.F;
            int firstIndex = Random.Range(1,straightHallwayLength/2);
            int secondIndex = Random.Range(straightHallwayLength/2 + 1, straightHallwayLength + 1);
            
            SideHallway hallway1 = new SideHallway(direction, firstIndex, generateRandomSideHallwayLength());
            SideHallway hallway2 = new SideHallway(direction, secondIndex, generateRandomSideHallwayLength());

            resul.Add(hallway1);
            resul.Add(hallway2);
        }
        else
        {
            SideHallway hallway1 = new SideHallway(direction, 1, generateRandomSideHallwayLength());
            SideHallway hallway2 = new SideHallway(direction, 3, generateRandomSideHallwayLength());

            resul.Add(hallway1);
            resul.Add(hallway2);
        }


        return resul;
    }

    private void updateCoordinatesForLeftTurn(int x, int y)
    {
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

    private void updateCoordinatesForRightTurn(int x, int y)
    {
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

    private bool multipleSideHallwayNodeFits(int straightHallwayLength)
    {
        bool resul = false;

        if (straightHallwayLength > 2)
            resul = true;

        return resul;
    }

    private Graph<GraphNode> generateNewPathForTNode()
    {
        Vector3 auxCoord = playerCoordinates;
        nodeDirection directionAux = currentNodeDirection;
        GraphNode.nodeType auxNodeType = currentNodeType;
        GraphNode.nodePosition auxNodePos = currentNodePos;
        int difference;
        Graph<GraphNode> auxGraph;

        Debug.Log("MainYOffset = " + mainYIndex);
        Debug.Log("Coordinates before adjusting to not main path = " + playerCoordinates);

        //Adjust the coordinates to match the opposite side hallway of the main side hallway
        if (currentMainSideHallway == 1)//Main side hallway is the right one
        {
            switch (nodeDirectionAux)
            {
                case nodeDirection.Up:
                    playerCoordinates.x -= 2;
                    break;
                case nodeDirection.Right:
                    playerCoordinates.y += 2;
                    break;
                case nodeDirection.Left:
                    playerCoordinates.y -= 2;
                    break;
                case nodeDirection.Bottom:
                    playerCoordinates.x += 2;
                    break;
            }
        }
        else
        {
            switch (nodeDirectionAux)//Main side hallway is the left one
            {
                case nodeDirection.Up:
                    playerCoordinates.x += 2;
                    break;
                case nodeDirection.Right:
                    playerCoordinates.y -= 2;
                    break;
                case nodeDirection.Left:
                    playerCoordinates.y += 2;
                    break;
                case nodeDirection.Bottom:
                    playerCoordinates.x -= 2;
                    break;
            }
        }

        if (!isCurrentMainSideHallwayBiggest)//If the new path starts from the first side hallway
        {
            difference = mainYIndex - secondaryYIndex;
            switch (nodeDirectionAux)
            {
                case nodeDirection.Up:
                    playerCoordinates.y -= difference;
                    break;
                case nodeDirection.Right:
                    playerCoordinates.x -= difference;
                    break;
                case nodeDirection.Left:
                    playerCoordinates.x += difference ;
                    break;
                case nodeDirection.Bottom:
                    playerCoordinates.y += difference;
                    break;
            }
        }
        else//If the new path starts from the second side hallway
        {
            difference = secondaryYIndex - mainYIndex;
            switch (nodeDirectionAux)
            {
                case nodeDirection.Up:
                    playerCoordinates.y += difference;
                    break;
                case nodeDirection.Right:
                    playerCoordinates.x += difference;
                    break;
                case nodeDirection.Left:
                    playerCoordinates.x -= difference;
                    break;
                case nodeDirection.Bottom:
                    playerCoordinates.y -= difference;
                    break;
            }
        }



        Debug.Log("Starting Path with coordinates = " + playerCoordinates);
        zAxisOffset += 10;
        auxGraph = generateMaze(Random.Range(1, 6), new Vector3(startingCoordinates.x, startingCoordinates.y, startingCoordinates.z + zAxisOffset));
        playerCoordinates = auxCoord;
        currentNodeDirection = directionAux;
        currentNodeType = auxNodeType;
        currentNodePos = auxNodePos;

        return auxGraph;
    }

    private Graph<GraphNode> generateNewPathForFNode()
    {
        Vector3 auxCoord = playerCoordinates;
        nodeDirection directionAux = currentNodeDirection;
        GraphNode.nodeType auxNodeType = currentNodeType;
        GraphNode.nodePosition auxNodePos = currentNodePos;
        int difference;
        Graph<GraphNode> auxGraph;

        Debug.Log("MainYOffset = " + mainYIndex);
        Debug.Log("Coordinates before adjusting to not main path = " + playerCoordinates);

    
        if (currentMainSideHallway == 1)//If the new path starts from the first side hallway
        {
            difference = mainYIndex - secondaryYIndex;
            switch (nodeDirectionAux)
            {
                case nodeDirection.Up:
                    playerCoordinates.y -= difference;
                    break;
                case nodeDirection.Right:
                    playerCoordinates.x -= difference;
                    break;
                case nodeDirection.Left:
                    playerCoordinates.x += difference;
                    break;
                case nodeDirection.Bottom:
                    playerCoordinates.y += difference;
                    break;
            }
        }
        else//If the new path starts from the second side hallway
        {
            difference = secondaryYIndex - mainYIndex;
            switch (nodeDirectionAux)
            {
                case nodeDirection.Up:
                    playerCoordinates.y += difference;
                    break;
                case nodeDirection.Right:
                    playerCoordinates.x += difference;
                    break;
                case nodeDirection.Left:
                    playerCoordinates.x -= difference;
                    break;
                case nodeDirection.Bottom:
                    playerCoordinates.y -= difference;
                    break;
            }
        }

        Debug.Log("Starting Path with coordinates = " + playerCoordinates);
        Debug.Log("Difference = " + difference);
        zAxisOffset += 10;
        auxGraph = generateMaze(Random.Range(1,6), new Vector3(startingCoordinates.x, startingCoordinates.y, startingCoordinates.z + zAxisOffset));
        playerCoordinates = auxCoord;
        currentNodeDirection = directionAux;
        currentNodeType = auxNodeType;
        currentNodePos = auxNodePos;

        return auxGraph;
    }
    private int getWorkSpace()//Returns the dimensions of the workSpace
    {
        return 7;
    }

    private void initializeGlobalVariables()
    {
        workSpace = getWorkSpace();
        playerCoordinates = playerStartPoint;//Sets the coordinates to the starting point
        startingCoordinates = Vector3.zero;
        nodeOffset = 10;
        currentNodeDirection = nodeDirection.Up;//First node is always in the "up" direction
        first = true;
        ramificationStart = null;
    }

    private void initializeMaze()
    {
        currentNode = maze.getNodes()[0];
        leaveCollisionDetected = false;
        entryCollisionDetected = false;
    }
}

