using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public enum SquareType {
        ROAD,
        BUILDING,
        POLICE_BUILDING,
        BANK,
        ABILITY_ITEM
    }

    public GridCoord coord;
    public SquareType squareType;

    public Material squareMaterial;

    public Car occupiedCar;

    // Start is called before the first frame update

    public void SetSquareColor(Color color) {
        squareMaterial.color = color;
    }

    public void ResetSquareColor() {
        squareMaterial.color = GameController.gc.baseSquareColor;
    }

    public Vector3 GetCarPos() {
        return transform.position + (Vector3.up * 0.14f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
