using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public struct ConnectorObject
{
    public GameObject gameObject;
    public Vector3 originPosition;
}

public class TheConductor : MonoBehaviour
{
    #region Variables
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
    public List<Cell> cells;
    public List<ConnectorObject> connectors = new List<ConnectorObject>();
    TheGenerator generator = null;

    // Sprites
    public Sprite[] spriteCellIcons;
    public Sprite spriteRoomSign;
    private Sprite spriteConnector;
    private Sprite spriteCellFrameActive;
    private Sprite spriteCellFrame;

    private GameObject minimapCanvas;

    public int UISize = 48;
    private int CellSize;

    #endregion

    Cell ReplaceCell(Cell cell, CellType cellType)
    {
        cell.CellType = cellType;
        cell.SetupCell(spriteCellFrame, spriteCellFrameActive, spriteCellIcons[(int)cellType]);
        return cell;
    }

    Cell CreateCell(Vector3 position, CellType cellType)
    {
        Vector3Int positionInt = position.ToInt();
        GameObject cellGameObject = new GameObject();
        Cell cell = cellGameObject.AddComponent<Cell>();
        cell.Parent = minimapCanvas.transform;
        cell.AddComponent<RectTransform>();
        cell.Position = positionInt;
        cell.PositionString = Cell.GeneratePositionString(cell.Position);
        cell.CellType = cellType;
        cell.SetupCell(spriteCellFrame, spriteCellFrameActive, spriteCellIcons[(int)cellType]);
        cell.Size = (Vector2.up + Vector2.right * Utils.Fi) * CellSize;
        cell.Scale = 1f;
        cells.Add(cell);
        return cell;
    }

    Cell CreateCellAtRandomNeightbor(Cell previousCell, CellType cellType)
    {
        List<Vector3> validPositions = new List<Vector3>
        {
            UISize * Utils.Fi * Vector3.left,
            UISize * Utils.Fi * Vector3.right,
            Vector3.up * UISize,
            Vector3.down * UISize
        };
        while (validPositions.Count > 0)
        {
            // Select Random Spot from validPositions List
            Vector3 position = validPositions[UnityEngine.Random.Range(0, validPositions.Count)];
            Vector3Int positionInt = new Vector3(
                position.x + previousCell.Position.x,
                position.y + previousCell.Position.y
            ).ToInt();
            string positionString = Cell.GeneratePositionString(positionInt);

            // Attempt to find a valid spot...
            if (
                cells.Find(e => e.PositionString == positionString) == null
                && Mathf.Abs(positionInt.x) <= UISize * 5
                && Mathf.Abs(positionInt.y) <= UISize * 3
            )
            {
                Cell newCell = CreateCell(positionInt, cellType);
                previousCell.AddConnection(newCell, newCell.CellType);
                newCell.AddConnection(previousCell, previousCell.CellType);
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
        while (length > 0)
        {
            if (previousCell == null)
                break;
            previousCell = CreateCellAtRandomNeightbor(previousCell, CellType.hall_tile);
            length--;
        }
        if (previousCell == null)
            return cells.Last();
        return CreateCellAtRandomNeightbor(previousCell, finalCellType);
    }

    void GenerateBonusCells()
    {
        for (int i = 0; i < optionalCells.Count; i++)
        {
            bool wasTileCreated = false;
            List<Cell> hallCells = cells.FindAll(e => e.CellType == CellType.hall_tile);
            List<Cell> endCells = hallCells.FindAll(e => e.Connections.Count == 0);

            if (UnityEngine.Random.Range(0, 100) > optionalCells[i].chance)
                wasTileCreated = true;

            if (wasTileCreated == false && endCells.Count > 0)
            {
                Cell corrCell = endCells[UnityEngine.Random.Range(0, endCells.Count - 1)];
                ReplaceCell(corrCell, optionalCells[i].cellType);
                wasTileCreated = true;
            }

            List<Cell> lonelyCells = hallCells.FindAll(e => e.Connections.Count < 4);
            if (wasTileCreated == false && lonelyCells.Count > 0)
            {
                Cell corrCell = lonelyCells[UnityEngine.Random.Range(0, lonelyCells.Count - 1)];
                CreateCellAtRandomNeightbor(corrCell, optionalCells[i].cellType);
                wasTileCreated = true;
            }

            if (wasTileCreated == false)
                break;
        }
    }

    void GenerateConnectors()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Cell _cell = cells[i];
            for (int j = 0; j < _cell.Connections.Count; j++)
            {
                CellConnection cellConnection = _cell.Connections[j];
                Cell __cell = cellConnection.cell;
                GameObject connector = new GameObject();
                connector.transform.SetParent(minimapCanvas.transform);
                connector.AddComponent<RectTransform>();
                RectTransform rectTransform = connector.GetComponent<RectTransform>();
                connector.name =
                    $"connector:{_cell.PositionString}:{cellConnection.cell.PositionString}";

                rectTransform.anchoredPosition = (_cell.Position + __cell.Position).AvgPoint();

                Vector2Int directionVector = new Vector2Int(
                    Mathf.Abs(Mathf.RoundToInt(_cell.Position.y - cellConnection.cell.Position.y)),
                    Mathf.Abs(Mathf.RoundToInt(_cell.Position.x - cellConnection.cell.Position.x))
                );

                connector.AddComponent<UnityEngine.UI.Image>().sprite = spriteConnector;
                rectTransform.localScale = Vector3.one;
                rectTransform.sizeDelta =
                    (directionVector / CellSize + Vector2.up + Vector2.right * Utils.Fi)
                    * (UISize - CellSize);

                connectors.Add(
                    new ConnectorObject()
                    {
                        gameObject = connector,
                        originPosition = rectTransform.anchoredPosition,
                    }
                );
            }
        }
    }

