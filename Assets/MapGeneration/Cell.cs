using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellTypes
{
    start_tile = 0,
    end_tile = 1,
    hall_tile = 2,
    treasure_tile = 3,
    store_tile = 4,
    sacrifice_tile = 5,
    choice_tile = 6,
}

public class Cell : MonoBehaviour
{
    // This is here due to some random issues with rounding of regular positions;
    public string PositionString = string.Empty;
    public List<Cell> NextCells = new List<Cell>();
    public CellTypes CellType = CellTypes.hall_tile;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public SpriteRenderer Renderer;

    public void SetupCell(Sprite sprite)
    {
        if (Renderer == null)
            Renderer = gameObject.AddComponent<SpriteRenderer>();
        Renderer.sortingOrder = 1;
        CellSprite = sprite;
        gameObject.name = $"{CellSprite.name}:{PositionString}";
    }

    public Sprite CellSprite
    {
        get => Renderer.sprite;
        set => Renderer.sprite = value;
    }

    public static string GeneratePositionString(Vector3 position)
    {
        Vector3Int positionInt = new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y)
        );
        return $"({positionInt.x}:{positionInt.y}:{positionInt.z})";
    }
}
