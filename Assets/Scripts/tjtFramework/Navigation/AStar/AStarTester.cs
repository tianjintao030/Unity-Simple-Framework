using System.Collections;
using System.Collections.Generic;
using tjtFramework.Navigation;
using tjtFramework.Utiliy;
using UnityEngine;

public class AStarTester : MonoBehaviour
{
    public AStarGridController grid;
    public Transform startPoint;
    public Transform endPoint;

    private void Start()
    {
        grid.ReSetGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AStarPathFinding.FindPath(grid,startPoint.position,endPoint.position);
        }
    }
}

