using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
   public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] myBoard, int tileCountX,int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        // Top Right move :
        int x = currentX + 1;
        int y = currentY + 2;
        if (x < tileCountX && y < tileCountY)
        {
            if (myBoard[x,y] == null || myBoard[x,y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
        {
            if (myBoard[x,y] == null || myBoard[x,y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        // Top Left Move :
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0 && y < tileCountY)
            if (myBoard[x, y] == null || myBoard[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        
        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (myBoard[x, y] == null || myBoard[x, y].team != team)
                r.Add(new Vector2Int(x, y));

        //Bottom Right Move :
        x = currentX + 1;
        y = currentY - 2;
        if (x <tileCountX && y >=0)
            if (myBoard[x, y] == null || myBoard[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        
        x = currentX + 2;
        y = currentY - 1;
        if (x <tileCountX && y >=0)
            if (myBoard[x, y] == null || myBoard[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        
        //Bottom Left Move :
        x = currentX - 1;
        y = currentY - 2;
        if (x >=0 && y >=0)
            if (myBoard[x, y] == null || myBoard[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        
        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >=0)
            if (myBoard[x, y] == null || myBoard[x, y].team != team)
                r.Add(new Vector2Int(x, y));
        return r;
    }
}
