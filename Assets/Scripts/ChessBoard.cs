using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public enum SpecialMove
{
    None =0,
    EnPassant,
    Castling,
    Promotion
}
public class ChessBoard : MonoBehaviour
{
    #region Global_Variables :
    [Header("Art Stuff :")]
    [SerializeField] private Material tileMeterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [Header("Prefabs & Materials :")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1f;
    [SerializeField] private GameObject VictoryScreen;
    [SerializeField] private Transform rematchIndicator;
    [SerializeField] private Button rematchButton;
    // Logic 
    private ChessPiece currentlyDragging;
    private List<ChessPiece> deadWhite = new List<ChessPiece>();
    private List<ChessPiece> deadBlack = new List<ChessPiece>();
    private List<Vector2Int> AvailableMoves = new List<Vector2Int>();
    private ChessPiece[,] chessPiecesPlacement;
    private const int Tile_Count_X = 8;
    private const int Tile_Count_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    // Multiplayer Logic :
    private int playerCount = -1;
    private int currentTeam = -1;
    private bool localGame { get; set; } = true;
    private bool[] playerRematch = new bool[2];
    #endregion
    private void Start()
    {
        isWhiteTurn = true;
        GenerateAllTiles(tileSize,Tile_Count_X,Tile_Count_Y);
        SpawnAllPieces();
        PositionAllPieces();
        // Start Listening to events :
        RegisterEvents();
    }
    private void Update()
    {

        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover","Highlight")))
        {
            //Get the indexes of tile we hit
            Vector2Int hitPosition = LookUpTileIndex(info.transform.gameObject);

            //If we are hovering any tile after not hovering any tile
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //if we were already hovernig a tile, change prewius
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref AvailableMoves, currentHover)) ?
                                                              LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[currentHover.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            // If we press down on the mouse :
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPiecesPlacement[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn ?
                    if ((chessPiecesPlacement[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn)||(chessPiecesPlacement[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn && currentTeam == 1))
                    {
                        currentlyDragging = chessPiecesPlacement[hitPosition.x, hitPosition.y];
                        // Get a list of where i can go and highlight tiles as well :
                        AvailableMoves = currentlyDragging.GetAvailableMoves(ref chessPiecesPlacement, Tile_Count_X, Tile_Count_Y);
                        
                        // Get a list of special moves as well :
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPiecesPlacement, ref moveList, ref AvailableMoves);
                        PreventCheck();
                        HighLightTiles();
                    }
                }
            }
            // If we are releasing the mouse button :
            if ( currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                if (ContainsValidMove(ref AvailableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                {
                    //Check if the move is valide :
                    MoveTo(previousPosition.x,previousPosition.y, hitPosition.x, hitPosition.y);
                    // Net implementation : (sending the move informtion to the server)
                    NetMakeMove mm = new NetMakeMove();
                    mm.originalX = previousPosition.x;
                    mm.originalY = previousPosition.y;
                    mm.destinationX = hitPosition.x;
                    mm.destinationY = hitPosition.y;
                    mm.teamID = currentTeam;
                    Client.Instance.SendToServer(mm);
                }
                else
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                    // Removing the hilighting of the possible moves of the latest selected piece;
                    DeleteHighLightTiles();
                }
               
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref AvailableMoves,currentHover))? 
                                                              LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if (currentlyDragging&&Input.GetMouseButtonUp(0))
            {
                // juste to make the pieces not going outside the board if clic_holded outside :
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                // Removing the hilighting of the possible moves of the latest selected piece;
                DeleteHighLightTiles();
            }
        }
        // if we're dragging a piece 
        if (currentlyDragging)
        {
            Plane horizentalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizentalPlane.Raycast(ray,out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance)+ Vector3.up * dragOffset);
            }
        }
    }

    // Generate Game Board :
    private void GenerateAllTiles(float tileSize,int tileCountX,int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize)+boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize,x,y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format($"X:{x}, Y:{y}"));
        tileObject.transform.parent = transform;

        //Adding a Mesh for the 3D tile object :
        Mesh TileMesh = new Mesh();
        // Adding a MeshFilter to our tileObject that will be the form of the tile : 
        tileObject.AddComponent<MeshFilter>().mesh=TileMesh;
        // Adding a MeshRender to our tileObject to bake or render our material on the GameObject :
        tileObject.AddComponent<MeshRenderer>().material=tileMeterial;

        /* 
         * Creating the Vertices of our TileObject :
         *  - Generally, a Vertice is an edge or a vector constructing a 3D object.
         *  - For our tileObject, we will be declaring 4 vertices, cuz our tile in question is square :) 
         */
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize)-bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        // Drowing 2 triangles with our vertices that will make the shape of the square tileObject :
        int[] triangles = new int[] { 0,1,2, // First triangle.
                                      1,3,2  // Second triangle.
        };
        TileMesh.vertices = vertices;
        TileMesh.triangles = triangles;
        TileMesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile"); 
        // Adding a collider to the tileObject :
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawn all the Pieces :
    private void SpawnAllPieces()
    {
        chessPiecesPlacement = new ChessPiece[Tile_Count_X, Tile_Count_Y];
        int whiteTeam = 0, blackTeam = 1;
        //White Team Pieces :
            chessPiecesPlacement[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
            chessPiecesPlacement[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
            chessPiecesPlacement[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
            chessPiecesPlacement[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
            chessPiecesPlacement[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
            chessPiecesPlacement[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
            chessPiecesPlacement[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
            chessPiecesPlacement[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
         //Spawning the Pawns in a sigle ligne : 
         for (int i = 0; i < Tile_Count_X; i++)
            chessPiecesPlacement[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
         //Black Team Pieces :
        chessPiecesPlacement[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPiecesPlacement[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPiecesPlacement[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPiecesPlacement[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPiecesPlacement[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPiecesPlacement[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPiecesPlacement[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPiecesPlacement[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        //Spawning the Pawns in a sigle ligne : 
        for (int i = 0; i < Tile_Count_X; i++)
            chessPiecesPlacement[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
    }

    // Spawning a single piece :
    private ChessPiece SpawnSinglePiece(ChessPieceType type,int team)
    {
        // Spawning the Piece :
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        // Adding Material to the pieces for the 2 teams :
        //cp.GetComponent<MeshRenderer>().material = teamMaterials[team]; (for jut 2 type materials white and black)
        // or using more materials :
        cp.GetComponent<MeshRenderer>().material = teamMaterials[((team == 0) ? 0 : 6) + ((int)type - 1)];
        return cp;
    }

    // Positioning :
    private void PositionAllPieces()
    {
        for (int x = 0; x < Tile_Count_X; x++)
        {
            for (int y = 0; y < Tile_Count_Y; y++)
            {
                if (chessPiecesPlacement[x,y] != null)
                {
                    PositionSinglePiece(x,y,true);
                }
            }
        }
    }

    // HighLight Tiles :
    private void HighLightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
        {
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void DeleteHighLightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
        {
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        AvailableMoves.Clear();
    }
    private void PositionSinglePiece(int x,int y, bool force=false)
    {
        chessPiecesPlacement[x, y].currentX = x;
        chessPiecesPlacement[x, y].currentY = y;
        chessPiecesPlacement[x, y].SetPosition(GetTileCenter(x, y),force);
    }

    // Getting the right positions of the pieces in the board after recalculating the Spawning position :
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    // CheckMate :
    private void CheckMate(int Team)
    {
        DisplayVictory(Team);
    }
    // Special Moves :
    private void ProcessSpecialMove()
    {
        //Enpassant:
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece mypawn = chessPiecesPlacement[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPiecesPlacement[targetPawnPosition[1].x, targetPawnPosition[1].y];
            if (mypawn.currentX == enemyPawn.currentX)
            {
                if (mypawn.currentY == enemyPawn.currentY - 1 || mypawn.currentY == enemyPawn.currentY + 1)
                {
                    if (enemyPawn.team == 0)
                    {
                        deadWhite.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);

                        // Moving the dead white pieces to the right side of the board :
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhite.Count);
                    }
                    else
                    {
                        deadBlack.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);

                        // Moving the dead white pieces to the right side of the board :
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadBlack.Count);
                    }
                    chessPiecesPlacement[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }
        //Promotion :
        if (specialMove==SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPiecesPlacement[lastMove[1].x, lastMove[1].y];
            //White team :
            if (targetPawn.team == 0 && lastMove[1].y == 7)
            {
                ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen,0);
                // smoothing the spawn animation of the queen with the positionning:
                newQueen.transform.position = chessPiecesPlacement[lastMove[1].x, lastMove[1].y].transform.position;
                Destroy(chessPiecesPlacement[lastMove[1].x,lastMove[1].y].gameObject);
                chessPiecesPlacement[lastMove[1].x, lastMove[1].y] = newQueen;
                PositionSinglePiece(lastMove[1].x, lastMove[1].y);
            }
            //black team:
            if (targetPawn.team == 1 && lastMove[1].y == 0)
            {
                ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                // smoothing the spawn animation of the queen with the positionning:
                newQueen.transform.position = chessPiecesPlacement[lastMove[1].x, lastMove[1].y].transform.position;
                Destroy(chessPiecesPlacement[lastMove[1].x, lastMove[1].y].gameObject);
                chessPiecesPlacement[lastMove[1].x, lastMove[1].y] = newQueen;
                PositionSinglePiece(lastMove[1].x, lastMove[1].y);
            }
        }
        //Castling :
        if (specialMove==SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            //Left rook :
            if (lastMove[1].x == 2 )
            {
                if (lastMove[1].y == 0)// white side 
                {
                    ChessPiece rook = chessPiecesPlacement[0, 0];
                    chessPiecesPlacement[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPiecesPlacement[0, 0] = null;
                }
                else if (lastMove[1].y == 7)// Black side
                {
                    ChessPiece rook = chessPiecesPlacement[0, 7];
                    chessPiecesPlacement[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPiecesPlacement[0, 7] = null;
                }
            }
            //Right rook :
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0)// white side 
                {
                    ChessPiece rook = chessPiecesPlacement[7, 0];
                    chessPiecesPlacement[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPiecesPlacement[7, 0] = null;
                }
                else if (lastMove[1].y == 7)// Black side
                {
                    ChessPiece rook = chessPiecesPlacement[7, 7];
                    chessPiecesPlacement[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPiecesPlacement[7, 7] = null;
                }
            }
        }
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < Tile_Count_X; x++)
            for (int y = 0; y < Tile_Count_Y; y++)
                if (chessPiecesPlacement[x, y]!= null)
                    if (chessPiecesPlacement[x, y].type == ChessPieceType.King)
                    if (chessPiecesPlacement[x, y].team == currentlyDragging.team)
                        targetKing = chessPiecesPlacement[x, y];
        // Since we're sending ref availableMoves, we will be deleting moves that are putting us in check :
        SimulateMoveSinglePiece(currentlyDragging,ref AvailableMoves,targetKing);
    }
    private void SimulateMoveSinglePiece(ChessPiece cp,ref List<Vector2Int> moves,ChessPiece targetKing)
    {
        // Save the current values, to reset after the function call :
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going throwgh all the moves, simulate them and check if we are in Check :
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;
            Vector2Int KingPositionThisSim = new Vector2Int(targetKing.currentX,targetKing.currentY);
            // Did we simulate the king's move :
            if (cp.type == ChessPieceType.King)
                KingPositionThisSim = new Vector2Int(simX, simY);

            //Copy the [,] and not a reference 
            ChessPiece[,] simulation = new ChessPiece[Tile_Count_X, Tile_Count_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < Tile_Count_X; x++)
            {
                for (int y = 0; y < Tile_Count_Y; y++)
                {
                    if (chessPiecesPlacement[x,y] != null)
                    {
                        simulation[x, y] = chessPiecesPlacement[x, y];
                        if (simulation[x,y].team != cp.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }
            // Simulate the move : (hardcoded  manually)
            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            // Did one of the piece got taken down during  our simulation :
            var deadPiece = simAttackingPieces.Find(piece => piece.currentX == simX && piece.currentY == simY);
            if (deadPiece != null)
                simAttackingPieces.Remove(deadPiece);
            // Get all the simulated attacking pieces moves :
            List<Vector2Int> simuMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMove = simAttackingPieces[a].GetAvailableMoves(ref simulation, Tile_Count_X, Tile_Count_Y);
                for (int b = 0; b < pieceMove.Count; b++)
                {
                    simuMoves.Add(pieceMove[b]);
                }
            }
            // Is the king in trouble? if so, remove the move :
            if (ContainsValidMove(ref simuMoves, KingPositionThisSim)) 
            {
                movesToRemove.Add(moves[i]);
            }
            // Restore the actual piece data
            cp.currentX = actualX;
            cp.currentY = actualY;

        }
        // Remove from the current available move List
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }
    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPiecesPlacement[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < Tile_Count_X; x++)
            for (int y = 0; y < Tile_Count_Y; y++)
                if (chessPiecesPlacement[x, y] != null)
                {
                   if (chessPiecesPlacement[x, y].team == targetTeam)
                   {
                        defendingPieces.Add(chessPiecesPlacement[x, y]);
                        if (chessPiecesPlacement[x,y].type==ChessPieceType.King)
                        {
                            targetKing = chessPiecesPlacement[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(chessPiecesPlacement[x, y]);
                    }
                }
        //Is the king attacked right now :
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPiecesPlacement, Tile_Count_X, Tile_Count_Y);
            for (int b = 0; b < pieceMoves.Count; b++)
              currentAvailableMoves.Add(pieceMoves[b]);
            
        }
        // Are we in check right now?
        if (ContainsValidMove(ref currentAvailableMoves,new Vector2Int(targetKing.currentX,targetKing.currentY)))
        {
           // king is under attack, can we move something to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPiecesPlacement, Tile_Count_X,Tile_Count_Y);
                SimulateMoveSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (defendingMoves.Count!=0)
                {
                    return false;
                }
                return true;
            }
        }
        return false;
    }
    // UI STUFF :
    private void DisplayVictory(int winningTeam)
    {
        VictoryScreen.SetActive(true);
        VictoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    public void OnRematchButton()
    {
        Debug.Log("cliccked !");
        if (localGame)
        {
            // Both players wante a rematch message:
            NetRematch wrm = new NetRematch();
            wrm.teamID = 0;
            wrm.wanteRematch = 1;
            Client.Instance.SendToServer(wrm);

            NetRematch brm = new NetRematch();
            brm.teamID = 1;
            brm.wanteRematch = 1;
            Client.Instance.SendToServer(brm);
        }
        else
        {
            NetRematch rm = new NetRematch();
            rm.teamID = currentTeam;
            rm.wanteRematch = 1;
            Client.Instance.SendToServer(rm);
        }
    }
    public void GameReset()
    {
        // UI
        rematchButton.interactable = true;

        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        VictoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        VictoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        VictoryScreen.SetActive(false);

        //fields reset
        currentlyDragging = null;
        AvailableMoves.Clear();
        moveList.Clear();

        playerRematch[0] = playerRematch[1] = false;

        //CleanUp the pieces :
        for (int x = 0; x < Tile_Count_X; x++)
        {
            for (int y = 0; y < Tile_Count_Y; y++)
            {
                if (chessPiecesPlacement[x, y] != null)
                {
                    Destroy(chessPiecesPlacement[x, y].gameObject);
                }
                chessPiecesPlacement[x, y] = null;
            }
        }
        //CleanUp Dead White Pieces :
        for (int i = 0; i < deadWhite.Count; i++)
            Destroy(deadWhite[i].gameObject);
        //CleanUp Dead Black Pieces :
        for (int i = 0; i < deadBlack.Count; i++)
            Destroy(deadBlack[i].gameObject);

        deadWhite.Clear();
        deadBlack.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }
    public void OnMenuButton()
    {
        NetRematch rm = new NetRematch();
        rm.teamID = currentTeam;
        rm.wanteRematch = 0;
        Client.Instance.SendToServer(rm);
        
        GameReset();
        GameUI.Instance.OnLeaveFromGameMenu();

        Invoke("ShutdownRelay", 1.0f);

        //Reset some values :
        playerCount = -1;
        currentTeam = -1;
    }
    // Operations 
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x==pos.x && moves[i].y == pos.y)
            {
                return true;
            }
            
        }
        return false;
    }
    private Vector2Int LookUpTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < Tile_Count_X; x++)
        {
            for (int y = 0; y < Tile_Count_Y; y++)
            {
                if (tiles[x,y]==hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one;
    }
    private void MoveTo(int originalX,int originalY, int x, int y)
    {
        ChessPiece cp = chessPiecesPlacement[originalX, originalY];
        Vector2Int previousPosition = new Vector2Int(originalX, originalY);
        // Check if there is another piece on the target position :
        if (chessPiecesPlacement[x,y] != null)
        {
            ChessPiece ocp = chessPiecesPlacement[x, y];
            if (cp.team==ocp.team)
            {
                return;
            }
            // what if it is the enemy team ? :
            if (ocp.team==0)
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(1);
                }
                deadWhite.Add(ocp);
                ocp.SetScale(Vector3.one*deathSize);

                // Moving the dead white pieces to the right side of the board :
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhite.Count);
                
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(0);
                }
                deadBlack.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);

                // Moving the dead Black pieces to the left side of the board :
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlack.Count);
            }
        }
        chessPiecesPlacement[x, y] = cp;
        chessPiecesPlacement[previousPosition.x, previousPosition.y] = null;
        // Move the piece to its new position (her right position) :
        PositionSinglePiece(x, y);

        // end turn white :
        isWhiteTurn = !isWhiteTurn;
        // Setting up the team in lovcal Game mode :
        if (localGame)
        {
            currentTeam = (currentTeam == 0) ? 1 : 0;
        }
        // list for the special chess moves :
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x,y)});

        ProcessSpecialMove();

        if(currentlyDragging) currentlyDragging = null;

        // Removing the hilighting of the possible moves of the latest selected piece;
        DeleteHighLightTiles();

        if (CheckForCheckmate())
        {
            CheckMate(cp.team);
        }
        return;
    }
    #region GameEvents listeners 
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;

        NetUtility.S_REMATCH += OnMakeRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;

        NetUtility.C_REMATCH += OnMakeRematchClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    } 
    private void UnregisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;

        NetUtility.S_REMATCH -= OnMakeRematchServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;

        NetUtility.C_REMATCH -= OnMakeRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }
    // Server Reception:
    private void OnWelcomeServer(NetMessage msg,NetworkConnection connec)
    {
        // Client has connected, assign a team and return the msg back to him :
        NetWelcome newMsg = msg as NetWelcome;
        // Assign a team :
        newMsg.AssignedTeam =++playerCount;
        // Return back to the client :
        Server.Instance.SendToClient(connec,newMsg);
        // If full, start a game :
        if (playerCount==1)
        {
            Server.Instance.BroadCast(new NetStartGame());
        }
    }
    private void OnMakeMoveServer(NetMessage msg, NetworkConnection connec)
    {
        // Receive the message and broadcast it back :
        NetMakeMove mm = msg as NetMakeMove;

        //This where you could do some validation check (for security stuff)
        //--
        if (mm.teamID != currentTeam)
        {

        }
        //Receive and just broadcast it back :
        Server.Instance.BroadCast(mm);        
    }
    private void OnMakeRematchServer(NetMessage msg, NetworkConnection connec)
    {
        //Receive and just broadcast it back :
        Server.Instance.BroadCast(msg);        
    }
    // Client Reception :
    private void OnWelcomeClient(NetMessage msg)
    {
        // Recieve the connection message :
        NetWelcome newMsg = msg as NetWelcome;
        // Assign the team :
        currentTeam = newMsg.AssignedTeam;
        // Test the code : 
        Debug.Log($"My assigned team is : {newMsg.AssignedTeam}");
        if (localGame && currentTeam==0)
        {
            Server.Instance.BroadCast(new NetStartGame());
        }
    }
    private void OnStartGameClient(NetMessage msg)
    {
        // We just need to change the camera :
        GameUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.WhiteTeam : CameraAngle.BlackTeam);
    }
    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;
        Debug.Log($"Mouvement :{mm.teamID} : {mm.originalX}-{mm.originalY} -> {mm.destinationX}-{mm.destinationY}");
        if (mm.teamID != currentTeam)
        {
            ChessPiece target = chessPiecesPlacement[mm.originalX, mm.originalY];
            AvailableMoves = target.GetAvailableMoves(ref chessPiecesPlacement, Tile_Count_X, Tile_Count_Y);
            specialMove = target.GetSpecialMoves(ref chessPiecesPlacement, ref moveList, ref AvailableMoves);
            MoveTo(mm.originalX, mm.originalY, mm.destinationX, mm.destinationY);
        }
    }
    private void OnMakeRematchClient(NetMessage msg)
    {
        // Receive the connection message :
        NetRematch rm = msg as NetRematch;
        // Set the boolean rematch :
        playerRematch[rm.teamID] = rm.wanteRematch == 1;
        // Activate the pieceof UI
        if (rm.teamID != currentTeam)
        {
            rematchIndicator.transform.GetChild((rm.wanteRematch == 1) ? 0 : 1).gameObject.SetActive(true);
            if (rm.wanteRematch != 1)
            {
                rematchButton.interactable = false;
            }
        }
        // If both wants to rematch 
        if (playerRematch[0] && playerRematch[1])
        {
            GameReset();
        }
    }
    
    private void ShutdownRelay()
    {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
    }
    private void OnSetLocalGame(bool v)
    {
        playerCount = -1;
        currentTeam = -1;
        localGame = v;
    }
    #endregion
}
