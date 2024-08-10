using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellType
{
    start_tile = 0,
    end_tile = 1,
    hall_tile = 2,
    treasure_tile = 3,
    store_tile = 4,
    sacrifice_tile = 5,
    choice_tile = 6,
}

public struct CellConnection
{
    public Cell cell;
    public CellType cellType;
    public GameObject connector;
}

public class Cell : MonoBehaviour
{
    // This is here due to some random issues with rounding of regular positions;
    public string PositionString = string.Empty;
    public List<CellConnection> NextCells = new List<CellConnection>();
    public CellType CellType = CellType.hall_tile;

    public Transform Parent
    {
        set { transform.SetParent(value); }
    }

    public Vector3 Position
    {
        get { return gameObject.GetComponent<RectTransform>().anchoredPosition; }
        set { gameObject.GetComponent<RectTransform>().anchoredPosition = value; }
    }

    public Vector2 Size
    {
        get { return gameObject.GetComponent<RectTransform>().sizeDelta; }
        set { gameObject.GetComponent<RectTransform>().sizeDelta = value; }
    }

    public Image Renderer;

    public void AddConnection(Cell cell, CellType cellType)
    {
        CellConnection cellConnection = new CellConnection();
        cellConnection.cell = cell;
        cellConnection.cellType = cellType;
        cellConnection.connector = null;
        NextCells.Add(cellConnection);
    }

    public void SetupCell(Sprite sprite)
    {
        if (Renderer == null)
            Renderer = gameObject.AddComponent<Image>();
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
