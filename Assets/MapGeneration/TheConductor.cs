using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TheConductor : MonoBehaviour
{
    public Vector3 startingPosition;

    [SerializeField]
    Vector3 endPosition;

    public string levelName = "DEBUG";

    [SerializeField, MinMaxSlider(0, 10)]
    private Vector2 pathToEndRange = new Vector2(2, 5);

    [SerializeField, MinMaxSlider(0, 10)]
    private Vector2 pathToTreasureRange = new Vector2(0, 2);

    [SerializeField, Range(0, 10)]
    private int coridorCount = 1;

    [Serializable]
    public struct NamedImage
    {
        public CellTypes cellType;
        public int chance;
    }

    public List<NamedImage> optionalCells;

    [SerializeField]
    List<Cell> cells;

    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private Sprite spriteConnector;
    private GameObject levelLayout;

    IEnumerator ExampleCoroutine()
    {
        yield return new WaitForSeconds(5);
    }

    Cell ReplaceCell(Cell cell, CellTypes cellType)
    {
        cell.CellType = cellType;
        cell.SetupCell(sprites[(int)cellType]);
        return cell;
    }

    Cell CreateCell(Vector3 position, CellTypes cellType)
    {
        Vector3Int positionInt = new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y)
        );
        GameObject cellGameObject = new GameObject();
        Cell cell = cellGameObject.AddComponent<Cell>();
        cell.transform.SetParent(levelLayout.transform);
        cell.Position = positionInt;
        cell.PositionString = Cell.GeneratePositionString(positionInt);
        cell.CellType = cellType;
        cell.SetupCell(sprites[(int)cellType]);
        cells.Add(cell);
        return cell;
    }

    Cell CreateCellAtRandomNeightbor(Cell previousCell, CellTypes cellType)
    {
        List<Vector3> validPositions = new List<Vector3>
        {
            Vector3.left,
            Vector3.right,
            Vector3.up,
            Vector3.down
        };
        while (validPositions.Count > 0)
        {
            // Select Random Spot from validPositions List
            Vector3 position = validPositions[
                UnityEngine.Random.Range(0, validPositions.Count - 1)
            ];
            Vector3Int positionInt = new Vector3Int(
                Mathf.RoundToInt(position.x + previousCell.Position.x),
                Mathf.RoundToInt(position.y + previousCell.Position.y)
            );
            string positionString = Cell.GeneratePositionString(positionInt);

            // Attempt to find a valid spot...
            if (cells.Find(e => e.PositionString == positionString) == null)
            {
                Cell newCell = CreateCell(positionInt, cellType);
                previousCell.NextCells.Add(newCell);
                return newCell;
            }
            else
                validPositions.Remove(position);
        }
        return null;
    }

    Cell GeneratePath(int length, Cell startingCell, CellTypes finalCellType)
    {
        Cell previousCell = startingCell;
        int _length = length;
        while (_length > 0)
        {
            if (previousCell == null)
                break;
            previousCell = CreateCellAtRandomNeightbor(previousCell, CellTypes.hall_tile);
            _length--;
        }
        if (previousCell == null)
            return cells.Last();
        return CreateCellAtRandomNeightbor(previousCell, finalCellType);
    }

    void Start()
    {
        levelLayout = GameObject.Find("LevelLayout");
        StartCoroutine(ExampleCoroutine());
        sprites = new Sprite[Enum.GetValues(typeof(CellTypes)).Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Resources.Load<Sprite>($"Sprites/Level {levelName}/{(CellTypes)i}");
        }
        spriteConnector = Resources.Load<Sprite>($"Sprites/Level {levelName}/connector");

        startingPosition = transform.position;
        Cell previousCell = CreateCell(startingPosition, CellTypes.start_tile);

        //Generate Hall To Boss
        GeneratePath(
            Mathf.RoundToInt(UnityEngine.Random.Range(pathToEndRange.x, pathToEndRange.y)),
            previousCell,
            CellTypes.end_tile
        );
        //Generate Hall To Treasure
        GeneratePath(
            Mathf.RoundToInt(
                UnityEngine.Random.Range(pathToTreasureRange.x, pathToTreasureRange.y)
            ),
            previousCell,
            CellTypes.treasure_tile
        );

        //Generate Halls To Random Positions
        List<Cell> expansionCells = cells.FindAll(e =>
            e.CellSprite.name == CellTypes.hall_tile.ToString()
        );
        for (int i = 0; i < coridorCount; i++)
        {
            Cell corrCell = expansionCells[UnityEngine.Random.Range(0, expansionCells.Count - 1)];
            GeneratePath(UnityEngine.Random.Range(0, 3), corrCell, CellTypes.hall_tile);
        }

        //Generate Bonus Cells

        for (int i = 0; i < optionalCells.Count; i++)
        {
            List<Cell> hallCells = cells.FindAll(e => e.CellType == CellTypes.hall_tile);
            List<Cell> endCells = hallCells.FindAll(e => e.NextCells.Count == 0);

            if (UnityEngine.Random.Range(0, 100) > optionalCells[i].chance)
                continue;

            if (endCells.Count > 0)
            {
                Cell corrCell = endCells[UnityEngine.Random.Range(0, endCells.Count - 1)];
                ReplaceCell(corrCell, optionalCells[i].cellType);
                continue;
            }

            List<Cell> lonelyCells = hallCells.FindAll(e => e.NextCells.Count < 4);
            if (lonelyCells.Count > 0)
            {
                Cell corrCell = lonelyCells[UnityEngine.Random.Range(0, lonelyCells.Count - 1)];
                CreateCellAtRandomNeightbor(corrCell, optionalCells[i].cellType);
                continue;
            }

            print("No more valid cells found for generation... Breaking out of loop.");
            break;
        }

        // Generate Connecting Graphics
        foreach (Cell _cell in cells)
        {
            foreach (var _neighbour in _cell.NextCells)
            {
                GameObject connector = new GameObject();
                connector.transform.SetParent(levelLayout.transform);
                connector.name = $"connector:{_cell.PositionString}:{_neighbour.PositionString}";
                connector.AddComponent<SpriteRenderer>().sprite = spriteConnector;
                connector.transform.position = new Vector3(
                    (_cell.Position.x + _neighbour.Position.x) / 2,
                    (_cell.Position.y + _neighbour.Position.y) / 2
                );

                Vector3Int directionVector = new Vector3Int(
                    Mathf.RoundToInt(_cell.Position.x - _neighbour.Position.x),
                    Mathf.RoundToInt(_cell.Position.y - _neighbour.Position.y)
                );
                if (MathF.Abs(directionVector.y) == 1)
                {
                    connector.transform.Rotate(new Vector3(0, 0, 90), Space.World);
                }

                print(directionVector);
            }
        }
    }

    void Update() { }
}
