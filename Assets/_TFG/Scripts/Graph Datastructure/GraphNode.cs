using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEditor;
using UnityEngine;
using System.Linq;

//This class is used to store the characterisics of the sections that form the maze. It is also used to render or unrender each section separately.

public class GraphNode
{
    protected Object portal;//Reference to the prefab
    public enum nodeType { L, T, F };
    public enum nodePosition { S, I, F };
    protected List<GraphNode> connectedNodes;//List of the nodes connected to this node
    protected Transform entryPortal;//Portal used to enter the node
    protected List<Transform> leavePortals;//List of portals used to leave the node
    protected List<SideHallway> rightHallways;//List of rightHallways of the node
    protected List<SideHallway> leftHallways;//List of leftHallways of the node
    protected GameObject parent;//Empty GameObject to store the node
    protected bool hasIndicator;
    protected GameObject indicator;
    protected bool indicatorActivated;

    protected Vector3 currentWorldCoordinates;//Store here the global position where the node should be instantiated (Basically its (0,0,0) coordinates)
    protected nodePosition nodePos;
    protected nodeType nodeForm;
    protected int straightHallwayLength;//Straight hallway length
    protected int height;//Height of the node
    protected Color color;//Color of the node(generated randomly)
    protected bool uniqueNode;//Used if there�s only one node for the main path


    public GraphNode()
    {
        connectedNodes = new List<GraphNode>();
    }

    public GraphNode(Vector3 startingPoint, int hallwayLength, List<SideHallway> rightHallways, List<SideHallway> leftHallways, nodePosition nodePos) : this()
    {
        currentWorldCoordinates = startingPoint;
        straightHallwayLength = hallwayLength;
        this.rightHallways = rightHallways;
        this.leftHallways = leftHallways;
        this.nodePos = nodePos;
        nodeForm = setNodeType();
        height = 3;
        color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
        portal = Resources.Load("Portal");
        leavePortals = new List<Transform>();
    }

    public bool addConnectedNode(GraphNode node)
    {
        if (node != this && !connectedNodes.Contains(node))
        {
            connectedNodes.Add(node);
            return true;
        }
        return false;
    }

    public void removeConnectedNode(GraphNode node)
    {
        connectedNodes.Remove(node);
    }

    public bool isConnectedTo(GraphNode node)
    {
        return connectedNodes.Contains(node);
    }

    public List<GraphNode> getConnectedNodes()
    {
        return connectedNodes;
    }

