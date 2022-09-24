using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] myBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Top Right Move :
        for (int x = currentX + 1, y=currentY + 1; x < tileCountX && y < tileCountY; x++,y++)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                   r.Add(new Vector2Int(x,y));
                break;
            }
        }
        // Top Left Move :
        for (int x = currentX - 1, y=currentY + 1; x >=0 && y < tileCountY; x--,y++)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                   r.Add(new Vector2Int(x,y));
                break;
            }
        }
        // Bottom Right Move :
        for (int x = currentX + 1, y=currentY - 1; x < tileCountX  && y >=0; x++,y--)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                   r.Add(new Vector2Int(x,y));
                break;
            }
        }
        // Bottom Left Move :
        for (int x = currentX - 1, y=currentY - 1; x >=0  && y >=0; x--,y--)
        {
            if (myBoard[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (myBoard[x, y].team != team)
                   r.Add(new Vector2Int(x,y));
                break;
            }
        }

        return r;
    }
}