    void Start()
    {
        generator = FindAnyObjectByType<TheGenerator>();

        CellSize = UISize - 2;

        minimapCanvas = GameObject.Find("MinimapCanvas");
        RectTransform minimapCanvasRT = minimapCanvas.GetComponent<RectTransform>();
        minimapCanvasRT.sizeDelta = new Vector2(UISize * 8 * Utils.Fi, UISize * 8);

        spriteCellIcons = new Sprite[Enum.GetValues(typeof(CellType)).Length];
        for (int i = 0; i < spriteCellIcons.Length; i++)
        {
            spriteCellIcons[i] = Resources.Load<Sprite>($"Sprites/Level {levelName}/{(CellType)i}");
        }
        spriteRoomSign = Resources.Load<Sprite>($"Sprites/room_sign");

        spriteCellFrame = Resources.Load<Sprite>($"Sprites/Level {levelName}/frame");
        spriteCellFrameActive = Resources.Load<Sprite>($"Sprites/Level {levelName}/frame_active");
        spriteConnector = Resources.Load<Sprite>($"Sprites/Level {levelName}/connector");

        startingPosition = transform.position;
        Cell startCell = CreateCell(startingPosition, CellType.start_tile);
        startCell.Active = true;
        //Generate Hall To Boss
        GeneratePath(
            UnityEngine.Random.Range(pathToBossR.x, pathToBossR.y),
            startCell,
            CellType.end_tile
        );
        //Generate Hall To Treasure
        GeneratePath(
            UnityEngine.Random.Range(pathToChestR.x, pathToChestR.y),
            startCell,
            CellType.treasure_tile
        );

        //Generate Halls To Random Positions
        List<Cell> expansionCells = cells.FindAll(e => e.CellType != CellType.end_tile);
        for (int i = 0; i < coridorCount; i++)
            GeneratePath(
                UnityEngine.Random.Range(0, 3),
                expansionCells[UnityEngine.Random.Range(0, expansionCells.Count - 1)],
                CellType.hall_tile
            );

        //Generate Bonus Cells
        GenerateBonusCells();

        // Generate Connecting Graphics
        GenerateConnectors();
        this.SetMinimapOriginPoint(Vector3.zero);

        generator.Generate(this);
    }

    public void SetMinimapOriginPoint(Vector3 point)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].IconPosition = cells[i].Position - point;
        }

        for (int i = 0; i < connectors.Count; i++)
        {
            connectors[i].gameObject.GetComponent<RectTransform>().anchoredPosition =
                connectors[i].originPosition - point;
        }
    }
}
