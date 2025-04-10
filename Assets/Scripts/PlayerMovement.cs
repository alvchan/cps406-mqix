﻿using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SpeedTree.Importer;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private EdgeCollider2D topLine;
    [SerializeField] private EdgeCollider2D bottomLine;
    [SerializeField] private EdgeCollider2D leftLine;
    [SerializeField] private EdgeCollider2D rightLine;

    [SerializeField] private LineRenderer playerTrail;

    [SerializeField] private GameManager gameManager;

    private GameObject pendingEdge = null;

    private float playerScale;



    private bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board
    private bool isCutting = false;
    private bool startedCutting = false;

    private Directions cutDirection = Directions.Right;
    private enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }
    private List<GameObject> currentEdge = new List<GameObject>();
    
    public void Initialize()
    {
        currentEdge.Add(rightLine.gameObject);
        currentEdge.Add(null);
        playerScale = 1/transform.localScale.x;
    }
    public void PlayerMove()
    {
        if (Input.GetKey(KeyCode.Space) && pendingEdge == null)
        {
            isOnEdge = false;

            if (!startedCutting)
            {
                startedCutting = true;
                startLine();
            }
        }
        else if (!isCutting)
        {
            isOnEdge = true;
            startedCutting = false; // Reset when not cutting
        }


        // Don’t move unless touching an edge
        if (isCutting)
        {
            cutting();
        }
        else
        {
            // Handle pending edge swap only if player is intentionally trying to move
            if (pendingEdge != null)
            {
                bool moveIntent = false;

                if ((pendingEdge == topLine.gameObject || pendingEdge == bottomLine.gameObject) &&
                    (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)))
                {
                    moveIntent = true;
                }
                else if ((pendingEdge == leftLine.gameObject || pendingEdge == rightLine.gameObject) &&
                    (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
                {
                    moveIntent = true;
                }

                if (moveIntent)
                {
                    currentEdge[1] = currentEdge[0];
                    currentEdge[0] = pendingEdge;
                    pendingEdge = null;

                    //Debug.Log("Switched to edge: " + currentEdge[0].name);
                }
            }


            // Direction logic — 8 possible movement combos
            if (currentEdge.Contains(topLine.gameObject))
            {
                cutDirection = Directions.Down;
                moveLeft();
                moveRight();
            }
            if (currentEdge.Contains(bottomLine.gameObject))
            {
                cutDirection = Directions.Up;
                moveLeft();
                moveRight();
            }
            if (currentEdge.Contains(leftLine.gameObject))
            {
                cutDirection = Directions.Right;
                moveUp();
                moveDown();
            }
            if (currentEdge.Contains(rightLine.gameObject))
            {
                cutDirection = Directions.Left;
                moveUp();
                moveDown();
            }
        }
    }
    private void FixedUpdate()
    {
        SnapPlayerOnEdges(currentEdge);
    }


    private void cutting()
    {
        startLine();

        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && cutDirection != Directions.Down)
        {
            cutDirection = Directions.Up;
            moveUp();
        }
        else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && cutDirection != Directions.Up)
        {
            cutDirection = Directions.Down;
            moveDown();
        }
        else if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && cutDirection != Directions.Right)
        {
            cutDirection = Directions.Left;
            moveLeft();
        }
        else if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && cutDirection != Directions.Left)
        {
            cutDirection = Directions.Right;
            moveRight();
        }


        checkingTailUpdate();
    }



    private void startLine()
    {
        Vector3[] temp = new Vector3[playerTrail.positionCount];
        playerTrail.GetPositions(temp);
        List<Vector3> linePoints = temp.ToList();

        // If trail just started, seed it with current position
        if (linePoints.Count == 0)
        {
            linePoints.Add(transform.position);
        }

        // Add current position
        linePoints.Add(transform.position);

        playerTrail.positionCount = linePoints.Count;
        playerTrail.SetPositions(linePoints.ToArray());
    }






    private void checkingTailUpdate()
    {
        Vector3[] linePoints = new Vector3[playerTrail.positionCount];
        playerTrail.GetPositions(linePoints);
        print(linePoints.ToString());
    }



    // Player Snapping Tech
    private void SnapPlayerOnEdges(List<GameObject> edges)
    {
        if (!isOnEdge)
            return;

        float minDist = Mathf.Infinity;
        Vector2 closestPoint = transform.position;

        foreach (GameObject edge in edges)
        {
            if (edge == null) continue;

            Vector2 pointOnEdge = GetClosestPointOnEdge(transform.position, edge.GetComponent<EdgeCollider2D>());
            float dist = Vector2.Distance(transform.position, pointOnEdge);

            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = pointOnEdge;
            }
        }

        transform.position = closestPoint;
    }

    private Vector2 GetClosestPointOnEdge(Vector2 playerPos, EdgeCollider2D edge)
    {
        float minDist = Mathf.Infinity;
        Vector2 closestPoint = Vector2.zero;


        var points = edge.points; // array of edge collider points

        // check every edge to see which is the closest to the player's position
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 p1 = edge.transform.TransformPoint(points[i]);
            Vector2 p2 = edge.transform.TransformPoint(points[i + 1]);

            Vector2 projected = Proj_ab_aplayer(playerPos, p1, p2);
            float dist = Vector2.Distance(playerPos, projected);
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = projected;
            }
        }
        return closestPoint;
    }


    private Vector2 Proj_ab_aplayer(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a; // edge we are projecting onto
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t); // ensures the player doesn't move beyond the length of the edge it is currently on
        return a + t * ab;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3) gameManager.GameOver();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isOnEdge)
        {
            switch (cutDirection)
            {
                case Directions.Left:
                    moveLeft();
                    break;
                case Directions.Right:
                    moveRight();
                    break;
                case Directions.Up:
                    moveUp();
                    break;
                case Directions.Down:
                    moveDown();
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // snapbaaaaack mechanic
        if (!isOnEdge)
        {
            isCutting = false;
            isOnEdge = true;
            if (topLine.gameObject == collision.gameObject)
            {
                currentEdge[0] = topLine.gameObject;
            }
            else if (bottomLine.gameObject == collision.gameObject)
            {
                currentEdge[0] = bottomLine.gameObject;
            }
            else if (leftLine.gameObject == collision.gameObject)
            {
                currentEdge[0] = leftLine.gameObject;
            }
            else if (rightLine.gameObject == collision.gameObject)
            {
                currentEdge[0] = rightLine.gameObject;
            }
            //addEdgeTo(currentEdge[0]);
        }

        if (currentEdge.Contains(collision.gameObject)) return;

        // Buffer the new edge, don't apply immediately
        if (collision.gameObject.layer == 7)
        {
            pendingEdge = collision.gameObject;
            //Debug.Log("Buffered edge: " + pendingEdge.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isOnEdge)
        {
            isCutting = true;
        }
        else
        {
            if (collision.gameObject == pendingEdge)
            {
                pendingEdge = null;
                //Debug.Log("Canceled buffered edge: " + collision.gameObject.name);
            }

            if (currentEdge[0] == collision.gameObject)
            {
                currentEdge[0] = currentEdge[1];
                currentEdge[1] = null;
            }
            else if (currentEdge[1] == collision.gameObject)
            {
                currentEdge[1] = null;
            }
        }
        


        //Debug.Log("Exited: " + collision.gameObject.name);
        //Debug.Log("Edge 0: " + currentEdge[0]?.name);
        //Debug.Log("Edge 1: " + currentEdge[1]?.name);
    }






    // helpers 

    private void moveUp()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, 1 * speed, 0) * Time.deltaTime;
        }
    }

    private void moveDown()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, -1 * speed, 0) * Time.deltaTime;
        }
    }

    private void moveLeft()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(-1 * speed, 0, 0) * Time.deltaTime;
        }
    }

    private void moveRight()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(1 * speed, 0, 0) * Time.deltaTime;
        }
    }


    public void playMoveSound()
    {
        if (!AudioManager.Instance.isPlaying("MovingPlayer"))
        {
            AudioManager.Instance.Play("MovingPlayer");
        }
    }



}

