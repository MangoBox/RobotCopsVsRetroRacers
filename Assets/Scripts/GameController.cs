using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool playersTurn = true;
    public bool moveTurn = false;
    public int remainingMoves;

    public GridGenerator gridGenerator;
    public CameraController cameraController;

    public Powerup[] powerups;
    public GameObject powerupUIPrefab;

    [Header("Cars")]
    public Car playerCar;
    public GameObject copCarPrefab;
    private List<Car> copCars;
    public int copsNumber;

    public string[] copNames;


    [HideInInspector]
    public Square[,] grid;

    //Controls animating cars across the board
    public List<AnimatingCar> animatingCars = new List<AnimatingCar>();

    public static GameController gc;

    public UIController uiController;

    [Header("Aesthetics")]
    public Color highlightedSquareColor;
    public Color currentSquareColor;
    public Color baseSquareColor;
    public Color dangerColor;

    public class AnimatingCar {
        public Car car;
        public float time;
        public Vector3 from;
        public Vector3 to;
        public float seconds;
        public Quaternion startRot;
        public Quaternion endRot;

        //Returns true if perc is >= 1.
        public bool FrameUpdate(float deltaTime) {
            time += deltaTime;
            float perc = time / seconds;
            //A converted value that uses a smooth cosine wave for the animation.
            float perc_smooth = 0.5f - 0.5f * Mathf.Cos(perc * Mathf.PI);
            //Square offset
            Vector3 offset = Vector3.up * 0.14f;
            Vector3 pos = Vector3.Lerp(from,to + offset,perc_smooth);

            Quaternion rotLerp = Quaternion.Lerp(startRot,endRot, perc);

            
            //car.transform.rotation = Quaternion.Euler(-rotLerp.eulerAngles);
            car.transform.rotation = endRot;
            car.transform.rotation = Quaternion.Euler(new Vector3(0,180+car.transform.rotation.eulerAngles.y,0));

            car.transform.position = pos;
            return perc >= 1;
        }

        public AnimatingCar(Car car, Vector3 from, Vector3 to, float seconds) {
            this.car = car;
            this.time = 0;
            this.from = from;
            this.to = to;
            this.seconds = seconds;

            //original rotation
            this.startRot = car.transform.rotation;
            Transform t = new GameObject().transform;
            t.position = car.transform.position;

            t.transform.LookAt(to,Vector3.up);
            this.endRot = t.transform.rotation;
            Destroy(t.gameObject);
        }
    }

    public enum GameState {
        TURN,
        WAITING,
        MAIN_MENU
    }

    // Start is called before the first frame update
    void Start()
    {
        gc = this;
        grid = gridGenerator.GenerateGrid();
        GenerateCops();
        PlayerTurn();
        MoveToSquare(playerCar,gridGenerator.GetSquare(new GridCoord(0,0)));
        
    }

    public List<Car> GetAllCars() {
        List<Car> allCars = new List<Car>();
        allCars.Add(playerCar);
        allCars.AddRange(copCars);
        return allCars;
    }

    public string GetRandomName() {
        return copNames[Random.Range(0,copNames.Length)];
    }

    void GenerateCops() {
        copCars = new List<Car>();
        for(int i = 0; i < copsNumber; i++) {
            SpawnCop();
        }
    }

    Car SpawnCop() {
        GameObject spawnedCop = Instantiate(copCarPrefab);
        Car copCar = spawnedCop.GetComponent<Car>();
        copCar.carName = GetRandomName();
        bool found = false;
        do {
            GridCoord gridCoord = new GridCoord(Random.Range(0,gridGenerator.dimensionsX),Random.Range(0,gridGenerator.dimensionsY));
            if(gridGenerator.GetSquare(gridCoord).squareType == Square.SquareType.ROAD && gridCoord.x != 0 && gridCoord.y != 0) {
                MoveToSquare(copCar, gridGenerator.GetSquare(gridCoord));
                found = true;
                continue;
            }
        } while (!found);
        copCars.Add(copCar);
        return copCar;
    }


    public IEnumerator CopTurn() {
        uiController.UpdateTurnUI(false);
        for(int i = 0; i < copCars.Count; i++) {
            
            Car copCar = copCars[i];
            HighlightAvailableSquares(copCar);
            cameraController.currentTargetCar = copCar;
            uiController.UpdateBanner("Cop " + copCar.carName + "'s turn");
            GridCoord currentClone = copCar.gridCoord;
            int diceRoll = GetDiceRoll();
            uiController.UpdateMoveCount(diceRoll);
            yield return new WaitForSeconds(0.8f);
            for(int m = diceRoll; m > 0; m--) {
                /*OLD AI
                int remainingTries = 10;
                bool found = false;
                do {
                    currentClone = copCar.gridCoord;
                    int dir = Random.Range(0,4);
                    switch(dir) {
                        case 0:
                        currentClone.x++;
                        break;
                        case 1:
                        currentClone.x--;
                        break;
                        case 2:
                        currentClone.y++;
                        break;
                        case 3:
                        currentClone.y--;
                        break;
                    }
                    if(currentClone.x < 0 || currentClone.y < 0 || currentClone.x >= gridGenerator.dimensionsX || currentClone.y >= gridGenerator.dimensionsY)
                        continue;
                    if(gridGenerator.GetSquare(currentClone).squareType == Square.SquareType.BUILDING)
                        continue;
                    found = true;
                }while(--remainingTries > 0 && !found);
                */
                int remainingTries = 10;
                Dictionary<float, GridCoord> distanceDict = new Dictionary<float, GridCoord>();
                do {
                    currentClone = copCar.gridCoord;
                    int dir = Random.Range(0,4);
                    switch(dir) {
                        case 0:
                        currentClone.x++;
                        break;
                        case 1:
                        currentClone.x--;
                        break;
                        case 2:
                        currentClone.y++;
                        break;
                        case 3:
                        currentClone.y--;
                        break;
                    }
                    float distance = GridCoord.Distance(currentClone, playerCar.gridCoord);
                    
                    if(currentClone.x < 0 || currentClone.y < 0 || currentClone.x >= gridGenerator.dimensionsX || currentClone.y >= gridGenerator.dimensionsY)
                        continue;
                    if(gridGenerator.GetSquare(currentClone).squareType == Square.SquareType.BUILDING)
                        continue;
                    distanceDict[distance] = currentClone;
                    
                }while(--remainingTries > 0);
                var result = distanceDict.OrderBy(x => x.Key);
                GridCoord lowestDist = result.FirstOrDefault().Value;
                if(lowestDist!=null) {
                    MoveToSquare(copCar, gridGenerator.GetSquare(lowestDist));
                }
                uiController.UpdateMoveCount(m);
                yield return new WaitForSeconds(0.25f);
            }
            
        }
        PlayerTurn();
    }

    public int GetDiceRoll() {
        return Random.Range(1,7);
    }

    public void PlayerTurn() {
        if(CheckIfBusted()) {
            SceneManager.LoadScene("MainMenu");
        }
        HighlightAvailableSquares(playerCar);
        playersTurn = true;
        remainingMoves = GetDiceRoll();
        uiController.UpdateMoveCount(remainingMoves);
        cameraController.currentTargetCar = playerCar;
        uiController.UpdateTurnUI(true);
    }

    public bool CheckIfBusted() {
        List<Square> adjacentSquares = GetAdjacentSquares(gridGenerator.GetSquare(playerCar.gridCoord));
        bool ableToMove = false;
        foreach(Square s in adjacentSquares) {
            if(CanMoveTo(playerCar, s))
                ableToMove = true;
        }
        return !ableToMove;
    }
    
    public void ClickOnSquare(Square clicked) {
        if(playersTurn == false)
            return;
        //Initial check
        if(remainingMoves <= 0)
            return;
        int result = MoveToSquare(playerCar, clicked);
        if(result == 1) {
            remainingMoves--;
            uiController.UpdateMoveCount(remainingMoves);
        }
        if(remainingMoves <= 0 && result != 2) {
            playersTurn = false;
            StartCoroutine(CopTurn());
        }
    }

    public IEnumerator HandlePowerup() {
        Powerup pu = powerups[Random.Range(0,powerups.Length)];
        GameObject prefabInst = Instantiate(powerupUIPrefab);
        prefabInst.transform.SetParent(uiController.canvas.transform,false);
        RectTransform rectTransform = prefabInst.GetComponent<RectTransform>();
        PowerupUI powerupUI = prefabInst.GetComponent<PowerupUI>();
        powerupUI.ApplyPowerup(pu);
        yield return new WaitForSeconds(4f);
        Destroy(prefabInst);
        
    }

    void Update() {
        //Iterate over car animations
        if(animatingCars.Count > 0) {
            List<AnimatingCar> toRemove = new List<AnimatingCar>();
            foreach (AnimatingCar aC in animatingCars) {
                bool finished = aC.FrameUpdate(Time.deltaTime);
                if(finished) {
                    toRemove.Add(aC);
                }
            }
            animatingCars.RemoveAll(x => toRemove.Contains(x));
        }
        
    }

    public int MoveToSquare(Car car, Square square) {
        if(!CanMoveTo(car, square,true))
            return 0;
        gridGenerator.GetSquare(car.gridCoord).occupiedCar = null;
        square.occupiedCar = car;
        car.gridCoord = square.coord;
        //Start car animating
        AnimatingCar aC = new AnimatingCar(car, car.transform.position, square.transform.position, 0.2f);
        animatingCars.Add(aC);

        //Spawning cop car when player goes past station.
        if(car == playerCar) {
            foreach(Square s in GetAdjacentSquares(gridGenerator.GetSquare(car.gridCoord))) {
                if(s.squareType == Square.SquareType.POLICE_BUILDING) {
                    Car newCop = SpawnCop();
                    StartCoroutine(NotifyNewCopCar(newCop));
                }
            }
            if(gridGenerator.GetSquare(playerCar.gridCoord).squareType == Square.SquareType.ABILITY_ITEM) {
                Square s = gridGenerator.GetSquare(playerCar.gridCoord);
                if(s.transform.GetChild(1).gameObject.activeSelf) {
                    StartCoroutine(HandlePowerup());
                    s.transform.GetChild(1).gameObject.SetActive(false);
                    StartCoroutine(WaitForCopCompletion());
                    return 2;
                }             
            }
        }
        HighlightAvailableSquares(car);
        return 1;
    }

    IEnumerator WaitForCopCompletion() {
        yield return new WaitForSeconds(4f);
        CopTurn();
    }
    
    public List<Square> GetAdjacentSquares(Square square) {
        List<Square> adajacentSquares = new List<Square>();
        for(int x = 0; x < gridGenerator.dimensionsX; x++) {
            for(int y = 0; y < gridGenerator.dimensionsY; y++) {
                if(square.coord.x < 0 || square.coord.y < 0 || square.coord.x >= gridGenerator.dimensionsX || square.coord.y >= gridGenerator.dimensionsY)
                        continue;
                if(GridCoord.IsAdjacent(square.coord, new GridCoord(x,y))) {
                    adajacentSquares.Add(gridGenerator.GetSquare(new GridCoord(x,y)));
                }
            }
        }
        return adajacentSquares;
    }


    public void HighlightAvailableSquares(Car car) {
        for(int x = 0; x < gridGenerator.dimensionsX; x++) {
            for(int y = 0; y < gridGenerator.dimensionsY; y++) {
                Square s = grid[x,y];
                s.ResetSquareColor();
                if(car == playerCar) {
                    if(s.occupiedCar != null && s.occupiedCar != playerCar && GridCoord.IsAdjacent(playerCar.gridCoord,s.occupiedCar.gridCoord))
                        s.SetSquareColor(dangerColor);
                } else {
                    if(s.occupiedCar != null && s.occupiedCar == playerCar && GridCoord.IsAdjacent(car.gridCoord,s.occupiedCar.gridCoord))
                        s.SetSquareColor(dangerColor);
                }
                if(CanMoveTo(car, s)) {
                    s.SetSquareColor(highlightedSquareColor);
                } else if(s = gridGenerator.GetSquare(car.gridCoord)) {
                    s.SetSquareColor(car.centreHighlightColor);
                }
            }   
        }
    }

    public IEnumerator NotifyNewCopCar(Car target) {
        uiController.UpdateBanner("New Cop Car Deployed", "You passed by a police station!", new Color(255,0,0));
        cameraController.currentTargetCar = target;
        yield return new WaitForSeconds(3f);
        cameraController.currentTargetCar = playerCar;
        uiController.UpdateTurnUI(true);
        yield return new WaitForSeconds(0.3f);
    }



    public bool CanMoveTo(Car car, Square square, bool ignoreDistance = false) {
        if(square.squareType != Square.SquareType.ROAD && square.squareType != Square.SquareType.ABILITY_ITEM)
            return false;

        if(square.occupiedCar != null)
            return false;

        if(!GridCoord.IsAdjacent(grid[car.gridCoord.x,car.gridCoord.y].coord, square.coord) && !ignoreDistance)
            return false;

        return true;
    }
}
