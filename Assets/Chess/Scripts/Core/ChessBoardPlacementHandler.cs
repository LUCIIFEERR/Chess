using System;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class ChessBoardPlacementHandler : MonoBehaviour {
    [SerializeField] private GameObject[] _rowsArray;
    [SerializeField] private GameObject _highlightPrefab;
    [SerializeField] private GameObject _highlightRPrefab;
    [SerializeField] private GameObject PlayerPositions;
    [SerializeField] private GameObject EnemyPositions;
    private GameObject[,] _chessBoard;
    private GameObject[,] _chessBoardPositions;
    private bool firstMove = false; 
    private bool selected = false;
    private GameObject selectedPiece = null;
    private char turn = 'W';  
    private Vector3 Bkillpos = new Vector3(-3.7f,4.8f,0);
    private Vector3 Wkillpos = new Vector3(-3.75f,-4.95f,0);
    private int[,] checkposition;
    private GameObject[] allowedpieces;
    private bool checkmatecondition = false;
    private bool clicked = false;
    private int inscount=0;
    

    internal static ChessBoardPlacementHandler Instance;

    private void Awake() {
        Instance = this;
        GenerateArray();
        checkposition = new int[2,2];
    }

    private void GenerateArray() {
        _chessBoard = new GameObject[8, 8];
        _chessBoardPositions = new GameObject[8, 8];
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                _chessBoard[i, j] = _rowsArray[i].transform.GetChild(j).gameObject;
                _chessBoardPositions[i, j] = null;
                //Highlight(i, j);
            }
            //Debug.Log("nextRow");
        }
    }

    private void Update()
    {
        UpdatePosition();
        
        
            clicking();
        for(int i = 0; i<8; i++)
        {
            for(int j = 0 ; j<8; j++)
            {
                //Debug.Log("i=" + i + " j=" + j + _chessBoardPositions[i,j]);
            }
        }
    }

    private void LateUpdate() {
        
    }

private void ChangeTurn()
{
    if(turn == 'W')
    {
        turn = 'B';
    }
    else
    {
        turn = 'W';
    }
}

private void clicking()
{

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.Log("Raycasting");
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject Piece = hit.transform.gameObject;
                Debug.Log("Piece: " + Piece.name);
                if (Piece.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler script) && Piece.name[0]==turn) 
                {
                    firstMove = script.firstMove;
                    selectedPiece = Piece;
                    //Debug.Log("got script");
                    if(IsCheckMate())
                    {
                        Debug.Log("Check Mate");
                        Debug.Log("Check from position : " + checkposition[0,1] + " " + checkposition[0,0]);
                    }
                    else if(selected==false && script.killed==false)
                    {
                        selected = true;
                        //Debug.Log("Checking for possible moves");
                        PossibleMoves(selectedPiece.name, script.row, script.column);
                    }
                    else
                    {
                        selected = false;
                        ClearHighlights();
                    }
                }
                else if(Piece.name == "Highlighter(Clone)")
                {
                    MovePiece(Piece);
                    ChangeTurn();
                }
                else if(Piece.name == "HighlighterRed(Clone)")
                {
                    Kill(Piece);
                    MovePiece(Piece);
                    ChangeTurn();
                }
            }   
        }
}


private bool IsCheckMate()
{
    // First, check if the king is in check
    if(IsCheck())
    {
        // Check if ANY piece can make a move to get out of check
        if(turn=='W')
        {
            // White's turn, check if any white piece can save the king
            return !CheckIfAnyMovePossible(PlayerPositions);
        }
        else if(turn == 'B')
        {
            // Black's turn, check if any black piece can save the king
            return !CheckIfAnyMovePossible(EnemyPositions);
        }
    }
    return false; // Not in check, so not checkmate
}

