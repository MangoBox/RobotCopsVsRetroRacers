using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public GameController gameController;
    public Car thisCar;

    // Start is called before the first frame update
    void Start()
    {
        thisCar = GetComponent<Car>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit = new RaycastHit();
        if(Physics.Raycast(mouseRay, out rayHit, 1000)) {
            GameObject hit = rayHit.collider.gameObject;
                    if(!Input.GetMouseButtonDown(0))
                        return;
                    if(hit.GetComponentInParent<Square>() == null)
                        return;
                    Square s = hit.GetComponentInParent<Square>();
                    if(!GridCoord.IsAdjacent(thisCar.gridCoord,s.coord))
                        return;
                    if(s.squareType==Square.SquareType.BUILDING)
                        return;
                    gameController.ClickOnSquare(s);
        }
           
    }
}
