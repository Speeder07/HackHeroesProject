using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMenager : MonoBehaviour
{
    
    public static CameraMenager Instance;

    void Awake()=>Instance = this;
    [SerializeField] Transform lookPoint;
    [SerializeField] Camera camera;

    void Start()
    {
        this.transform.SetParent(lookPoint);
    }

    [SerializeField]Transform tempPrefab; 


    void Update()
    {
        this.transform.LookAt(lookPoint);

        if (Input.GetKey(KeyCode.Q))
        {
            lookPoint.RotateAround(lookPoint.position, lookPoint.up, Time.deltaTime * 90f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            lookPoint.RotateAround(lookPoint.position, lookPoint.up, -Time.deltaTime * 90f);
        }
        /**/
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                HexFieldGo fieldGo;
                if (hit.transform.gameObject.TryGetComponent<HexFieldGo>(out fieldGo))
                {
                    GameMenager.Instance.stage.OnTileClicked(fieldGo);
                }
            }
        }
    }

    
}
