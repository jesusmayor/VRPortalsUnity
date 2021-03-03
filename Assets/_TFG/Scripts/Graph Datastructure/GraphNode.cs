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
    protected Color color;//Do I generate it randomnly or do I add it to the constructor?


    public GraphNode()
    {
        connectedNodes = new List<GraphNode>();
    }

    public GraphNode(Vector3 startingPoint, int hallwayLength,int rightTurnIndex, int rightHallwayLength, int leftTurnIndex, int leftHallwayLength):this()
    {
        this.currentWorldCoordinates = startingPoint;
        this.straightHallwayLength = hallwayLength;
        this.rightTurnIndex = rightTurnIndex;
        this.rightHallwayLength = rightHallwayLength;
        this.leftTurnIndex = leftTurnIndex;
        this.leftHallwayLength = leftHallwayLength;
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
        if(this.parent.activeSelf == false)//If node was unrendered, render it, but dont build it again
        {
            this.parent.SetActive(true);
        }
        else //Build the node from zero
        {
            createstraightHallway(currentWorldCoordinates, straightHallwayLength);
            turn("right", rightHallwayLength);
            turn("left", leftHallwayLength);
        }
    }
    public void unrender()//Unrender this node, could probably destroy the gameObject
    {
        this.parent.SetActive(false);
    }
    private void createstraightHallway(Vector3 pos, int length)
    {
        //Track current position
        Vector3 currentPos = pos;

        for (int i = 0; i < length; i++)//Iterate over the number of floors and instantiate them
        {
            //Create floor instance
            createFloor(currentPos);
            //Move forward 1 time
            currentPos.z++;
        }
    }//Create the straight hallway of the node

    private void turn(string direction, int length)//Calculate where the turned hallways are, their lengths and instantiates them
    {
        //Current position to instantiate several floors
        Vector3 currentPos = currentWorldCoordinates;

        if (direction == "right")//If direction is right, set up coordinates to instantiate the right hallway
        {
            currentPos.x++;
            currentPos.z = rightTurnIndex -1;
        }
        else if (direction == "left")//If direction is left, set up coordinates to instantiate the left hallway
        {
            currentPos.x--;
            currentPos.z = leftTurnIndex -1;
        }

        for (int i = 0; i < length; i++)//Iterate over the number of floors and instantiate them
        {
            //Create floor instance
            createFloor(currentPos);
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

    private void createFloor(Vector3 pos)//Create vertices and triangles arrays to make a 1 by 1 square and instantiate it at the position given
    {
        Vector3[] vertices = new Vector3[] { new Vector3(pos.x, pos.y, pos.z), new Vector3(pos.x, pos.y, pos.z + 1), new Vector3(pos.x + 1, pos.y, pos.z), new Vector3(pos.x + 1, pos.y, pos.z + 1) };

        int[] triangles = new int[] { 0, 1, 2, 2, 1, 3 };
        GameObject floor = createMeshObject();
        createMesh(floor, vertices, triangles);
    }

    private GameObject createMeshObject() //Creates the object where the floor is attached, and attaches it to the node gameobject (Its parent)
    {
        GameObject floor = new GameObject("Floor");
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
