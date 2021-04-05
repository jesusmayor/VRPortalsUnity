using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEditor;
using UnityEngine;

public class GraphNode
{
    protected Object portal;//Reference to the prefab
    protected List<GraphNode> connectedNodes;//List of the nodes connected to this node
    public Transform entryPortal;//Portal used to enter the node
    public Transform leavePortal;//Portal used to leave the node
    protected GameObject parent;//Empty GameObject to store the node

    protected Vector3 currentWorldCoordinates;//Store here the global position where the node should be instantiated (Basically its (0,0,0) coordinates)
    protected int rightTurnIndex;//Right hallway start index
    protected int rightHallwayLength;//Right hallway length
    protected int leftTurnIndex;//Left hallway start index
    protected int leftHallwayLength;//Left hallway length
    protected int straightHallwayLength;//Straight hallway length
    protected int height;//Height of the node
    protected Color color;//Color of the node(generated randomly)


    public GraphNode()
    {
        connectedNodes = new List<GraphNode>();
    }

    public GraphNode(Vector3 startingPoint, int hallwayLength, int rightTurnIndex, int rightHallwayLength, int leftTurnIndex, int leftHallwayLength) : this()
    {
        this.currentWorldCoordinates = startingPoint;
        this.straightHallwayLength = hallwayLength;
        this.rightTurnIndex = rightTurnIndex;
        this.rightHallwayLength = rightHallwayLength;
        this.leftTurnIndex = leftTurnIndex;
        this.leftHallwayLength = leftHallwayLength;
        this.height = Random.Range(2, 5);
        this.color = new Color(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
        portal = Resources.Load("Portal");
        createParentGameObject();
    }

    public void connectTo(GraphNode node)//Used to connect this node "leavePortal" with another node "entryPortal" 
    {
        leavePortal.GetComponent<PortalRender>().connectedPortal = node.entryPortal;
        node.entryPortal.GetComponent<PortalRender>().connectedPortal = leavePortal;
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
        }

        createstraightHallway(currentWorldCoordinates, straightHallwayLength);
        turn("right", rightHallwayLength);
        turn("left", leftHallwayLength);
    }
    public void unrender()//Unrender this node
    {
        Object.Destroy(this.parent);
        parent = null;
    }
    private void createstraightHallway(Vector3 pos, int length)//Create the straight hallway of the node
    {
        Vector3 currentPos = pos;//Track current position
        Vector3 wallRotation = new Vector3(0, 0, 90);//Indicates rotation for walls for the straight hallway

        for (int i = 0; i < length; i++)
        {
            //Create floor instance
            createFloor(currentPos, "Floor");//Create the floor
            createFloor(new Vector3(currentPos.x, currentPos.y + height + 0.5f, currentPos.z), "Ceiling");//Create the ceiling

            if (i == 0)//If this is the first floor, instantiate the walls at start, always 1 less than the height of the node. Also create the entry portal
            {
                createWall(new Vector3(currentPos.x, currentPos.y + 1, currentPos.z), new Vector3(0, 90, 90), height - 1);
                entryPortal = createPortal(new Vector3(currentPos.x + 0.5f, currentPos.y + 0.5f, currentPos.z), Vector3.zero, "Entry Portal");
            }

            if(i + 1 != this.leftTurnIndex)//If node doesn´t turn left at this position, create a wall in the left
                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y,currentPos.z), wallRotation, height);

            if (i + 1 != this.rightTurnIndex)//If node doesn´t turn right at this position, create a wall in the right
                createWall(new Vector3(currentPos.x + 1, currentPos.y, currentPos.z), wallRotation, height);