    public void render()//Render this node
    {
        if (parent == null)//If parent is null, this node was unrendered previously so we need to create the parent gameObject before rendering it
        {
            createParentGameObject();
            renderstraightHallway(straightHallwayLength);//Render the straight hallway
            renderSideHallways(rightHallways);//Render right hallways
            renderSideHallways(leftHallways);//Render left hallways
        }
        if (hasIndicator && indicator == null)//Generate the indicator if this is the start of the end of the maze
        {
            GameObject aux = GameObject.Instantiate((GameObject)Resources.Load("ActivateThis"));
            aux.transform.parent = parent.transform;
            aux.transform.position = currentWorldCoordinates;
            aux.transform.position += (new Vector3(0.5f, 0.5f, 0.5f));

            if (currentWorldCoordinates != Vector3.zero)//If its the end of the maze, move the indicator to the end of the hallway
            {
                aux.transform.position += new Vector3(0, 0, straightHallwayLength - 1);
            }

            if (indicatorActivated)//To maintain the state of the activation
            {
                aux.GetComponent<Light>().color = Color.green;
                aux.GetComponent<MeshRenderer>().material = Resources.Load("Green", typeof(Material)) as Material;
                Debug.Log(aux.GetComponent<Light>().color);
            }

            indicator = aux;
        }
    }
    public void unrender()//Unrender this node
    {
        if (hasIndicator)
        {
            if (indicator.GetComponent<Light>().color == Color.green)
            {
                Debug.Log("Set as activated");
                indicatorActivated = true;
            }
        }
       
        Object.Destroy(parent);
        parent = null;
        indicator = null;
    }
    private void renderstraightHallway(int length)//Create the straight hallway of the node
    {
        Vector3 currentPos = currentWorldCoordinates;//Track current position
        Vector3 wallRotation = new Vector3(0, 0, 90);//Indicates rotation for walls for the straight hallway

        for (int i = 0; i < length; i++)
        {
            //Create floor instance
            createFloor(currentPos, "Floor");//Create the floor
            createFloor(new Vector3(currentPos.x, currentPos.y + height + 0.5f, currentPos.z), "Ceiling");//Create the ceiling

            if (i == 0)//If this is the first floor, instantiate the walls at start, always 1 less than the height of the node. Also create the entry portal
            {
                if (nodePos == nodePosition.S || uniqueNode)
                    createWall(new Vector3(currentPos.x, currentPos.y + 1, currentPos.z), new Vector3(90, 0, 0), height);
                else
                {
                    //createWall(new Vector3(currentPos.x, currentPos.y + 2, currentPos.z), new Vector3(0, 90, 90), height - 2);
                    createPortal(new Vector3(currentPos.x + 0.5f, currentPos.y + 1, currentPos.z), Vector3.zero, 0);
                }
            }

            if(!isTurning(i + 1,leftHallways))//If node doesn�t turn left at this position, create a wall in the left
                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y,currentPos.z), wallRotation, height);

            if (!isTurning(i + 1, rightHallways))//If node doesn�t turn right at this position, create a wall in the right
                createWall(new Vector3(currentPos.x + 1, currentPos.y, currentPos.z), wallRotation, height);

