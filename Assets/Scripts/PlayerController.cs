using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject leftPlayer;
    public GameObject rightPlayer;

    public float gridSize = 1f; // Size of each grid cell
    public float moveSpeed = 5f; // Speed of movement animation
    public float bumpDistance = 0.2f; // How far to move during bump animation
    public LayerMask barrierLayer = -1; // What layers count as barriers
    public float collisionRadius = 0.4f; // Radius for collision checking
    
    private bool isMoving = false; // Prevents input during movement
    
    void Update()
    {
        if (!isMoving)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Vector2 moveDirection = Vector2.zero;
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = Vector2.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = Vector2.down;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = Vector2.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = Vector2.right;
        }
        
        if (moveDirection != Vector2.zero)
        {
            // Check movement validity for each player independently
            bool rightCanMove = CanMove(rightPlayer.transform.position, moveDirection);
            bool leftCanMove = CanMove(leftPlayer.transform.position, new Vector2(-moveDirection.x, moveDirection.y));
            
            // Only move if at least one player can move
            if (rightCanMove || leftCanMove)
            {
                StartCoroutine(MoveToGridPosition(moveDirection, rightCanMove, leftCanMove));
            }
        }
    }
    
    private IEnumerator MoveToGridPosition(Vector2 direction, bool moveRight, bool moveLeft)
    {
        isMoving = true;
        
        // Calculate target positions for moving players
        Vector3 rightStartPos = rightPlayer.transform.position;
        Vector3 rightTargetPos = moveRight ? 
            rightStartPos + new Vector3(direction.x * gridSize, direction.y * gridSize, 0) : 
            rightStartPos;
        
        Vector3 leftStartPos = leftPlayer.transform.position;
        Vector3 leftDirection = new Vector3(-direction.x, direction.y, 0);
        Vector3 leftTargetPos = moveLeft ? 
            leftStartPos + leftDirection * gridSize : 
            leftStartPos;
        
        // Calculate bump positions for blocked players
        Vector3 rightBumpPos = rightStartPos + new Vector3(direction.x * bumpDistance, direction.y * bumpDistance, 0);
        Vector3 leftBumpPos = leftStartPos + leftDirection * bumpDistance;
        
        float moveTime = 0f;
        float totalMoveTime = 1f / moveSpeed;
        float halfMoveTime = totalMoveTime * 0.5f;
        
        // Animate movement
        while (moveTime < totalMoveTime)
        {
            moveTime += Time.deltaTime;
            float t = moveTime / totalMoveTime;
            
            // Right player animation
            if (moveRight)
            {
                // Normal smooth movement
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                rightPlayer.transform.position = Vector3.Lerp(rightStartPos, rightTargetPos, smoothT);
            }
            else
            {
                // Bump animation: move forward then back
                if (moveTime < halfMoveTime)
                {
                    float bumpT = (moveTime / halfMoveTime);
                    rightPlayer.transform.position = Vector3.Lerp(rightStartPos, rightBumpPos, bumpT);
                }
                else
                {
                    float backT = (moveTime - halfMoveTime) / halfMoveTime;
                    rightPlayer.transform.position = Vector3.Lerp(rightBumpPos, rightStartPos, backT);
                }
            }
            
            // Left player animation
            if (moveLeft)
            {
                // Normal smooth movement
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                leftPlayer.transform.position = Vector3.Lerp(leftStartPos, leftTargetPos, smoothT);
            }
            else
            {
                // Bump animation: move forward then back
                if (moveTime < halfMoveTime)
                {
                    float bumpT = (moveTime / halfMoveTime);
                    leftPlayer.transform.position = Vector3.Lerp(leftStartPos, leftBumpPos, bumpT);
                }
                else
                {
                    float backT = (moveTime - halfMoveTime) / halfMoveTime;
                    leftPlayer.transform.position = Vector3.Lerp(leftBumpPos, leftStartPos, backT);
                }
            }
            
            yield return null;
        }
        
        // Ensure exact final positions
        rightPlayer.transform.position = moveRight ? rightTargetPos : rightStartPos;
        leftPlayer.transform.position = moveLeft ? leftTargetPos : leftStartPos;
        
        isMoving = false;
    }
    
    private bool CanMove(Vector3 currentPosition, Vector2 direction)
    {
        // Calculate target position
        Vector3 targetPosition = currentPosition + new Vector3(direction.x * gridSize, direction.y * gridSize, 0);
        
        // Check for collisions at target position using OverlapCircle
        Collider2D hit = Physics2D.OverlapCircle(targetPosition, collisionRadius, barrierLayer);
        
        // Return true if no collision detected
        return hit == null;
    }
}