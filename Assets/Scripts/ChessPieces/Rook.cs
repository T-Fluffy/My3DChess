using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] myBoard,int tileCountX,  int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        // Move Down :
        for (int i = currentY - 1; i >=0 ; i--)
        {
            if (myBoard[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            if (myBoard[currentX,i] != null)
            {
                if (myBoard[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX,i));
                break;
            }
        }
        // Move Up :
        for (int i = currentY + 1; i < tileCountY ; i++)
        {
            if (myBoard[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            if (myBoard[currentX,i] != null)
            {
                if (myBoard[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX,i));
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
        return r;
    }
}