            if (i == length - 1)//If this is the last floor, create a wall in the end
                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), new Vector3(0, 90, 90), height);

            currentPos.z++;//Move forward 1 time
        }
    }

    private void renderSideHallways(List<SideHallway> hallways)//Calculate where the turned hallways are, their lengths and instantiates them
    {
        //Current position to instantiate several floors
        Vector3 currentPos;
        //Base rotations needed
        Vector3 wallRotation = new Vector3(0, 90, 90);
        Vector3 leftPortalRotation = new Vector3(0,90,0);
        Vector3 rightPortalRotation = new Vector3(0, -90, 0);

        if (!IsEmpty(hallways))
        {
            string direction = hallways[0].getSideValue();
            for (int h = 0; h < hallways.Count; h++)
            {
                currentPos = currentWorldCoordinates;
                int length = hallways[h].getHallwayLength();
                int turnIndex = hallways[h].getTurnIndex();
                SideHallway currentHallway = hallways[h];
                if (direction == "right")//If direction is right, set up coordinates to instantiate the right hallway
                {
                    currentPos.x++;
                    currentPos.z = currentPos.z + turnIndex - 1;
                }
                else if (direction == "left")//If direction is left, set up coordinates to instantiate the left hallway
                {
                    currentPos.x--;
                    currentPos.z = currentPos.z + turnIndex - 1;
                }

                for (int i = 0; i < length; i++)//Iterate over the number of floors and instantiate them
                {
                    //Create floor instance
                    createFloor(currentPos, "Floor");//Create the floor
                    createFloor(new Vector3(currentPos.x, currentPos.y + height + 0.5f, currentPos.z), "Ceiling");//Create the ceiling

                    if (i == length - 1)//If its the last floor of the side hallway, create the end walls and the exit portals
                    {
                        if (direction == "left")
                        {
                            if (nodePos != nodePosition.F && !uniqueNode)//If this node isnt the last one or if this hallway is the main one, open the sidehallway with a portal
                            {
                                //createWall(new Vector3(currentPos.x - 0.5f, currentPos.y + 2, currentPos.z), new Vector3(0, 0, 90), height - 2);
                                hallways[h].setLeavePortal(createPortal(new Vector3(currentPos.x, currentPos.y + 1, currentPos.z + 0.5f), leftPortalRotation, 1));
                            }
                            else if(nodeForm != nodeType.L)//If this node has ramifications
                            {
                                if(!hallways[h].isMain())//If this hallway isnt the main one open a portal for the ramification
                                {
                                    //createWall(new Vector3(currentPos.x - 0.5f, currentPos.y + 2, currentPos.z), new Vector3(0, 0, 90), height - 2);
                                    hallways[h].setLeavePortal(createPortal(new Vector3(currentPos.x, currentPos.y + 1, currentPos.z + 0.5f), leftPortalRotation, 1));
                                }
                                else
                                {
                                    createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z), new Vector3(0, 0, 90), height);
                                }

                            }
                            else//In this case its a node in L form at the last position, so we close it
                            {
                                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z), new Vector3(0, 0, 90), height);
                            }

                        }
                        else//Same but coordinates for right hallways
                        {
                            if (nodePos != nodePosition.F && !uniqueNode)
                            {
                                //createWall(new Vector3(currentPos.x + 1, currentPos.y + 2, currentPos.z), new Vector3(0, 0, 90), height - 2);
                                hallways[h].setLeavePortal(createPortal(new Vector3(currentPos.x + 1, currentPos.y + 1, currentPos.z + 0.5f), rightPortalRotation, 1));
                            }
                            else if (nodeForm != nodeType.L)
                            {
                                if (!hallways[h].isMain())
                                {
                                    //createWall(new Vector3(currentPos.x + 1, currentPos.y + 2, currentPos.z), new Vector3(0, 0, 90), height - 2);
                                    hallways[h].setLeavePortal(createPortal(new Vector3(currentPos.x + 1, currentPos.y + 1, currentPos.z + 0.5f), rightPortalRotation, 1));
                                }
                                else
                                {
                                    createWall(new Vector3(currentPos.x + 1, currentPos.y, currentPos.z), new Vector3(0, 0, 90), height);
                                }
                            }
                            else
                                createWall(new Vector3(currentPos.x + 1, currentPos.y, currentPos.z), new Vector3(0, 0, 90), height);
                        }

                    }
                    if (i == 0 && length != 0 && direction == "right")//To avoid overlapping of walls at the right hallway
                    {
                        if (turnIndex != 1 && nodeForm != nodeType.F)
                            createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                        else if(nodeForm == nodeType.F)
                        {
                            
                            if(fNodeHasContigousSideHallways(rightHallways[0], rightHallways[1]))//If the F section has contigous sideHallways
                            {
                                if(currentHallway == rightHallways[1])
                                {
                                    createWall(new Vector3 (currentPos.x, currentPos.y, currentPos.z + 0.025f), wallRotation, height, 0.95f);
                                }
                                else
                                {
                                    createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                                }
                            }
                            else
                            {
                                createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                            }
                        }
                        if(nodeForm == nodeType.F && fNodeHasContigousSideHallways(rightHallways[0], rightHallways[1]))
                        {
                            if(turnIndex != straightHallwayLength && currentHallway == rightHallways[1])
                                createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                            if (turnIndex == straightHallwayLength && currentHallway == rightHallways[1])
                                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                        }
                        else
                        {
                            if (turnIndex != straightHallwayLength)
                                createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                            else
                                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                        }
                    }
                    if (i == 0 && length != 0 && direction == "left")//To avoid overlapping of walls at the left hallway
                    {
                        if (turnIndex != 1 && nodeForm != nodeType.F)
                            createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                        else if (nodeForm == nodeType.F)
                        {
                            if (fNodeHasContigousSideHallways(leftHallways[0], leftHallways[1]))//If the F section has contigous sideHallways
                            {
                                if (currentHallway == leftHallways[1])
                                {
                                    createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 0.025f), wallRotation, height, 0.95f);
                                }
                                else
                                {
                                    createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                                }
                            }
                            else
                            {
                                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                            }
                        }
                        else
                            createWall(currentPos, wallRotation, height);
                        if (nodeForm == nodeType.F && fNodeHasContigousSideHallways(leftHallways[0], leftHallways[1]))
                        {
                            if (turnIndex != straightHallwayLength && currentHallway == leftHallways[1])
                                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                            if (turnIndex == straightHallwayLength && currentHallway == leftHallways[1])
                                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                        }
                        else
                        {
                            if (turnIndex != straightHallwayLength)
                                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                            else
                                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                        }
                    }
                    if (i != 0)//Basic case for wall creation
                    {
                        createWall(currentPos, wallRotation, height);
                        createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                    }

                    if (direction == "right")
                    {
                        currentPos.x++;
                    }
                    else if (direction == "left")
                    {
                        currentPos.x--;
                    }
                }
            }
        }
    }

    private void createFloor(Vector3 pos, string name)
    {
        GameObject floor = createSquare(pos);
        floor.name = name;
    }

    private void createWall(Vector3 pos, Vector3 rotation, int height, float shrinkRatio = 0.5f)
    {
        int aux = 0;
        for(int i = 0; i< height; i++)
        {
            GameObject wall = createSquare(new Vector3(pos.x, pos.y + aux, pos.z), shrinkRatio);
            wall.transform.Rotate(rotation);
            wall.name = "Wall";
            aux += 1;
        }
    }

    private Transform createPortal(Vector3 pos, Vector3 rotation, int portalType)
    {
        GameObject portalRef = GameObject.Instantiate((GameObject)portal);
        portalRef.GetComponent<PortalRender>().parentObject = portalRef;
        portalRef.transform.parent = parent.transform;
        portalRef.transform.position += pos;
        portalRef.transform.Rotate(rotation);
        if (portalType == 0)
            portalRef.name = "Entry Portal";
        else
            portalRef.name = "Leave Portal";

        if (portalType == 0)
        {
            entryPortal = portalRef.transform;
        }
        else
        {
            leavePortals.Add(portalRef.transform);
        }
        return portalRef.transform;
    }

    private GameObject createSquare(Vector3 pos, float shrinkRatio = 0.5f)//Create vertices and triangles arrays to make a 1^3 meter cube and instantiate it at the position given
    {
        //Create the vertices and triangles arrays to create a square at (0,0,0) below the player
        Vector3[] vertices = new Vector3[] {
            new Vector3(0,- 1 + shrinkRatio,0),
            new Vector3(1 ,- 1 + shrinkRatio,0),
            new Vector3(1,- 1 + 1,0),
            new Vector3(0,- 1 + 1,0),
            new Vector3(0,- 1 + 1,1),
            new Vector3(1,- 1 + 1,1),
            new Vector3(1,- 1 + shrinkRatio, 1),
            new Vector3(0,- 1 + shrinkRatio,1),
            };

        int[] triangles = {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };
        GameObject square = createMeshObject();
        //Move the floor to the correct position
        square.transform.position += pos;
        createMesh(square, vertices, triangles);

        return square;
    }

    private GameObject createMeshObject() //Creates the object where the floor is attached, and attaches it to the node gameobject (Its parent)
    {
        GameObject gameobject = new GameObject();
        gameobject.transform.parent = parent.transform;
        gameobject.AddComponent<MeshFilter>();
        gameobject.AddComponent<MeshRenderer>();
        return gameobject;
    }

    private void createMesh(GameObject instance, Vector3[] vertices, int[] triangles) //Gives values to the mesh of the gameobject
    {
        Mesh mesh = instance.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //Apply a default material
        instance.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

        //Set the color to the material
        var renderer = instance.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", color);

        //Recalculate normals for more realistic light effects
        mesh.RecalculateNormals();
    }

    private void createParentGameObject()
    {
        parent = new GameObject("Node");
        parent.transform.position = currentWorldCoordinates;
    }

    private nodeType setNodeType()
    {
        if (leftHallways.Count == 1 && rightHallways.Count == 1)
            return nodeType.T;
        else if (leftHallways.Count == 2 || rightHallways.Count == 2)
            return nodeType.F;
        else
            return nodeType.L;
    }

    private bool isTurning(int i, List<SideHallway> hallways)
    {
        bool resul = false;

        if (!IsEmpty(hallways))
        {
            for (int j = 0; j < hallways.Count; j++)
            {
                if (i == hallways[j].getTurnIndex())
                    resul = true;
            }
        }
        return resul;
    }

    public nodeType getNodeType()
    {
        return nodeForm;
    }

    public nodePosition getNodePosition()
    {
        return nodePos;
    }

    public void setNodeAsUnique()
    {
        uniqueNode = true;
    }
    private bool IsEmpty<T>(List<T> list)
    {
        if (list == null)
        {
            return true;
        }

        return !list.Any();
    }

    public bool hasRightHallways()
    {
        if (!IsEmpty(rightHallways))
        {
            return true;
        }
        return false;
    }

    public bool hasLeftHallways()
    {
        if (!IsEmpty(leftHallways))
        {
            return true;
        }
        return false;
    }

    public List<SideHallway> getRightHallways()
    {
        return rightHallways;
    }

    public List<SideHallway> getLeftHallways()
    {
        return leftHallways;
    }

    public Transform getMainHallwayPortal()//Gets the reference to the main portal of the section
    {
        if(nodeForm == nodeType.L)
        {
            if (!IsEmpty(rightHallways))
                return rightHallways[0].getLeavePortal();
            else
                return leftHallways[0].getLeavePortal();
        }
        else if(nodeForm == nodeType.F)
        {
            if (!IsEmpty(rightHallways))
            {
                if (rightHallways[0].isMain())
                    return rightHallways[0].getLeavePortal();
                else
                    return rightHallways[1].getLeavePortal();
            }
            else
            {
                if (leftHallways[0].isMain())
                    return leftHallways[0].getLeavePortal();
                else
                    return leftHallways[1].getLeavePortal();
            }
        }
        else
        {
            if (rightHallways[0].isMain())
                return rightHallways[0].getLeavePortal();
            else
                return leftHallways[0].getLeavePortal();
        }
    }

    public Transform getNotMainHallwayPortal()//Gets the reference to the non main portal of the section
    {
        if (nodeForm == nodeType.F)
        {
            if (!IsEmpty(rightHallways))
            {
                if (rightHallways[0].isMain())
                    return rightHallways[1].getLeavePortal();
                else
                    return rightHallways[0].getLeavePortal();
            }
            else
            {
                if (leftHallways[0].isMain())
                    return leftHallways[1].getLeavePortal();
                else
                    return leftHallways[0].getLeavePortal();
            }
        }
        else
        {
            if (rightHallways[0].isMain())
                return leftHallways[0].getLeavePortal();
            else
                return rightHallways[0].getLeavePortal();
        }
    }

    public Transform getEntryPortal()
    {
        return entryPortal;
    }

    public bool isRendered()
    {
        if (parent == null)
            return false;
        return true;
    }

    public int getStraightHallwayLength()
    {
        return straightHallwayLength;
    }
    public void setIndicator()
    {
        hasIndicator = true;
    }

    private bool fNodeHasContigousSideHallways(SideHallway hallway0, SideHallway hallway1)//True if this is an F node and the sidehallways are contiguous.
    {
        int biggestIndex;
        int lowestIndex;

        if (hallway1.getTurnIndex() > hallway0.getTurnIndex())
        {
            biggestIndex = hallway1.getTurnIndex();
            lowestIndex = hallway0.getTurnIndex();
        }
        else
        {
            biggestIndex = hallway0.getTurnIndex();
            lowestIndex = hallway1.getTurnIndex();
        }

        return biggestIndex - lowestIndex == 1;
    }
}