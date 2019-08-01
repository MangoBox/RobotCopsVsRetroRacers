using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GridPrefab[] gridPrefabs;
    public int dimensionsX;
    public int dimensionsY;

    public int minRowSkips;
    public int maxRowSkips;
    public float gridSpacing;


    public Square[,] GenerateGrid() {
        //Provide a small X & Y offset to compensate for the centre of a square.
        float offsetX = (dimensionsX * gridSpacing) / 2;
        float offsetY = (dimensionsY * gridSpacing) / 2;
        //Generate a set of random row skips to make sure the map is always navigable.
        List<int> xSkips = GenerateSkipRowIndexes(minRowSkips,maxRowSkips,dimensionsX,true);
        List<int> ySkips = GenerateSkipRowIndexes(minRowSkips,maxRowSkips,dimensionsY,true);

        //Initialise the grid, with requred dimensions.
        Square[,] grid = new Square[dimensionsX, dimensionsY];
        //Iterate over every single GridCoord.
        for(int x = 0; x < dimensionsX; x++) {
            for(int y = 0; y < dimensionsY; y++) {
                //Pick a random weighted grid prefab.
                GameObject chosenPrefab = PickRandomGridPrefab();
                //Check if the current grid square is meant to be a skip. If it is, it's a blank square.
                if(xSkips.Contains(x) || ySkips.Contains(y)) {
                    chosenPrefab = gridPrefabs[0].prefab;
                }
                //Spawn the grid block.
                GameObject spawnedGrid = Instantiate(chosenPrefab);
                //Move the grid block into the correct 3D position (in accordance with offsetting and spacing)
                spawnedGrid.transform.position = new Vector3(x * gridSpacing - offsetX, 0, y*gridSpacing - offsetY);
                //Assign the square object in the grid so we can use it later.
                Square squareObj = spawnedGrid.GetComponent<Square>();
                grid[x,y] = squareObj;
                squareObj.coord = new GridCoord(x,y);
                //Set the square material.
                squareObj.squareMaterial = squareObj.GetComponentInChildren<MeshRenderer>().material;
            }
        }
        //Return the newly-declared and filled grid.
        return grid;
    }

    public List<int> GenerateSkipRowIndexes(int minSkips, int maxSkips, int maxDimension, bool addBorderSkips) {
        int skips = Random.Range(minSkips, maxSkips);
        List<int> listSkips = new List<int>();
        for(int i = 0; i < skips; i++) {
            listSkips.Add(Random.Range(0,maxDimension));
        }
        if(addBorderSkips) {
            listSkips.Add(0);
            listSkips.Add(maxDimension-1);
        }
        return listSkips;
    }

    public Square GetSquare(GridCoord gridCoord) {
        return GameController.gc.grid[gridCoord.x,gridCoord.y];
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    [System.Serializable]
    public class GridPrefab {
        public GameObject prefab;
        public float weighting;
        public GridPrefab(GameObject prefab, float weighting) {
            this.prefab = prefab;
            this.weighting = weighting;
        }
    }

    public GameObject PickRandomGridPrefab() {
        float sum = 0;
        foreach(GridPrefab gp in gridPrefabs) {
            sum += gp.weighting;
        }
        float result = Random.Range(0,sum);
        foreach(GridPrefab gp in gridPrefabs) {
            result -= gp.weighting;
            if(result <= 0) {
                return gp.prefab;
            }
        }
        return gridPrefabs[0].prefab;
    }
}
