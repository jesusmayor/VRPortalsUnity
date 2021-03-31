using System.Collections;
using System.Collections.Generic;
using TFG;
using UnityEditor;
using UnityEngine;

public class GraphNode
{
    protected List<GraphNode> connectedNodes;
    [SerializeField]
    public Transform entryPortal;
    [SerializeField]
    public Transform leavePortal;

    protected GameObject parent;
    //The world coordinates could probably be just the X axis value
    protected Vector3 currentWorldCoordinates;//Store here the global position where the node should be instantiated (Basically its (0,0,0) coordinates)
    protected int rightTurnIndex;//Store where the right hallway starts
    protected int rightHallwayLength;//Store the right hallway length
    protected int leftTurnIndex;//Store where the left hallway starts
    protected int leftHallwayLength;//Store the left hallway length
    protected int straightHallwayLength;//Store the straight hallway length
    protected int height;
    protected Color color;//Do I generate it randomly or do I add it to the constructor?


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
        this.height = Random.Range(1, 4);
        this.color = new Color(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 1), Random.Range(0, 2));
        createParentGameObject();
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
    private void createstraightHallway(Vector3 pos, int length)
    {
        Vector3 currentPos = pos;//Track current position
        Vector3 wallRotation = new Vector3(0, 0, 90);//Indicates rotation for walls for the straight hallway

        for (int i = 0; i < length; i++)
        {
            //Create floor instance
            createFloor(currentPos, "Floor");//Create the floor
            createFloor(new Vector3(currentPos.x, currentPos.y + height + 0.5f, currentPos.z), "Ceiling");//Create the ceiling

            if(i == 0)//If this is the first floor, instantiate the walls at start, always 1 less than the height of the node
                createWall(new Vector3(currentPos.x,currentPos.y + 1, currentPos.z), new Vector3(0, 90, 90), height - 1);

            if(i + 1 != this.leftTurnIndex)//If node doesn´t turn left at this position, create a wall in the left
                createWall(new Vector3(currentPos.x - 0.5f, currentPos.y,currentPos.z), wallRotation, height);

            if (i + 1 != this.rightTurnIndex)//If node doesn´t turn right at this position, create a wall in the left
                createWall(new Vector3(currentPos.x + 1, currentPos.y, currentPos.z), wallRotation, height);

            if (i == length - 1)//If this is the last floor, create a wall in the end
                createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), new Vector3(0, 90, 90), height);

            currentPos.z++;//Move forward 1 time
        }
    }//Create the straight hallway of the node

    private void turn(string direction, int length)//Calculate where the turned hallways are, their lengths and instantiates them
    {
        //Current position to instantiate several floors
        Vector3 currentPos = currentWorldCoordinates;
        //Rotation of the side hallway walls
        Vector3 wallRotation = new Vector3(0, 90, 90);

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

            if(i == length - 1)//If its the last node of the side hallway, create the end walls 
            {
                if(direction == "left")
                    createWall(new Vector3(currentPos.x, currentPos.y + 1, currentPos.z), new Vector3(0,0,90), height - 1);
                else
                    createWall(new Vector3(currentPos.x + 1, currentPos.y + 1, currentPos.z), new Vector3(0, 0, 90), height - 1);
            }
            //Create walls across the side hallway distantiated by 1 meter
            createWall(currentPos, wallRotation, height);
            createWall(new Vector3(currentPos.x, currentPos.y, currentPos.z + 1.5f), wallRotation, height);
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
        GameObject floor = new GameObject();
        floor.transform.parent = parent.transform;
        floor.AddComponent<MeshFilter>();
        floor.AddComponent<MeshRenderer>();
        return floor;
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