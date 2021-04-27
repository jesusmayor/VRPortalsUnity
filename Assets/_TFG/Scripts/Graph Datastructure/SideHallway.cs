using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideHallway
{
    protected bool ismain;
    protected string side;
    protected int turnIndex;//Hallway start index
    protected int hallwayLength;//Hallway length


    public SideHallway(string side, int turnIndex, int hallwayLength)
    {
        this.side = side;
        this.turnIndex = turnIndex;
        this.hallwayLength = hallwayLength;
    }
    public int getTurnIndex()
    {
        return turnIndex;
    }

    public int getHallwayLength()
    {
        return hallwayLength;
    }

    public string getSideValue()
    {
        return side;
    }

    public bool isMain()
    {
        return ismain;
    }

    public void setAsMain()
    {
        ismain = true;
    }
}