            if (i == length - 1)//If this is the last floor, create a wall in the end
                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), new Vector3(0, 90, 90), height);

            currentPos.z++;//Move forward 1 time
        }
    }

    private void turn(string direction, int length)//Calculate where the turned hallways are, their lengths and instantiates them
    {
        //Current position to instantiate several floors
        Vector3 currentPos = currentWorldCoordinates;
        //Base rotations needed
        Vector3 wallRotation = new Vector3(0, 90, 90);
        Vector3 leftPortalRotation = new Vector3(0,90,0);
        Vector3 rightPortalRotation = new Vector3(0, -90, 0);

        if (direction == "right")//If direction is right, set up coordinates to instantiate the right hallway
        {
            currentPos.x++;
            currentPos.z = rightTurnIndex - 1;
        }
        else if (direction == "left")//If direction is left, set up coordinates to instantiate the left hallway
        {
            currentPos.x--;
            currentPos.z = leftTurnIndex - 1;
        }

        for (int i = 0; i < length; i++)//Iterate over the number of floors and instantiate them
        {
            //Create floor instance
            createFloor(currentPos, "Floor");//Create the floor
            createFloor(new Vector3(currentPos.x, currentPos.y + height + 0.5f, currentPos.z), "Ceiling");//Create the ceiling

            if (i == length - 1)//If its the last node of the side hallway, create the end walls and the exit portals
            {
                if(direction == "left")
                {
                    createWall(new Vector3(currentPos.x - 0.5f, currentPos.y + 1, currentPos.z), new Vector3(0, 0, 90), height - 1);
                    leavePortal = createPortal(new Vector3(currentPos.x, currentPos.y + 0.5f, currentPos.z + 0.5f), leftPortalRotation, "Leave Portal");
                }
                else
                {
                    createWall(new Vector3(currentPos.x + 1, currentPos.y + 1, currentPos.z), new Vector3(0, 0, 90), height - 1);
                    leavePortal = createPortal(new Vector3(currentPos.x + 1, currentPos.y + 0.5f, currentPos.z + 0.5f), rightPortalRotation, "Leave Portal");
                }

            }
            //Create walls across the side hallway distantiated by 1 meter
            if(i == 0 && rightHallwayLength != 0 && direction == "right")//To avoid overlapping of walls at the right hallway
            {
                if(rightTurnIndex != 1)
                    createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                else
                    createWall(currentPos, wallRotation, height);
                if (rightTurnIndex != straightHallwayLength)
                    createWall(new Vector3(currentPos.x + 0.5f, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                else
                    createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
            }
            if (i == 0 && leftHallwayLength != 0 && direction == "left")//To avoid overlapping of walls at the left hallway
            {
                if(leftTurnIndex != 1)
                    createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z), wallRotation, height);
                else
                    createWall(currentPos, wallRotation, height);
                if (leftTurnIndex != straightHallwayLength)
                    createWall(new Vector3(currentPos.x - 0.5f, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
                else
                    createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
            }
            if(i != 0)//Basic case for wall creation
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

    private void createFloor(Vector3 pos, string name)
    {
        GameObject floor = createSquare(pos,0.5f);
        floor.name = name;
    }

    private void createWall(Vector3 pos, Vector3 rotation, int height)
    {
        int aux = 0;
        for(int i = 0; i< height; i++)
        {
            GameObject wall = createSquare(new Vector3(pos.x, pos.y + aux, pos.z), 0.5f);
            wall.transform.Rotate(rotation);
            wall.name = "Wall";
            aux += 1;
        }
    }

    private Transform createPortal(Vector3 pos, Vector3 rotation, string name)
    {
        GameObject portalRef = GameObject.Instantiate((GameObject)portal);
        portalRef.transform.parent = parent.transform;
        portalRef.transform.position += pos;
        portalRef.transform.Rotate(rotation);
        portalRef.name = name;

        if (name == "Entry portal")
            entryPortal = portalRef.transform;
        else
            leavePortal = portalRef.transform;

        return portalRef.transform;
    }

    private GameObject createSquare(Vector3 pos, float shrinkRatio)//Create vertices and triangles arrays to make a 1^3 meter cube and instantiate it at the position given
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

}