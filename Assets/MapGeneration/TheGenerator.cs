using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct CellRoom
{
    public Cell cell;
    public GameObject roomParent;
    public Room room;
}

public static class CustomColors
{
    public static Color32 black = new Color32(0, 0, 0, 255);
}

public class TheGenerator : MonoBehaviour
{
    [SerializeField]
    TheConductor theConductor;
    GameObject theGrid;

    List<CellRoom> Rooms = new List<CellRoom>();

    public TileBase floorTile;

    // Start is called before the first frame update
    void Start()
    {
        theGrid = GameObject.Find("Grid");
    }

    public void GenerateCells(TheConductor _conductor)
    {
        theConductor = _conductor;
        List<Cell> cells = theConductor.cells.FindAll(e => e.CellType == CellType.choice_tile);
        for (int i = 0; i < cells.Count; i++)
        {
            Cell cell = cells[i];

            CellRoom cellRoom = new CellRoom { cell = cell };
            GameObject parent = new GameObject($"room:{cell.CellType}:{cell.PositionString}");
            parent.transform.SetParent(theGrid.transform);
            cellRoom.room = parent.AddComponent<Room>();

            GameObject floor = new GameObject("Floor");
            floor.transform.SetParent(parent.transform);
            Tilemap floorTilemap = floor.AddComponent<Tilemap>();
            TilemapRenderer floorTilemapRenderer = floor.AddComponent<TilemapRenderer>();
            floorTilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopLeft;

            Sprite[] validLayouts = Resources.LoadAll<Sprite>($"Rooms/{cell.CellType}/");
            Texture2D selectedLayout = validLayouts[Random.Range(0, validLayouts.Length)].texture;
            Color[] pixels = selectedLayout.GetPixels();
            Vector2 originPoint = new Vector2(selectedLayout.width, selectedLayout.height) / -2;
            for (int j = 0; j < selectedLayout.width; j++)
            {
                for (int k = 0; k < selectedLayout.height; k++)
                {
                    TileBase tileToSet = null;
                    Color32 pixel = pixels[j + k * (selectedLayout.width)];
                    Vector3 targetTile = new Vector2(originPoint.x + j, originPoint.y + k);
                    if (Equals(pixel, CustomColors.black))
                    {
                        tileToSet = floorTile;
                    }
                    floorTilemap.SetTile(targetTile.ToVectorInt(), tileToSet);
                }
            }

            Vector3 previousCellDirection = (
                cell.PreviousCell.cell.Position - cell.Position
            ).normalized;
            addPath(
                previousCellDirection,
                floorTilemap,
                new Vector2(selectedLayout.width, selectedLayout.height)
            );
        }
    }

    void addPath(Vector3 direction, Tilemap tilemap, Vector2 roomSize) {
        Vector3 centerPoint1 = 
    }

    // Update is called once per frame
    void Update() { }
}
