using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

[Serializable]
public struct HexCoordinate : INetworkSerializable
{
    [SerializeField] private short _q;
    [SerializeField] private short _r;

    public short Q {get{return _q;}}
    public short R {get{return _r;}}
    public short S => (short)(-_r - _q);

    public HexCoordinate(short r, short q)
    {
        this._r = r;
        this._q = q;
    }

    public override string ToString() => $"(Q:{Q}|R:{R}|S:{S})";

    public Vector3 ToVector3()
    {
        float x = Q * Mathf.Sqrt(3) + R * (Mathf.Sqrt(3)/2);
        float z = R * 1.5f; 
        return new Vector3(x,0,z);
    }

    public bool IsValid => Q + R + S == 0;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _q);
        serializer.SerializeValue(ref _r);
    }

    public static bool operator!= (HexCoordinate obj1, HexCoordinate obj2)
    {
        return !(obj1._q == obj2._q)&&(obj1._r==obj2._r);
    }

    public static bool operator== (HexCoordinate obj1, HexCoordinate obj2)
    {
        return (obj1._q == obj2._q)&&(obj1._r==obj2._r);
    }

    public static HexCoordinate operator+ (HexCoordinate obj1, HexCoordinate obj2)
    {
        return new HexCoordinate((short)(obj1._r + obj2._r), (short)(obj1._q + obj2._q));
    }
}