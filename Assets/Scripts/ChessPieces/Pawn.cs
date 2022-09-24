using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] myBoard,int tilecountX,int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        // If its team white, direction will be moving up or down if its black :
        int direction = (team == 0) ? 1 : -1;
        // Move piece 1 square in front :
        if (myBoard[currentX , currentY+direction]==null)
            r.Add(new Vector2Int(currentX, currentY + direction));
        // Move piece 2 squares in front :
        if (myBoard[currentX, currentY + direction] == null)
        {
            // White team move :
            if (team==0 && currentY == 1 && myBoard[currentX,currentY+(direction*2)]==null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            // Black team move :
            if (team==1 && currentY == 6 && myBoard[currentX,currentY+(direction*2)]==null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            // Kill piece move :
            if (currentX != tilecountX-1)
                if (myBoard[currentX + 1,currentY + direction] != null && myBoard[currentX + 1, currentY + direction].team != team)
                   r.Add(new Vector2Int(currentX + 1, currentY + direction));
            if (currentX != 0)
                if (myBoard[currentX - 1,currentY + direction] != null && myBoard[currentX - 1, currentY + direction].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY + direction));
        }
        return r;
    }
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] myBoard, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        //Promotion Move :
        if ((team==0 && currentY == 6)||(team==1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }
        // En passant move :
        if (moveList.Count>0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if (myBoard[lastMove[1].x,lastMove[1].y].type == ChessPieceType.Pawn) // If the last peice moved was a pawn
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y)==2) // If the last move was a +2 in either direction
                {
                    if (myBoard[lastMove[1].x,lastMove[1].y].team != team) //If the move was from the over team 
                    {
                        if (lastMove[1].y == currentY) // If both pawns are on the same Y
                        {
                            if (lastMove[1].x == currentX - 1) // Landed left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1) // Landed right 
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}
