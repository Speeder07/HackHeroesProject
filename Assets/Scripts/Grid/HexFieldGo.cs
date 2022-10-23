using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class HexFieldGo : MonoBehaviour
{
    public HexField field;
    public int id;
    public int variant;
    public void setField(HexField field) => this.field = field;

    public IEnumerator SelectTile(Vector3 target)
    {
        Vector3 step = target/100;

        while (transform.position.y <= target.y)
        {
            transform.position+=step;
            yield return new WaitForEndOfFrame();
        }
        
    }

}
