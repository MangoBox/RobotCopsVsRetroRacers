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
        float offsetX = (dimensionsX * gridSpacing) / 2;
        float offsetY = (dimensionsY * gridSpacing) / 2;
        List<int> xSkips = GenerateSkipRowIndexes(minRowSkips,maxRowSkips,dimensionsX,true);
        List<int> ySkips = GenerateSkipRowIndexes(minRowSkips,maxRowSkips,dimensionsY,true);

        Square[,] grid = new Square[dimensionsX, dimensionsY];
        for(int x = 0; x < dimensionsX; x++) {
            for(int y = 0; y < dimensionsY; y++) {
                GameObject chosenPrefab = PickRandomGridPrefab();
                if(xSkips.Contains(x) || ySkips.Contains(y)) {
                    chosenPrefab = gridPrefabs[0].prefab;
                }
                GameObject spawnedGrid = Instantiate(chosenPrefab);
                spawnedGrid.transform.position = new Vector3(x * gridSpacing - offsetX, 0, y*gridSpacing - offsetY);
                Square squareObj = spawnedGrid.GetComponent<Square>();
                grid[x,y] = squareObj;
                squareObj.coord = new GridCoord(x,y);
                squareObj.squareMaterial = squareObj.GetComponentInChildren<MeshRenderer>().material;
            }
        }
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
