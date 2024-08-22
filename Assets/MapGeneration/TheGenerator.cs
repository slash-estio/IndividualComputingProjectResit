using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TheGenerator : MonoBehaviour
{
    [SerializeField]
    TheConductor theConductor;

    public Sprite doorDisabledSprite;
    public Sprite doorOpenSprite;
    public TileBase wallTile;

    // Start is called before the first frame update
    public void Generate(TheConductor _conductor)
    {
        theConductor = _conductor;
        List<Cell> cells = theConductor.cells;
        for (int i = 0; i < cells.Count; i++)
        {
            GameObject[] roomOpts = Resources.LoadAll<GameObject>($"Rooms/{cells[i].CellType}");

            Cell cell = cells[i];

            // Room Generation
            GameObject roomGO = Instantiate(roomOpts[UnityEngine.Random.Range(0, roomOpts.Length)]);
            roomGO.name = $"{cell.CellType}:{i}";
            roomGO.transform.SetParent(this.transform);
            roomGO.transform.position = cell.Position;

            Room room = roomGO.AddComponent<Room>();

            // Neigbour List
            room.cell = cell;
            room.Generate(this, theConductor);
        }
    }

    // Update is called once per frame
    void Update() { }
}