private bool CheckIfAnyMovePossible(GameObject piecesParent)
{
    // Reset the flag before checking
    checkmatecondition = false;
    
    // Try each piece to see if it can make a move
    for(int i=0; i<16; i++)
    {
        GameObject piece = piecesParent.transform.GetChild(i).gameObject;
        if(piece.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler script))
        {
            if(!script.killed)
            {
                // Save the currently selected piece
                GameObject previousSelectedPiece = selectedPiece;
                
                // Temporarily select this piece to check its moves
                selectedPiece = piece;
                
                // Find possible moves for this piece
                PossibleMoves(piece.name, script.row, script.column);
                
                // If checkmatecondition was set to true, it means a valid move was found
                if(checkmatecondition)
                {
                    // Clean up and restore previous state
                    ClearHighlights();
                    selectedPiece = previousSelectedPiece;
                    return true; // Found a valid move
                }
                
                ClearHighlights();
                selectedPiece = previousSelectedPiece;
            }
        }
    }
    return false; // No valid moves found
}

private void Kill(GameObject Tile)
{
    int col = int.Parse(Tile.transform.parent.gameObject.name);
    int row = int.Parse(Tile.transform.parent.gameObject.transform.parent.gameObject.name);
    
    // Store the piece to be killed
    GameObject killedPiece = _chessBoardPositions[row, col];
    
    if(killedPiece.name[0]=='B')
    {
        Bkillpos.x = Bkillpos.x + 0.6f;
        killedPiece.transform.position = Bkillpos;
    }
    else if(killedPiece.name[0]=='W')
    {
        Wkillpos.x = Wkillpos.x + 0.6f;
        killedPiece.transform.position = Wkillpos;
    }
    
    if(killedPiece.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler script))
    {
        script.row = script.column = 8;
        script.killed = true;
        
        // Clear the position but DO NOT set selectedPiece to null
        _chessBoardPositions[row, col] = null;
    }
}
private void MovePiece(GameObject piece)
{
    // Get the coordinates from the clicked highlighter
    int col = int.Parse(piece.transform.parent.gameObject.name);
    int row = int.Parse(piece.transform.parent.gameObject.transform.parent.gameObject.name);
    
    Debug.Log("row = " + row + " col = " + col + " piece = " + selectedPiece.name);
    
    if (selectedPiece != null && selectedPiece.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler script)) 
    {
        // Validate array indices before accessing them
        if (script.row >= 0 && script.row < 8 && script.column >= 0 && script.column < 8)
        {
            // Clear the old position
            _chessBoardPositions[script.row, script.column] = null;
        }
        else
        {
            Debug.LogWarning($"Invalid source position: {script.row}, {script.column}");
        }
        
        // Save old position
        int oldRow = script.row;
        int oldCol = script.column;
        
        // Update piece position
        script.row = row;
        script.column = col;
        
        // Validate new position before updating array
        if (row >= 0 && row < 8 && col >= 0 && col < 8)
        {
            // Set the new position in the array
            _chessBoardPositions[row, col] = selectedPiece;
        }
        else
        {
            Debug.LogError($"Invalid destination position: {row}, {col}");
            // Revert position if destination is invalid
            script.row = oldRow;
            script.column = oldCol;
            return;
        }
        
        if(IsCheck())
        {
            Debug.Log("Check!!");
        }
        
        Debug.Log("updated");
        selectedPiece.transform.position = piece.transform.position;
        script.firstMove = false;
    }
    else
    {
        Debug.LogError("Selected piece is null or missing required component");
    }
    
    ClearHighlights();
    selected = false;
}

    private bool IsCheck()
    {
        GameObject King;
        if(turn=='W')
        {
            King = EnemyPositions.transform.GetChild(0).gameObject;
        }
        else 
        {
            King = PlayerPositions.transform.GetChild(0).gameObject;
        }

        if(King.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler Kingscript))
        {
            //Debug.Log("is check?? " + CheckforKing(Kingscript.row, Kingscript.column) + " at " + Kingscript.row + "," + Kingscript.column);
            if(!CheckforKing(Kingscript.row, Kingscript.column))
            {
                return true;
            }

        }

        //Debug.Log(King.name);

        return false;
    }

    private void UpdatePosition()
    {
        for(int i=0; i<16; i++)
        {
            GameObject PlayerPiece = PlayerPositions.transform.GetChild(i).gameObject;
            GameObject EnemyPiece = EnemyPositions.transform.GetChild(i).gameObject;

            if(PlayerPiece.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler script1))
            {
                int row1 = script1.row;
                int col1 = script1.column;
                if(row1<8 && col1<8)
                    _chessBoardPositions[row1, col1] = PlayerPiece;
            }

            if(EnemyPiece.TryGetComponent(out Chess.Scripts.Core.ChessPlayerPlacementHandler script2))
            {
                int row1 = script2.row;
                int col1 = script2.column;
                if(row1<8 && col1<8)
                    _chessBoardPositions[row1, col1] = EnemyPiece;
            }

        }
    }

