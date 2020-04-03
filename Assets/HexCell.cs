using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}
public enum HexEdgeType
{
    Flat, Slope, Cliff
}
public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }
    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
    public static HexDirection Previous2(this HexDirection direction)
    {
        direction -= 2;
        return direction >= HexDirection.NE ? direction : (direction + 6);
    }
    public static HexDirection Next2(this HexDirection direction)
    {
        direction += 2;
        return direction <= HexDirection.NW ? direction : (direction - 6);
    }
}

public class HexCell : MonoBehaviour {
    public HexCoordinates coordinates;
    public HexGridChunk chunk;

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
            {
                return;
            }
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y +=
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.elevationPerturbStrength;

            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            //uiPosition.z = elevation * -HexMetrics.elevationStep;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            if (hasOutgoingRiver &&
                elevation < GetNeighbor(outgoingRiver).elevation)
            {
                RemoveOutgoingRiver();
            }
            if (hasIncomingRiver &&
                elevation > GetNeighbor(incomingRiver).elevation)
            {
                RemoveIncomingRiver();
            }

            for(int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }

            Refresh();
        }
    }
    public int elevation = int.MinValue;
    public RectTransform uiRect;
    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    [SerializeField]
    HexCell[] neighbors;

    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }
    Color color;

    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;
    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }
    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }
    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }
    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }
    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }
    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            hasIncomingRiver && IncomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
    }
    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }
    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }
        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        hasOutgoingRiver = true;
        outgoingRiver = direction;
        //RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        //neighbor.RefreshSelfOnly();

        SetRoad((int)direction, false);
    }
    
    public float StreamBedY
    {
        get
        {
            return
                (elevation + HexMetrics.streamBedElevationOffset) *
                HexMetrics.elevationStep;
        }
    }
    
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType (HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            elevation, neighbors[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell othercell)
    {
        return HexMetrics.GetEdgeType(
            elevation, othercell.elevation);
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for(int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return
                (elevation + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return
                (waterLevel + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    [SerializeField]
    bool[] roads;

    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }
    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }
    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction] &&
            !HasRiverThroughEdge(direction) &&
            GetElevationDifference(direction) <= 1)
        {
            SetRoad((int)direction, true);
        }
    }
    public void RemoveRoads()
    {
        for(int i = 0; i < neighbors.Length; i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    public int GetElevationDifference(HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return difference >= 0 ? difference : -difference;
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            Refresh();
        }
    }
    int waterLevel;

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }
}
