using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TheConductor : MonoBehaviour
{
    public Vector3 startingPosition;

    [SerializeField]
    Vector3 endPosition;

    public string levelName = "DEBUG";

    [SerializeField, MinMaxSlider(0, 10)]
    private Vector2Int pathToBossR = new Vector2Int(2, 5);

    [SerializeField, MinMaxSlider(0, 10)]
    private Vector2Int pathToChestR = new Vector2Int(0, 2);

    [SerializeField, Range(0, 10)]
    private int coridorCount = 1;

    [Serializable]
    public struct NamedImage
    {
        public CellType cellType;
        public int chance;
    }

    public List<NamedImage> optionalCells;

    [SerializeField]
    List<Cell> cells;

    // Sprites
    private Sprite[] sprites;
    private Sprite spriteConnectorH;
    private Sprite spriteConnectorV;

    private GameObject minimapCanvas;

    Cell ReplaceCell(Cell cell, CellType cellType)
    {
        cell.CellType = cellType;
        cell.SetupCell(sprites[(int)cellType]);
        return cell;
    }

    Cell CreateCell(Vector3 position, CellType cellType)
    {
        Vector3Int positionInt = new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y)
        );
        GameObject cellGameObject = new GameObject();
        Cell cell = cellGameObject.AddComponent<Cell>();
        cell.Parent = minimapCanvas.transform;
        cell.AddComponent<RectTransform>();
        cell.Position = positionInt;
        cell.Size = Vector2.one * 16;
        cell.PositionString = Cell.GeneratePositionString(positionInt);
        cell.CellType = cellType;
        cell.SetupCell(sprites[(int)cellType]);
        cells.Add(cell);
        return cell;
    }

    Cell CreateCellAtRandomNeightbor(Cell previousCell, CellType cellType)
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
                previousCell.AddConnection(newCell, newCell.CellType);
                return newCell;
            }
            else
                validPositions.Remove(position);
        }
        return null;
    }

    Cell GeneratePath(int length, Cell startingCell, CellType finalCellType)
    {
        Cell previousCell = startingCell;
        int _length = length;
        while (_length > 0)
        {
            if (previousCell == null)
                break;
            previousCell = CreateCellAtRandomNeightbor(previousCell, CellType.hall_tile);
            _length--;
        }
        if (previousCell == null)
            return cells.Last();
        return CreateCellAtRandomNeightbor(previousCell, finalCellType);
    }

    void Start()
    {
        minimapCanvas = GameObject.Find("MinimapCanvas");
        sprites = new Sprite[Enum.GetValues(typeof(CellType)).Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Resources.Load<Sprite>($"Sprites/Level {levelName}/{(CellType)i}");
        }
        spriteConnectorH = Resources.Load<Sprite>($"Sprites/Level {levelName}/conn_hor");
        spriteConnectorV = Resources.Load<Sprite>($"Sprites/Level {levelName}/conn_vert");

        startingPosition = transform.position;
        Cell startCell = CreateCell(startingPosition, CellType.start_tile);

        return;
        //Generate Hall To Boss
        GeneratePath(
            Mathf.RoundToInt(UnityEngine.Random.Range(pathToBossR.x, pathToBossR.y)),
            startCell,
            CellType.end_tile
        );
        //Generate Hall To Treasure
        GeneratePath(
            Mathf.RoundToInt(UnityEngine.Random.Range(pathToChestR.x, pathToChestR.y)),
            startCell,
            CellType.treasure_tile
        );

        //Generate Halls To Random Positions
        List<Cell> expansionCells = cells.FindAll(e =>
            e.CellSprite.name == CellType.hall_tile.ToString()
        );
        for (int i = 0; i < coridorCount; i++)
        {
            Cell corrCell = expansionCells[UnityEngine.Random.Range(0, expansionCells.Count - 1)];
            GeneratePath(UnityEngine.Random.Range(0, 3), corrCell, CellType.hall_tile);
        }

        //Generate Bonus Cells

        for (int i = 0; i < optionalCells.Count; i++)
        {
            List<Cell> hallCells = cells.FindAll(e => e.CellType == CellType.hall_tile);
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
        for (int i = 0; i < cells.Count; i++)
        {
            Cell _cell = cells[i];
            for (int j = 0; j < _cell.NextCells.Count; j++)
            {
                CellConnection cellConnection = _cell.NextCells[j];
                GameObject connector = new GameObject();
                connector.transform.SetParent(minimapCanvas.transform);
                connector.name =
                    $"connector:{_cell.PositionString}:{cellConnection.cell.PositionString}";

                connector.transform.position = new Vector3(
                    (_cell.Position.x + cellConnection.cell.Position.x) / 2,
                    (_cell.Position.y + cellConnection.cell.Position.y) / 2
                );

                cellConnection.connector = connector;
                Vector3Int directionVector = new Vector3Int(
                    Mathf.RoundToInt(_cell.Position.x - cellConnection.cell.Position.x),
                    Mathf.RoundToInt(_cell.Position.y - cellConnection.cell.Position.y)
                );
                if (MathF.Abs(directionVector.y) == 1)
                    connector.AddComponent<UnityEngine.UI.Image>().sprite = spriteConnectorV;
                else
                    connector.AddComponent<UnityEngine.UI.Image>().sprite = spriteConnectorH;
            }
        }
    }

    void Update() { }
}
