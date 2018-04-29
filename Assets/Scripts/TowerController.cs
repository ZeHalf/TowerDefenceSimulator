﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour
{

    public ClickState clickState = ClickState.Default;
    public enum ClickState
    {
        Default,
        PlacingTower,
        TowerSelect
    }

    public int indexPlacingTower;
    public int indexSelectedTower;
    public GameObject placingTower;
    public BasicTower selectedTower;
    public Material canBuildMaterial;
    public Material canNotBuildMaterial;

    public List<BasicTower> towers;

    private Vector3 tapPosition;
    private MeshRenderer placingBlankMesh;

    public delegate void TowerEventHandler(int x);
    public static event TowerEventHandler OnBuyTower, OnSellTower;


    private void OnEnable()
    {
        BasicTower.OnClickTower += SelectTower;
    }

    private void OnDisable()
    {
        BasicTower.OnClickTower -= SelectTower;
    }

    // Update is called once per frame
    void Update()
    {
        ClickStateBehaviour();
    }

    /// <summary>
    /// Функция для инициации начала размещения башни
    /// </summary>
    /// <param name="id">Порядковый номер в массиве префабов башен в GameController</param>
    public void BeginPlacingTower(int id)
    {
        if (clickState != ClickState.PlacingTower)
        {
            if (GameController.Instance.AcceptBuyTower(id))
            {
                indexPlacingTower = id;
                ChangeClickState(ClickState.PlacingTower);
            }
        }
    }

    /// <summary>
    /// Завершение размещения башни
    /// </summary>
    public void EndPlacingTower()
    {
        if (clickState == ClickState.PlacingTower)
        {
            ChangeClickState(ClickState.Default);
        }
    }

    public void SelectTower(BasicTower tower)
    {
        if (clickState == ClickState.TowerSelect)
        {
            ChangeClickState(ClickState.Default);
        }

        if (selectedTower == null)
        {
            indexSelectedTower = towers.FindIndex(x => x.GetComponent<BasicTower>() == tower);

            ChangeClickState(ClickState.TowerSelect);
        }
    }

    public void SellSelectedTower()
    {
        SellTower(selectedTower);
        towers.Remove(selectedTower);
        Destroy(selectedTower.gameObject);
        ChangeClickState(ClickState.Default);
    }

    public void BuyTower(BasicTower tower)
    {
        GameController.Instance.SpendMoney(tower.price);

        if (OnBuyTower != null)
            OnBuyTower.Invoke(tower.price);
    }

    public void SellTower(BasicTower tower)
    {
        GameController.Instance.AddMoney(tower.bid);

        if (OnSellTower != null)
            OnSellTower.Invoke(tower.bid);
    }

    public void ChangeClickState(ClickState state)
    {
        if (clickState == state)
            return;

        switch (clickState)
        {
            case ClickState.Default:
                break;
            case ClickState.PlacingTower:
                tapPosition = Input.mousePosition;

                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(tapPosition), out hit);

                if (CheckPlace(hit) && !IsPointerOnUI())
                {
                    GameObject towerPrefab = GameController.Instance.towerPrefabs[indexPlacingTower];
                    GameObject newTowerObject = Instantiate(towerPrefab, placingTower.transform.position, placingTower.transform.rotation);
                    towers.Add(newTowerObject.GetComponent<BasicTower>());

                    BuyTower(towers[towers.Count - 1]);
                }

                Destroy(placingTower.gameObject);

                break;
            case ClickState.TowerSelect:
                selectedTower.selector.SetActive(false);
                UIController.Instance.DisableSellTower();
                selectedTower = null;
                break;
        }

        clickState = state;
        switch (state)
        {
            case ClickState.Default:
                break;
            case ClickState.PlacingTower:
                placingTower = Instantiate(GameController.Instance.towerTemps[indexPlacingTower]);
                placingBlankMesh = placingTower.transform.GetChild(0).GetComponent<MeshRenderer>();
                break;
            case ClickState.TowerSelect:
                selectedTower = towers[indexSelectedTower];
                selectedTower.selector.SetActive(true);
                UIController.Instance.EnableSellTower(selectedTower.GetComponent<BasicTower>());
                break;
        }
    }

    /// <summary>
    /// Главная функция
    /// </summary>
    private void ClickStateBehaviour()
    {
        switch (clickState)
        {
            case ClickState.Default:
                //if (Input.GetMouseButtonDown(0))
                //{

                //}
                //if (Input.touchCount == 1)
                //{
                //    if (Input.GetTouch(0).phase == TouchPhase.Began)
                //    {
                //        RaycastHit hitInfo;
                //        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hitInfo))
                //        {
                //            if (hitInfo.collider.CompareTag("Tower"))
                //            {
                //                SelectTower(hitInfo.collider.transform.parent.GetComponent<BasicTower>());
                //            }
                //        }

                //    }
                //}
                break;
            case ClickState.PlacingTower:
                tapPosition = Input.mousePosition;

                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(tapPosition), out hit);

                placingTower.transform.position = hit.point;

                break;
            case ClickState.TowerSelect:
                if (Input.GetMouseButtonDown(0))
                {
                    tapPosition = Input.mousePosition;
                    if (!IsPointerOnUI())
                    {
                        ChangeClickState(ClickState.Default);
                    }
                }
                if (Input.touchCount == 1)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        tapPosition = Input.GetTouch(0).position;
                        if (!IsPointerOnUI())
                        {
                                ChangeClickState(ClickState.Default);
                        }
                    }
                }
                break;
        }
    }
    private bool IsPointerOnUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = tapPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        return results.FindIndex(r => r.gameObject.layer == 5) > -1;
    }

    //private bool RaycastToCursorPosition()
    //{
    //    RaycastHit hitInfo;
    //    if (Physics.Raycast(Camera.main.ScreenPointToRay(tapPosition), out hitInfo))
    //    {
    //        if (!IsPointerOnUI())
    //        {
    //            if (!hitInfo.collider.CompareTag("Tower"))
    //            {
    //                if (IsPointerOnUI())
    //                    return false;
    //                else
    //                    return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        return true;
    //    }
    //}
    /// <summary>
    /// Функция проверки местности на возможность размещения башни. Если можно, то вернет true.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private bool CheckPlace(RaycastHit hit)
    {
        if (hit.collider.CompareTag("CanBuild"))
        {
            if (placingBlankMesh.material != canBuildMaterial)
                placingBlankMesh.material = canBuildMaterial;
            return true;
        }
        else
        {
            if (placingBlankMesh.material != canNotBuildMaterial)
                placingBlankMesh.material = canNotBuildMaterial;
            return false;
        }
    }
}