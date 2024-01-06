using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] myBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Right move:
        if (currentX + 1 < tileCountX)
        {
            // Right :
            if (myBoard[currentX + 1, currentY] == null)
                r.Add(new Vector2Int(currentX + 1, currentY));
            else if(myBoard[currentX + 1,currentY].team != team)
                r.Add(new Vector2Int(currentX + 1, currentY));
            // Top Right:
            if (currentY + 1 < tileCountY)
                if (myBoard[currentX + 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
                else if (myBoard[currentX + 1, currentY + 1].team != team)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
            // Bottom right :
            if (currentY - 1 >=0)
                if (myBoard[currentX + 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
                else if (myBoard[currentX + 1, currentY - 1].team != team)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
        }
        // Left move :
        if (currentX - 1 >=0)
        {
            // Left :
            if (myBoard[currentX - 1, currentY] == null)
                r.Add(new Vector2Int(currentX - 1, currentY));
            else if (myBoard[currentX - 1, currentY].team != team)
                r.Add(new Vector2Int(currentX - 1, currentY));
            // Top Right:
            if (currentY + 1 < tileCountY)
                if (myBoard[currentX - 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
                else if (myBoard[currentX - 1, currentY + 1].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
            // Bottom Right :
            if (currentY - 1 >= 0)
                if (myBoard[currentX - 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
                else if (myBoard[currentX - 1, currentY - 1].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
        }
        // Up move :
        if (currentY + 1 < tileCountY)
            if (myBoard[currentX, currentY + 1] == null || myBoard[currentX, currentY + 1].team != team)
                r.Add(new Vector2Int(currentX, currentY + 1));
        // Down move :
        if (currentY - 1 >=0)
            if (myBoard[currentX, currentY - 1] == null || myBoard[currentX, currentY - 1].team != team)
                r.Add(new Vector2Int(currentX, currentY - 1));
        return r;
    }
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] myBoard, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove r = SpecialMove.None;
        // Find the piueces to move :
        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));
        if (kingMove == null && currentX == 4)
        {
            // White team :
            if (team == 0)
            {
                //LeftRook :
                if (leftRook == null)
                    if (myBoard[0, 0].type == ChessPieceType.Rook)
                        if (myBoard[0, 0].team == 0)
                            if (myBoard[3, 0] == null)
                                if (myBoard[2, 0] == null)
                                    if (myBoard[1, 0] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }
                //RightRook :
                if (rightRook == null)
                    if (myBoard[7, 0].type == ChessPieceType.Rook)
                        if (myBoard[7, 0].team == 0)
                            if (myBoard[5, 0] == null)
                                if (myBoard[6, 0] == null)
                                {
                                        availableMoves.Add(new Vector2Int(6,0));
                                        r = SpecialMove.Castling;
                                }

            }
            else
            {
                //LeftRook :
                if (leftRook == null)
                    if (myBoard[0, 7].type == ChessPieceType.Rook)
                        if (myBoard[0, 7].team == 1)
                            if (myBoard[3, 7] == null)
                                if (myBoard[2, 7] == null)
                                    if (myBoard[1, 7] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }
                //RightRook :
                if (rightRook == null)
                    if (myBoard[7, 7].type == ChessPieceType.Rook)
                        if (myBoard[7, 7].team == 1)
                            if (myBoard[5, 7] == null)
                                if (myBoard[6, 7] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }
            }

        }

        return r;
    }
}