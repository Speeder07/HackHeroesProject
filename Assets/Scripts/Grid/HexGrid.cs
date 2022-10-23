using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class HexGrid : NetworkBehaviour
{
    
    public static HexGrid Instance;
    void Awake()=>Instance = this;

    public uint radius;
    HexField[,] gridArray;
    
    [SerializeField]HexFieldGo prefab;

    public override void OnNetworkSpawn()
    {
        new WorldGenerator(this.radius, prefab);
    }


    private (int x, int y) HexToId(HexCoordinate coord) =>
    (x: (int)this.radius + coord.Q, y: (int)this.radius + coord.R);

    private bool IsOutside(HexCoordinate coords) => 
    (Mathf.Abs(coords.Q) > this.radius || 
     Mathf.Abs(coords.R) > this.radius);

    public HexField Cell(HexCoordinate coordinate)
    {
        (int x, int y) arrayIndex = HexToId(coordinate);
        if(IsOutside(coordinate)) return null;
        return gridArray[arrayIndex.x, arrayIndex.y];
    }
    public HexField Cell(short q, short r)=>Cell(new HexCoordinate(r , q));


    public void All(Action<HexField> action)
    {
        foreach (HexField field in gridArray)
        {
            if (field==null) continue;
            {
                action(field);
            }
        }
    }

    public List<HexField> Neighbors(HexCoordinate coordinate)
    {
        List<HexField> temp = new List<HexField>();
        if (Cell(coordinate + new HexCoordinate(0,1))!=null) temp.Add(Cell(coordinate + new HexCoordinate(0,1)));
        if (Cell(coordinate + new HexCoordinate(0,-1))!=null) temp.Add(Cell(coordinate + new HexCoordinate(0,-1)));
        if (Cell(coordinate + new HexCoordinate(-1,0))!=null) temp.Add(Cell(coordinate + new HexCoordinate(-1,0)));
        if (Cell(coordinate + new HexCoordinate(1,0))!=null) temp.Add(Cell(coordinate + new HexCoordinate(1,0)));
        if (Cell(coordinate + new HexCoordinate(-1,1))!=null) temp.Add(Cell(coordinate + new HexCoordinate(-1,1)));
        if (Cell(coordinate + new HexCoordinate(1,-1))!=null) temp.Add(Cell(coordinate + new HexCoordinate(1,-1)));
        return temp;
    }
    public HexFieldGo InstantiateHexField(HexField field, HexFieldGo prefab)
    {
        HexFieldGo go = Instantiate<HexFieldGo>(prefab, field.Coordinates.ToVector3(), Quaternion.identity);
        field.Instance = go;
        go.transform.SetParent(transform);
        go.field = field;
        return go;
    }

    [ClientRpc] public void InstantiateHexField_ClientRpc(PlaningStage.PlanStruct[] activeTiles)
    {
        foreach (PlaningStage.PlanStruct item in activeTiles)
        {
            Destroy(Cell(item.coordinate).Instance.gameObject);
            HexFieldGo go = Instantiate<HexFieldGo>(GameMenager.Instance.hologramPrefabs[item.id].GetPrefab(item.variant), item.coordinate.ToVector3(), Quaternion.identity);
            Cell(item.coordinate).Instance = go;
            go.transform.SetParent(transform);
            go.field = Cell(item.coordinate);
            HexGrid.Instance.Cell(item.coordinate).Instance.StartCoroutine(HexGrid.Instance.Cell(item.coordinate).Instance.SelectTile(new Vector3(0,0.5f,0)));
        }
    }

    private struct WorldGenerator
    {
        HexGrid grid => HexGrid.Instance;
        public WorldGenerator(uint radius, HexFieldGo prefab)
        {
            grid.gridArray = new HexField[radius*2+1, radius*2+1];
            for (short q = (short)-radius; q <= radius; q++)
            {
                for (short r = (short)-radius; r <= radius; r++)
                {
                    for (int s = (short)-radius; s <= radius; s++)
                    {
                        HexCoordinate newCoord = new HexCoordinate(r, q);
                        if (q + r + s!=0) continue;

                        (int x, int y) arrayIndex = grid.HexToId(newCoord);

                        HexFieldGo go = Instantiate<HexFieldGo>(prefab, newCoord.ToVector3(), Quaternion.identity);
                        grid.gridArray[arrayIndex.x, arrayIndex.y] = new HexField(
                            newCoord, 0, 0, go
                        );
                        go.id = -1;
                        go.transform.SetParent(grid.transform);
                    }
                }
            }
        }
    }

}