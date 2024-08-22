using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;

public class Room : MonoBehaviour
{
    public Cell cell;
    public Door[] doors;
    public List<GameObject> enemies = new List<GameObject>();
    public bool roomCleared = false;

    public void Generate(TheGenerator _generator, TheConductor _conductor)
    {
        var Connections = cell.Connections;
        doors = GetComponentsInChildren<Door>();
        for (int i = 0; i < doors.Length; i++)
        {
            Vector3 doorDir = doors[i].transform.localPosition.Direction();
            for (int j = 0; j < Connections.Count; j++)
            {
                CellConnection connection = Connections[j];
                Vector3 conDir = (connection.cell.Position - cell.Position).Direction();
                doors[i].Generate(_generator, _conductor, cell);
                if (doorDir == conDir)
                {
                    connection.targetDoor = doors[i];
                    doors[i].doorEnabled = true;
                    doors[i].connection = connection;
                    doors[i].SetState(DoorState.closed);
                    if (
                        connection.cellType != CellType.start_tile
                        && connection.cellType != CellType.hall_tile
                    )
                        doors[i].AddIcon(connection);
                }
            }
        }

        Tilemap wallsTilemap = transform.Find("walls").GetComponent<Tilemap>();
        for (int i = 0; i < doors.Length; i++)
        {
            var door = doors[i];
            var doorPos = door.transform.localPosition;
            if (door.doorEnabled == false)
            {
                if (doorPos.Direction().x != 0)
                {
                    var target = doorPos.ToInt();
                    if (doorPos.x < 0)
                        target += Vector3Int.left;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                    target += Vector3Int.down;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                    if (doorPos.x < 0)
                        target += Vector3Int.left;
                    else
                        target += Vector3Int.right;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                    target += Vector3Int.up;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                }
                else
                {
                    var target = doorPos.ToInt();
                    if (doorPos.y < 0)
                        target += Vector3Int.down;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                    target += Vector3Int.left;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                    if (doorPos.y < 0)
                        target += Vector3Int.down;
                    else
                        target += Vector3Int.up;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                    target += Vector3Int.right;
                    wallsTilemap.SetTile(target, _generator.wallTile);
                }
                Destroy(door.gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        if (roomCleared)
            return;

        if (enemies.Count == 0)
        {
            for (int i = 0; i < doors.Length; i++)
            {
                doors[i].SetState(DoorState.open);
                roomCleared = true;
            }
        }
    }
}
