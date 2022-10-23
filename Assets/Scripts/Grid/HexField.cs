using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexField
{
    [SerializeField] HexCoordinate _coords;
    public HexCoordinate Coordinates {get => _coords;}
    public HexFieldGo Instance;
    public Vector3 Position => this.Instance.transform.position;
    public int HexFieldId = 0;

    public HexField(HexCoordinate coordinate, int light, int water, HexFieldGo go)
    {
        _coords = coordinate;
        this.Instance = go;
        this.Instance.setField(this);
    }
}