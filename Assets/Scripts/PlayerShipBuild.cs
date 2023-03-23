using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipBuild : MonoBehaviour
{
    [SerializeField] GameObject[] shopButtons;
    GameObject target;
    GameObject tmpSelection;
    void Start()
    {
        TurnOffSelectionHighlights();
    }

    void TurnOffSelectionHighlights()
    {
        for (int i = 0; i < shopButtons.Length; i++)
        {
            shopButtons[i].SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        AttemptSelection();
    }

    GameObject ReturnClickedObject( out RaycastHit hit)
    {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray.origin, ray.direction * 100, out hit))
        {
            target = hit.collider.gameObject;
        }

        return target;
    }

    void AttemptSelection()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            target = ReturnClickedObject(out hitInfo);
            if(target != null)
            {
                if(target.transform.Find("itemText"))
                {
                    TurnOffSelectionHighlights();
                    Select();
                }
            }
        }
    }

    void Select()
    {
        tmpSelection = target.transform.Find("SelectionQuad").gameObject;
        tmpSelection.SetActive(true);
    }
}