private bool savingKing(int row1, int col1, int row2, int col2)
{
    GameObject tempobj;
    _chessBoardPositions[row1,col1] = null;
    tempobj=_chessBoardPositions[row2,col2];
    _chessBoardPositions[row2,col2] = selectedPiece;
    if(IsCheck())
    {
        _chessBoardPositions[row1,col1] = selectedPiece;
        _chessBoardPositions[row2,col2] = tempobj;
        return false;
    }
    else
    {
        _chessBoardPositions[row1,col1] = selectedPiece;
        _chessBoardPositions[row2,col2] = tempobj;
        return true;
    }

}
    private void CheckEnemyforPawnOnly(int row, int col, int temprow, int tempcol)
    {
        //Debug.Log("Enemy " + _chessBoardPositions[row, col] + " row: " + row + " col: " + col + " turn: " + turn + " condition: "  + (_chessBoardPositions[row, col].name[0]!=turn));
        if(_chessBoardPositions[row, col].name[0]!=turn)
        {
            if(IsCheck())
            {
                if(savingKing(temprow,tempcol,row,col))
                {
                    HighlightR(row,col);
                }
            }
            else if(!IsCheck())
            {
                if(savingKing(temprow,tempcol,row,col))
                {
                    HighlightR(row,col);
                }
            }
        }
    }

    public void PossibleMoves(string name, int row, int col)
    {
        if(name=="WPawn")
        {
            //Debug.Log("Inside" + firstMove);
            if(col<7)
            {
                if(_chessBoardPositions[row-1, col+1]!=null)
                {
                    CheckEnemyforPawnOnly(row-1, col+1, row, col);
                }
            }
            if(col>0)
            {
                if(_chessBoardPositions[row-1, col-1]!=null)
                {
                    CheckEnemyforPawnOnly(row-1, col-1, row, col);
                }
            }
            if (firstMove)
            {
                //Debug.Log("Inside firstmove");
                for(int i=row-1; i>row-3; i--)
                {
                    
                    //Debug.Log("inside loop: i " + i + " row = " + row + " col = " + col);
                    //Debug.Log(_chessBoardPositions[i, col]);
                    if (_chessBoardPositions[i, col] == null)
                    {
                        Debug.Log("Inside" + IsCheck());
                        if(!IsCheck())
                        {
                            Debug.Log("About to Highlight" + savingKing(row,col,i,col));
                            if(savingKing(row,col,i,col))
                                Highlight(i, col);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,i,col))
                                Highlight(i, col);
                        }
                }
                }
            }
            else
            {
                if (_chessBoardPositions[row - 1, col] == null)
                {
                    if(IsCheck())
                    {
                        if(savingKing(row,col,row-1,col))
                            Highlight(row - 1, col);
                    }
                    else if(!IsCheck())
                    {
                        if(savingKing(row,col,row-1,col))
                            Highlight(row - 1, col);
                    }
                }
            }
        }
        
        if(name=="BPawn")
        {
            if(col<7)
            {
                if(_chessBoardPositions[row+1, col+1]!=null)
                {
                    CheckEnemyforPawnOnly(row+1, col+1, row, col);
                }
            }
            if(col>0)
            {
                if(_chessBoardPositions[row+1, col-1]!=null)
                {
                    CheckEnemyforPawnOnly(row+1, col-1, row, col);
                }
            }
            if (firstMove)
            {
                for(int i=row+1; i<row+3; i++)
                {
                    //Debug.Log("inside loop: i " + i + " row = " + row + " col = " + col);
                    //Debug.Log(_chessBoardPositions[i, col]);
                    if (_chessBoardPositions[i, col] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,i,col))
                            {
                                Highlight(i, col);
                            }
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,i,col))
                            {
                                Highlight(i, col);
                            }
                        }
                    }
                }
            }
            else
                if (_chessBoardPositions[row + 1, col] == null && !IsCheck())
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+1,col))
                            {
                                Highlight(row+1, col);
                            }
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+1,col))
                            {
                                Highlight(row+1, col);
                            }
                        }
                    }
        }

        if(name=="BRook" || name == "WRook" || name == "BQueen" || name == "WQueen")
        {
            bool above = true, below = true, left = true, right = true;
            int i=1;
            while(i<8 )
            {
                if(row+i<8)
                {
                    if(above && _chessBoardPositions[row+i, col] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+i,col))
                                Highlight(row+i,col);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+i,col))
                                Highlight(row+i,col);
                        }
                    }
                    else if(above && _chessBoardPositions[row+i,col].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+i,col))
                                HighlightR(row+i,col);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+i,col))
                                HighlightR(row+i,col);
                        }
                        above = false;
                    }
                    else{
                        above = false;
                    }
                }
                if(row-i>=0)
                {
                    if(below && _chessBoardPositions[row-i, col] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row-i,col))
                                Highlight(row-i,col);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row-i,col))
                                Highlight(row-i,col);
                        }
                    }
                    else if(below && _chessBoardPositions[row-i,col].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row-i,col))
                                HighlightR(row-i,col);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row-i,col))
                                HighlightR(row-i,col);
                        }
                        below = false;
                    }
                    else
                    {
                        below = false;
                    }
                }
                if(col+i<8)
                {
                    if(right && _chessBoardPositions[row, col+i] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row,col+i))
                                Highlight(row,col+i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row,col+i))
                                Highlight(row,col+i);
                        }
                    }
                    else if(right && _chessBoardPositions[row, col+i].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row,col+i))
                                HighlightR(row,col+i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row,col+i))
                                HighlightR(row,col+i);
                        }
                        right = false;
                    }
                    else
                    {
                        right = false;
                    }
                }
                if(col-i>=0)
                {
                    if(left && _chessBoardPositions[row, col-i] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row,col-i))
                                Highlight(row,col-i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row,col-i))
                                Highlight(row,col-i);
                        }
                    }
                    else if(left && _chessBoardPositions[row, col-i].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row,col-i))
                                HighlightR(row,col-i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row,col-i))
                                HighlightR(row,col-i);
                        }
                        left = false;
                    }
                    else
                    {
                        left = false;
                    }
                }
                i++;
            }   
            above = below = right = left = true;
            i=1;
        }

        if(name == "BBishop" || name == "WBishop" || name == "BQueen" || name == "WQueen")
        {
            bool NE = true, NW = true, SE = true, SW = true;
            int i = 1;
            while(i<8)
            {
                if(col+i<8 && row+i<8)
                {
                    if(NW && _chessBoardPositions[row+i, col+i] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+i,col+i))
                                Highlight(row+i, col+i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+i,col+i))
                                Highlight(row+i, col+i);
                        }
                    }
                    else if(NW && _chessBoardPositions[row+i, col+i].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+i,col+i))
                                HighlightR(row+i, col+i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+i,col+i))
                                HighlightR(row+i, col+i);
                        }
                        NW = false;
                    }
                    else 
                    {
                        NW = false;
                    }
                }
                if(col+i<8 && row-i>=0)
                {
                    if(SW && _chessBoardPositions[row-i, col+i] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row-i,col+i))
                                Highlight(row-i, col+i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row-i,col+i))
                                Highlight(row-i, col+i);
                        }
                    }
                    else if(SW && _chessBoardPositions[row-i, col+i].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row-i,col+i))
                                HighlightR(row-i, col+i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row-i,col+i))
                                HighlightR(row-i, col+i);
                        }
                        SW = false;
                    }
                    else
                    {
                        SW = false;
                    }
                }
                if(col-i>=0 && row+i<8)
                {
                    if(NE && _chessBoardPositions[row+i, col-i] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+i,col-i))
                                Highlight(row+i, col-i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+i,col-i))
                                Highlight(row+i, col-i);
                        }
                    }
                    else if(NE && _chessBoardPositions[row+i, col-i].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row+i,col-i))
                                HighlightR(row+i, col-i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row+i,col-i))
                                HighlightR(row+i, col-i);
                        }
                        NE = false;
                    }
                    else{
                        NE = false;
                    }
                }
                if(col-i>=0 && row-i>=0)
                {
                    if(SE && _chessBoardPositions[row-i, col-i] == null)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row-i,col-i))
                                Highlight(row-i, col-i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row-i,col-i))
                                Highlight(row-i, col-i);
                        }
                    }
                    else if(SE && _chessBoardPositions[row-i, col-i].name[0]!=turn)
                    {
                        if(IsCheck())
                        {
                            if(savingKing(row,col,row-i,col-i))
                                HighlightR(row-i, col-i);
                        }
                        else if(!IsCheck())
                        {
                            if(savingKing(row,col,row-i,col-i))
                                HighlightR(row-i, col-i);
                        }
                        SE = false;
                    }
                    else
                    {
                        SE = false;
                    }
                }
                i++;
            }
            NE = NW = SE = SW = true;
        }

        if(name == "BKing" || name == "WKing")
        {

            if(row<7)
            {
                if(CheckPosForKing(row+1,col))
                    Highlight(row+1,col);
                if(col<7)
                {
                    if(CheckPosForKing(row+1,col+1))
                        Highlight(row+1,col+1);
                    if(CheckPosForKing(row,col+1))
                        Highlight(row,col+1);
                }
                if(col>0)
                {
                    if(CheckPosForKing(row+1,col-1))
                        Highlight(row+1,col-1);
                    if(CheckPosForKing(row,col-1))
                        Highlight(row,col-1);
                }
            }
            if(row>0)
            {
                if(CheckPosForKing(row-1,col))
                    Highlight(row-1,col);
                if(col<7)
                {
                    if(CheckPosForKing(row-1,col+1))
                        Highlight(row-1,col+1);
                    if(CheckPosForKing(row,col+1))
                        Highlight(row,col+1);
                }
                if(col>0)
                {
                    if(CheckPosForKing(row-1,col-1))
                        Highlight(row-1,col-1);
                    if(CheckPosForKing(row,col-1))
                        Highlight(row,col-1);
                }
            }
        }

        if(name=="BKnight" || name == "WKnight")
        {
                //Debug.Log("First Block");
                int i = row-2, j;
                while(i<=row+2)
                {
                    //Debug.Log(i);
                    if(i>=0 && i<8)
                    {
                        j = col-1;
                        while(j<=col+1)
                        {
                            if(j>=0 && j<8)
                            {
                                if(_chessBoardPositions[i, j] == null)
                                {
                                    if(IsCheck())
                                    {
                                        if(savingKing(row,col,i,j))
                                            Highlight(i,j);
                                    }
                                    else if(!IsCheck())
                                    {
                                        if(savingKing(row,col,i,j))
                                            Highlight(i,j);
                                    }
                                }
                                else if(_chessBoardPositions[i,j].name[0]!=turn)
                                {
                                    if(IsCheck())
                                    {
                                        if(savingKing(row,col,i,j))
                                            HighlightR(i,j);
                                    }
                                    else if(!IsCheck())
                                        HighlightR(i,j);
                                }
                            }
                            j+=2;
                        }
                    }
                        i+=4;
                }

                //Debug.Log("Second Block");
                i = row-1;
                while(i<=row+1)
                {
                    if(i>=0 && i<8)    
                    {
                        j = col-2;
                        while(j<=col+2)
                        {
                            if(j>=0 && j<8)
                            {
                                if(_chessBoardPositions[i, j] == null)
                                {
                                    if(IsCheck())
                                    {
                                        if(savingKing(row,col,i,j))
                                            Highlight(i,j);
                                    }
                                    else if(!IsCheck())
                                    {
                                        if(savingKing(row,col,i,j))
                                            Highlight(i,j);
                                    }
                                }
                                else if(_chessBoardPositions[i,j].name[0]!=turn)
                                {
                                    if(IsCheck())
                                    {
                                        if(savingKing(row,col,i,j))
                                            HighlightR(i,j);
                                    }
                                    else if(!IsCheck())
                                        HighlightR(i,j);
                                }

                            }
                            j+=4;
                        }
                    }
                        i+=2;
                }
        }

    }

    private bool CheckPosForKing(int row, int col)
    {
        if(_chessBoardPositions[row,col]!=null)
        {
            if(_chessBoardPositions[row,col].name[0]!=turn)
            {
                if(CheckforKing(row,col))
                HighlightR(row,col);
                return false;
            }
            else
            return false;
        }
        else
        {

            //Debug.Log("at row:" + row + " col:" + col + " is"  + CheckforKing(row,col));
            return CheckforKing(row,col);
        }
    }

    bool CheckforKing(int row, int col)
    {
        //check for rook, bishop and queen
            bool below, above, right, left, belowRight, aboveRight, belowLeft, aboveLeft;
            below = above = right = left = belowRight = aboveRight = belowLeft = aboveLeft = true;
            for(int i = 1; i<8 ; i++)    
            {
                if(row-i>=0 && below)
                {
                    if(_chessBoardPositions[row-i,col]!=null)
                    {
                        below = false;
                        if((_chessBoardPositions[row-i,col].name[1]=='R' || _chessBoardPositions[row-i,col].name[1]=='Q') && _chessBoardPositions[row-i,col].name[0]!=turn)
                        {
                            checkposition[0,0]=col;
                            checkposition[0,1]=row - i;
                            return false;
                        }
                    }
                }
                if(row+i<8 && above)
                {
                    if(_chessBoardPositions[row+i,col]!=null)
                    {
                        above = false;
                        if((_chessBoardPositions[row+i,col].name[1]=='R' || _chessBoardPositions[row+i,col].name[1]=='Q') && _chessBoardPositions[row+i,col].name[0]!=turn)
                        {
                            checkposition[0,0]=col;
                            checkposition[0,1]=row + i;
                            return false;
                        }
                    }
                }
                if(col+i<8 && right)
                {
                    if(_chessBoardPositions[row,col+i]!=null)
                    {
                        right = false;
                        if((_chessBoardPositions[row,col+i].name[1]=='R' || _chessBoardPositions[row,col+i].name[1]=='Q') && _chessBoardPositions[row,col+i].name[0]!=turn)
                        {
                            checkposition[0,0]=col +i;
                            checkposition[0,1]=row;
                            return false;
                        }
                    }
                }
                if(col-i>=0 && left)
                {
                    if(_chessBoardPositions[row,col-i]!=null)
                    {
                        left = false;
                        if((_chessBoardPositions[row,col-i].name[1]=='R' || _chessBoardPositions[row,col-i].name[1]=='Q') && _chessBoardPositions[row,col-i].name[0]!=turn)
                        {
                            checkposition[0,0]=col - i;
                            checkposition[0,1]=row;
                            return false;
                        }
                    }
                }
                if(row+i<8 && col+i<8 && aboveRight)
                {
                    if(_chessBoardPositions[row+i,col+i]!=null)
                    {
                        aboveRight = false;
                        if((_chessBoardPositions[row+i,col+i].name[1]=='B' || _chessBoardPositions[row+i,col+i].name[1]=='Q') && _chessBoardPositions[row+i,col+i].name[0]!=turn)
                        {
                            checkposition[0,0]=col + i;
                            checkposition[0,1]=row + i;
                            return false;
                        }
                    }
                }
                if(row-i>=0 && col+i<8 && belowRight)
                {
                    if(_chessBoardPositions[row-i,col+i]!=null)
                    {
                        belowRight = false;
                        if((_chessBoardPositions[row-i,col+i].name[1]=='B' || _chessBoardPositions[row-i,col+i].name[1]=='Q' ) && _chessBoardPositions[row-i,col+i].name[0]!=turn)
                        {
                            checkposition[0,0]=col + i;
                            checkposition[0,1]=row - i;
                            return false;
                        }
                    }
                }
                if(row +i<8 && col-i>=0 && aboveLeft)
                {
                    if(_chessBoardPositions[row+i,col-i]!=null)
                    {
                        aboveLeft = false;
                        if((_chessBoardPositions[row+i,col-i].name[1]=='B' || _chessBoardPositions[row+i,col-i].name[1]=='Q' ) && _chessBoardPositions[row+i,col-i].name[0]!=turn)
                        {
                            checkposition[0,0]=col - i;
                            checkposition[0,1]=row + i;
                            return false;
                        }
                    }
                }
                if( row-i>=0 && col-i>=0 && belowLeft)
                {
                    if(_chessBoardPositions[row-i,col-i]!=null)
                    {
                        belowLeft = false;
                        if((_chessBoardPositions[row-i,col-i].name[1]=='B' || _chessBoardPositions[row-i,col-i].name[1]=='Q' ) && _chessBoardPositions[row-i,col-i].name[0]!=turn)
                        {
                            checkposition[0,0]=col - i;
                            checkposition[0,1]=row - i;
                            return false;
                        }
                    }
                }
            }

            //check for pawn
            int counter = -1;
            if(turn == 'W')
                counter = 1;
            else if(turn == 'B')
                {counter = -1;}
            if(row<7 && row>0)
            {
                if(col<7)
                {
                    if(_chessBoardPositions[row-counter,col+counter]!=null)
                    {
                        if(_chessBoardPositions[row-counter,col+counter].name[1]=='P'  && _chessBoardPositions[row-counter,col+counter].name[0]!=turn)
                        {
                            checkposition[0,0]=col+counter;
                            checkposition[0,1]=row - counter;
                            return false;
                        }
                    }
                }
                if(col>0)
                {
                    if(_chessBoardPositions[row-counter,col-counter]!=null)
                    {
                        if(_chessBoardPositions[row-counter,col-counter].name[1]=='P' && _chessBoardPositions[row-counter,col-counter].name[0]!=turn)
                        {
                            checkposition[0,0]=col-counter;
                            checkposition[0,1]=row-counter;
                            return false;
                        }
                    }
                }
            }

//check knight
                int a = row-2, j;
                while(a<=row+2)
                {
                    if(a>=0 && a<8)
                    {
                        j = col-1;
                        while(j<=col+1)
                        {
                            if(j>=0 && j<8)
                            {
                                if(_chessBoardPositions[a,j]!=null)
                                {
                                    if(_chessBoardPositions[a,j].name[2]=='n' && _chessBoardPositions[a,j].name[0]!=turn)
                                    {
                                        checkposition[0,0]=j;
                                        checkposition[0,1]=a;
                                        return false; 
                                    }
                                }
                            }
                            j+=2;
                        }
                    }
                        a+=4;
                }

                a = row-1;
                while(a<=row+1)
                {
                    if(a>=0 && a<8)    
                    {
                        j = col-2;
                        while(j<=col+2)
                        {
                            if(j>=0 && j<8)
                            {
                                if(_chessBoardPositions[a,j]!=null)
                                {
                                    if(_chessBoardPositions[a,j].name[2]=='n' && _chessBoardPositions[a,j].name[0]!=turn)
                                    {
                                        checkposition[0,0]=j;
                                        checkposition[0,1]=a;
                                        return false; 
                                    }
                                }
                            }
                            j+=4;
                        }
                    }
                        a+=2;
                }

                //king
            if(row<7)
            {
                if(_chessBoardPositions[row+1,col]!=null)
                    if(_chessBoardPositions[row+1,col].name[4]=='g' && _chessBoardPositions[row+1,col].name[0]!=turn)
                        return false;
                if(col<7)
                {
                    if(_chessBoardPositions[row+1,col+1]!=null)
                        if(_chessBoardPositions[row+1,col+1].name[4]=='g' && _chessBoardPositions[row+1,col+1].name[0]!=turn)
                            return false;
                    if(_chessBoardPositions[row,col+1]!=null)
                        if(_chessBoardPositions[row,col+1].name[4]=='g' && _chessBoardPositions[row,col+1].name[0]!=turn)
                            return false;
                }
                if(col>0)
                {
                    if(_chessBoardPositions[row+1,col-1]!=null)
                        if(_chessBoardPositions[row+1,col-1].name[4]=='g' && _chessBoardPositions[row+1,col-1].name[0]!=turn)
                            return false;
                    if(_chessBoardPositions[row,col-1]!=null)
                        if(_chessBoardPositions[row,col-1].name[4]=='g' && _chessBoardPositions[row,col-1].name[0]!=turn)
                            return false;
                }
            }
            if(row>0)
            {
                if(_chessBoardPositions[row-1,col]!=null)
                    if(_chessBoardPositions[row-1,col].name[4]=='g' && _chessBoardPositions[row-1,col].name[0]!=turn)
                        return false;
                if(col<7)
                {
                    if(_chessBoardPositions[row-1,col+1]!=null)
                        if(_chessBoardPositions[row-1,col+1].name[4]=='g' && _chessBoardPositions[row,col+1].name[0]!=turn)
                            return false;
                    if(_chessBoardPositions[row,col+1]!=null)
                        if(_chessBoardPositions[row,col+1].name[4]=='g' && _chessBoardPositions[row,col+1].name[0]!=turn)
                            return false;
                }
                if(col>0)
                {
                    if(_chessBoardPositions[row-1,col-1]!=null)
                        if(_chessBoardPositions[row-1,col-1].name[4]=='g' && _chessBoardPositions[row-1,col-1].name[0]!=turn)
                            return false;
                    if(_chessBoardPositions[row,col-1]!=null)
                        if(_chessBoardPositions[row,col-1].name[4]=='g' && _chessBoardPositions[row,col-1].name[0]!=turn)
                            return false;
                }
            }
            return true;
    }

    internal GameObject GetTile(int i, int j) {
        try {
            return _chessBoard[i, j];
        } catch (Exception) {
            Debug.LogError("Invalid row or column.");
            return null;
        }
    }
    internal void Highlight(int row, int col) {
        var tile = GetTile(row, col).transform;
        if (tile == null) {
            Debug.LogError("Invalid row or column.");
            return;
        }
        checkmatecondition = true;
        bool hasHighlight = false;
        foreach (Transform child in tile) {
        if (child.gameObject.name.Contains("Highlighter")) {
            hasHighlight = true;
            break;
        }
        }
        if (!hasHighlight) {
        Instantiate(_highlightPrefab, tile.transform.position, Quaternion.identity, tile.transform);
    }
    }

    internal void HighlightR(int row, int col) {
        var tile = GetTile(row, col).transform;
        if (tile == null) {
            Debug.LogError("Invalid row or column.");
            return;
        }
        checkmatecondition = true;
        bool hasHighlight = false;
    foreach (Transform child in tile) {
        if (child.gameObject.name.Contains("HighlighterRed")) {
            hasHighlight = true;
            break;
        }
    }
    
    // Only instantiate if there's no highlight
    if (!hasHighlight) {
        Instantiate(_highlightRPrefab, tile.transform.position, Quaternion.identity, tile.transform);
    }
    }

    internal void ClearHighlights() {
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                var tile = GetTile(i, j);
                if (tile.transform.childCount <= 0) continue;
                foreach (Transform childTransform in tile.transform) {
                    Destroy(childTransform.gameObject);
                }
            }
        }
    }


    #region Highlight Testing

    // private void Start() {
    //     StartCoroutine(Testing());
    // }

    // private IEnumerator Testing() {
    //     Highlight(2, 7);
    //     yield return new WaitForSeconds(1f);
    //
    //     ClearHighlights();
    //     Highlight(2, 7);
    //     Highlight(2, 6);
    //     Highlight(2, 5);
    //     Highlight(2, 4);
    //     yield return new WaitForSeconds(1f);
    //
    //     ClearHighlights();
    //     Highlight(7, 7);
    //     Highlight(2, 7);
    //     yield return new WaitForSeconds(1f);
    // }

    #endregion
}