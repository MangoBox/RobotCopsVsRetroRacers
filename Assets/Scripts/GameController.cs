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

    [HideInInspector]
    public int remaining3xMove = 0;

    [HideInInspector]
    public int remainingEscapeCards = 0;

    public string[] copNames;


    [HideInInspector]
    public Square[,] grid;

    //Controls animating cars across the board
    public List<AnimatingCar> animatingCars = new List<AnimatingCar>();

    public static GameController gc;

    public UIController uiController;

    public int score = 0;

    public int scoreMultiplier = 1;

    [Header("Aesthetics")]
    public Color highlightedSquareColor;
    public Color currentSquareColor;
    public Color baseSquareColor;
    public Color dangerColor;

    public Color bankColor;

    public List<Square> ActivatedBanks;

    [Header("Score Parameters")]
    public int scoreMoveAmount;
    public int newCityAmount;

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
        ActivatedBanks.Clear();

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

    public void RobBank(Square s) {
        if(s.squareType != Square.SquareType.BANK || ActivatedBanks.Contains(s))
            return;
        Animator bankAnimator = s.GetComponentInChildren<Animator>();
        bankAnimator.SetTrigger("TriggerAlarm");
        AddScoreMultiplier(1);
        AddScore(10000);
        ActivatedBanks.Add(s);
        
    }


    public IEnumerator CopTurn() {
        //Notifies the UI Controller to update relevant banner information
        uiController.UpdateTurnUI(false);
        //Iterates over the cop cars in the level
        for(int i = 0; i < copCars.Count; i++) {
            //Select the relevant cop car
            Car copCar = copCars[i];
            //Highlight the squares that the cop can move
            HighlightAvailableSquares(copCar);
            //Set the current camera target
            cameraController.currentTargetCar = copCar;
            //Update banner information for this specific cop
            uiController.UpdateBanner("Cop " + copCar.carName + "'s turn");
            //Declare a coordinate (clone) of the cops current coordinates
            GridCoord currentClone = copCar.gridCoord;
            //Get a dice roll
            int diceRoll = GetDiceRoll();
            //Update the move count on the banner information
            uiController.UpdateMoveCount(diceRoll);
            //Wait for the camera to move to the cop cars position
            yield return new WaitForSeconds(0.8f);
            //For every move on this dice roll
            for(int m = diceRoll; m > 0; m--) {
                //Try 10 times to move
                int remainingTries = 10;
                //Declare a dictionary that will help us find the coordinate that reduces distance
                //to the player the most.
                Dictionary<float, GridCoord> distanceDict = new Dictionary<float, GridCoord>();
                //(All below) Randomly move the coordinate by up to a unit in any direction
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
                    //Get the distance to the player if the cop car were to move here
                    float distance = GridCoord.Distance(currentClone, playerCar.gridCoord);
                    //Check invalid grid positions
                    if(currentClone.x < 0 || currentClone.y < 0 || currentClone.x >= gridGenerator.dimensionsX || currentClone.y >= gridGenerator.dimensionsY)
                        continue;
                    //Check if the grid is occupied by a building
                    if(gridGenerator.GetSquare(currentClone).squareType == Square.SquareType.BUILDING)
                        continue;
                    //Insert the distance and coordinate into the dictionary
                    distanceDict[distance] = currentClone;
                    
                }while(--remainingTries > 0);
                //Find the smallest result
                var result = distanceDict.OrderBy(x => x.Key);
                GridCoord lowestDist = result.FirstOrDefault().Value;
                //Check the dictionary result isn't empty
                if(lowestDist!=null) {
                    //Finally! Move to the square
                    MoveToSquare(copCar, gridGenerator.GetSquare(lowestDist));
                }
                //Update the move banner count
                uiController.UpdateMoveCount(m);
                //Wait for a quarter of a second to let the cop car to move to the position
                yield return new WaitForSeconds(0.25f);
            }
            
        }
        //Change to the players turn
        PlayerTurn();
    }

    public int GetDiceRoll() {
        return Random.Range(1,7);
    }

    public void ApplyPowerup(Powerup powerup) {
        
    }

    public void PlayerTurn() {
        if(CheckIfBusted()) {
            if(remainingEscapeCards > 0) {
                //Randomly teleport player somewhere!
                remainingEscapeCards--;
                bool found = false;
                do {
                    GridCoord gridCoord = new GridCoord(Random.Range(0,2)*(gridGenerator.dimensionsX-1),Random.Range(0,2)*(gridGenerator.dimensionsY-1));
                    if(gridGenerator.GetSquare(gridCoord).squareType == Square.SquareType.ROAD && CanMoveTo(playerCar, gridGenerator.GetSquare(gridCoord), true)) {
                        MoveToSquare(playerCar, gridGenerator.GetSquare(gridCoord));
                        found = true;
                        continue;
                    }
                } while (!found);

            } else {
                uiController.DisplayGameOverScreen();
            }
        }
        HighlightAvailableSquares(playerCar);
        playersTurn = true;
        remainingMoves = GetDiceRoll();
        if(remaining3xMove > 0) {
            remainingMoves *= 3;
            remaining3xMove--;
        }
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
        pu.Activate(this);
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

    public void AddScore(int amount) {
        SetScore(score + (amount * scoreMultiplier));
    }

    public void SetScore(int amount) {
        score = amount;
        uiController.SetScoreText(score);
    }

    public void AddScoreMultiplier(int amount) {
        SetScoreMultiplier(scoreMultiplier + amount);
    }

    public void SetScoreMultiplier(int amount) {
        scoreMultiplier = amount;
        uiController.SetScoreMultiplier(scoreMultiplier);
    }
    

    public int MoveToSquare(Car car, Square square) {
        //Checks if the car can move to the position
        if(!CanMoveTo(car, square,true))
            return 0;
        //Notifies the formerly-occupied square of it's vacancy
        gridGenerator.GetSquare(car.gridCoord).occupiedCar = null;
        //Notifies the new square of the new car
        square.occupiedCar = car;
        car.gridCoord = square.coord;
        //Start car animating
        AnimatingCar aC = new AnimatingCar(car, car.transform.position, square.transform.position, 0.2f);
        animatingCars.Add(aC);

        //Spawning cop car when player goes past station.
        if(car == playerCar) {
            //Iterate over the adjacent squares
            foreach(Square s in GetAdjacentSquares(gridGenerator.GetSquare(car.gridCoord))) {
                //If its a police building, spawn a new police car
                if(s.squareType == Square.SquareType.POLICE_BUILDING) {
                    Car newCop = SpawnCop();
                    //Pan camera over and let player know
                    StartCoroutine(NotifyNewCopCar(newCop));
                }
            }
            //Add Score
            AddScore(scoreMoveAmount);
            //Check if it's an ability item
            if(gridGenerator.GetSquare(playerCar.gridCoord).squareType == Square.SquareType.ABILITY_ITEM) {
                //Get a reference to the powerup square
                Square s = gridGenerator.GetSquare(playerCar.gridCoord);
                //If the powerup is not used (Active)
                if(s.transform.GetChild(1).gameObject.activeSelf) {
                    //Start the handlepowerup Coroutine
                    StartCoroutine(HandlePowerup());
                    //Mark the powerup as used (Inactive)
                    s.transform.GetChild(1).gameObject.SetActive(false);
                    //Wait for the powerup animation to be complete before starting on the cop's turn
                    StartCoroutine(WaitForCopCompletion());
                    //Notify the caller that the class had a powerup status
                    return 2;
                }             
            }
        }
        //Highlight new squares for the player.
        HighlightAvailableSquares(car);
        //Movement was successful.
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
                    
                } else if(s == gridGenerator.GetSquare(car.gridCoord)) {
                    s.SetSquareColor(car.centreHighlightColor);
                }
                if(s.squareType == Square.SquareType.BANK && GridCoord.IsAdjacent(car.gridCoord,new GridCoord(x,y)) && car == playerCar && !ActivatedBanks.Contains(s)) {
                    s.SetSquareColor(bankColor);
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
