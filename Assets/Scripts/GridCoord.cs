using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridCoord
{
    public int x;
    public int y;

    public GridCoord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static float Distance(GridCoord a, GridCoord b) {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x,2)+Mathf.Pow(a.y - b.y,2));
    }

    public static bool IsAdjacent(GridCoord a, GridCoord b) {
        return Distance(a,b) == 1;
    }

    public static bool operator ==(GridCoord a, GridCoord b) {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(GridCoord a, GridCoord b) {
        return a.x != b.x || a.y != b.y;
    }

    public override bool Equals(object obj) {
        if(!(obj is GridCoord)) 
            return false;
        GridCoord gc = (GridCoord) obj;
        return this.x == gc.x && this.y == gc.y;
    }
}
