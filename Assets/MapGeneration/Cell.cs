using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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

[Serializable]
public struct CellConnection
{
    public Cell cell;
    public CellType cellType;
    public Door targetDoor;
}

public class Cell : MonoBehaviour
{
    GameObject iconGameObject = null;

    // This is here due to some random issues with rounding of regular positions;
    public string PositionString = string.Empty;
    public List<CellConnection> Connections = new List<CellConnection>();

    public CellType CellType = CellType.hall_tile;

    public Transform Parent
    {
        set { transform.SetParent(value); }
    }

    public Vector3 Position;
    public Vector3 IconPosition
    {
        get { return gameObject.GetComponent<RectTransform>().anchoredPosition; }
        set { gameObject.GetComponent<RectTransform>().anchoredPosition = value; }
    }

    public Vector2 Size
    {
        get { return gameObject.GetComponent<RectTransform>().sizeDelta; }
        set
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = value;
            iconGameObject.GetComponent<RectTransform>().sizeDelta = Vector2.one * (value.y - 2);
        }
    }

    public float Scale
    {
        get { return gameObject.GetComponent<RectTransform>().localScale.x; }
        set
        {
            gameObject.GetComponent<RectTransform>().localScale = Vector3.one * value;
            iconGameObject.GetComponent<RectTransform>().localScale = Vector3.one * value * 0.8f;
        }
    }

    public Image RendererIcon;
    public Sprite frame;
    public Sprite frameActive;
    public Image RendererFrame;

    private bool _active;
    public bool Active
    {
        get { return _active; }
        set
        {
            _active = value;
            RendererFrame.sprite = _active ? frameActive : frame;
        }
    }

    public void AddConnection(Cell cell, CellType cellType)
    {
        CellConnection cellConnection = new CellConnection();
        cellConnection.cell = cell;
        cellConnection.cellType = cellType;
        cellConnection.targetDoor = null;
        Connections.Add(cellConnection);
    }

    public void SetupCell(Sprite _frame, Sprite _frameActive, Sprite icon)
    {
        if (RendererFrame == null)
        {
            RendererFrame = gameObject.AddComponent<Image>();
        }
        RendererFrame.type = Image.Type.Sliced;
        RendererFrame.pixelsPerUnitMultiplier = 16;
        frame = _frame;
        frameActive = _frameActive;
        Active = false;

        if (iconGameObject == null)
        {
            iconGameObject = new GameObject();
            iconGameObject.transform.SetParent(transform);
            iconGameObject.name = "Icon";
            iconGameObject.AddComponent<RectTransform>();
        }

        RectTransform iconRectTransform = iconGameObject.GetComponent<RectTransform>();
        iconRectTransform.anchoredPosition = Vector3.zero + Vector3.up * 1;
        iconRectTransform.sizeDelta = Vector2.one * Size.y;

        if (RendererIcon == null)
        {
            RendererIcon = iconGameObject.AddComponent<Image>();
        }
        RendererIcon.sprite = icon;

        gameObject.name = $"{CellType}:{PositionString}";
    }

    public Sprite IconSprite
    {
        get => RendererIcon.sprite;
        set => RendererIcon.sprite = value;
    }

    public static string GeneratePositionString(Vector3 position)
    {
        Vector3Int positionInt = position.ToInt();
        return $"({positionInt.x}:{positionInt.y}:{positionInt.z})";
    }
}
