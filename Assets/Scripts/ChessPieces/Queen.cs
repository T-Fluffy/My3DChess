
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] myBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        #region Rook movements :
        // Move Down :
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (myBoard[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            if (myBoard[currentX, i] != null)
            {
                if (myBoard[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }
        // Move Up :
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (myBoard[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            if (myBoard[currentX, i] != null)
            {
                if (myBoard[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }
        // Move Left :
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (myBoard[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            if (myBoard[i, currentY] != null)
            {
                if (myBoard[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }
        }
        // Move Right :
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (myBoard[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            if (myBoard[i, currentY] != null)
            {
                if (myBoard[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }
        }
        #endregion
        #region Bishop movements:
        // Top Right Move :
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }
        // Top Left Move :
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }
        // Bottom Right Move :
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }
        // Bottom Left Move :
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }
        #endregion

        return r;
    }
}