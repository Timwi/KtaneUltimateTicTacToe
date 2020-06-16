using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
// O = "ſ"
public class UltimateTicTacToeScript : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMSelectable[] Cells;
    public Material[] Materials;
    public GameObject StatusLight;

    static int moduleIdCounter = 1;
    int moduleId;
    bool moduleSolved = false;

    //struct Cell
    //{
    //    public KMSelectable Button;
    //    public TextMesh Label;
    //    public string Coordinate;
    //    public string Parent;
    //    public string Owner;
    //    public bool Blue;
    //    public bool Green;
    //    public bool Red;
    //}

    //Cell[][][][] Grid;

    //void Start()
    //{
    //    moduleId = moduleIdCounter++;

    //    StatusLight.SetActive(false);
    //    Generate();
    //}

    //void Generate()
    //{
    //    Grid = new Cell[3][][][]
    //    {
    //        new Cell[3][][]
    //        {
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            },
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            },
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            }
    //        },
    //        new Cell[3][][]
    //        {
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            },
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            },
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            }
    //        },
    //        new Cell[3][][]
    //        {
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            },
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            },
    //            new Cell[3][]
    //            {
    //                new Cell[3],
    //                new Cell[3],
    //                new Cell[3]
    //            }
    //        },
    //    };

    //    var c = 0;
    //    var logMessage = new StringBuilder();

    //    for (var parentRow = 0; parentRow < 3; parentRow++)
    //    {
    //        for (var parentCol = 0; parentCol < 3; parentCol++)
    //        {
    //            for (var childRow = 0; childRow < 3; childRow++)
    //            {
    //                for (var childCol = 0; childCol < 3; childCol++)
    //                {
    //                    var mat = 0;
    //                    Grid[parentRow][parentCol][childRow][childCol].Button = Cells[c];
    //                    Grid[parentRow][parentCol][childRow][childCol].Label = Grid[parentRow][parentCol][childRow][childCol].Button.GetComponentInChildren<TextMesh>();
    //                    Grid[parentRow][parentCol][childRow][childCol].Coordinate = string.Format("{0}{1}", (char) (childCol + 'A'), childRow + 1);
    //                    Grid[parentRow][parentCol][childRow][childCol].Parent = string.Format("{0}{1}", (char) (parentCol + 'A'), parentRow + 1);
    //                    Grid[parentRow][parentCol][childRow][childCol].Owner = "";
    //                    Grid[parentRow][parentCol][childRow][childCol].Blue = Random.Range(0, 2) == 0 ? false : true;
    //                    Grid[parentRow][parentCol][childRow][childCol].Green = Random.Range(0, 2) == 0 ? false : true;
    //                    Grid[parentRow][parentCol][childRow][childCol].Red = Random.Range(0, 2) == 0 ? false : true;
    //                    Grid[parentRow][parentCol][childRow][childCol].Label.color = new Color32(235, 235, 235, 255);
    //                    if (Grid[parentRow][parentCol][childRow][childCol].Blue)
    //                        mat += 1;
    //                    if (Grid[parentRow][parentCol][childRow][childCol].Green)
    //                    {
    //                        mat += 2;
    //                        Grid[parentRow][parentCol][childRow][childCol].Label.color = new Color32(50, 50, 50, 255);
    //                    }
    //                    if (Grid[parentRow][parentCol][childRow][childCol].Red)
    //                        mat += 4;

    //                    Grid[parentRow][parentCol][childRow][childCol].Button.GetComponent<MeshRenderer>().material = Materials[mat];
    //                    c++;

    //                    logMessage.AppendFormat("[Ultimate Tic Tac Toe #{0}] Child coordinate: {1} - Parent coordinate: {2} - Selected color channels: {3}\r\n", moduleId, Grid[parentRow][parentCol][childRow][childCol].Coordinate, Grid[parentRow][parentCol][childRow][childCol].Parent,
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == false && Grid[parentRow][parentCol][childRow][childCol].Green == false && Grid[parentRow][parentCol][childRow][childCol].Red == false) ? "None" :
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == true && Grid[parentRow][parentCol][childRow][childCol].Green == false && Grid[parentRow][parentCol][childRow][childCol].Red == false) ? "Blue" :
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == true && Grid[parentRow][parentCol][childRow][childCol].Green == true && Grid[parentRow][parentCol][childRow][childCol].Red == false) ? "Blue, Green" :
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == false && Grid[parentRow][parentCol][childRow][childCol].Green == true && Grid[parentRow][parentCol][childRow][childCol].Red == false) ? "Green" :
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == false && Grid[parentRow][parentCol][childRow][childCol].Green == true && Grid[parentRow][parentCol][childRow][childCol].Red == true) ? "Green, Red" :
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == false && Grid[parentRow][parentCol][childRow][childCol].Green == false && Grid[parentRow][parentCol][childRow][childCol].Red == true) ? "Red" :
    //                        (Grid[parentRow][parentCol][childRow][childCol].Blue == true && Grid[parentRow][parentCol][childRow][childCol].Green == false && Grid[parentRow][parentCol][childRow][childCol].Red == true) ? "Red, Blue" :
    //                        "All");
    //                }
    //            }
    //        }
    //    }
    //    Debug.Log(logMessage.ToString());
    //}

    struct Cell
    {
        public KMSelectable Button;
        public TextMesh Label;
        public int Color;
        public bool? OPlaced;
    }

    int? currentValidBigCell;
    Cell[] grid = new Cell[81];
    bool?[] wonGrids = new bool?[9];

    static readonly int[][] tictactoes = new[] {
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        new[] { 0, 3, 6 },
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        new[] { 0, 4, 8 },
        new[] { 2, 4, 6 }
    };

    void Start()
    {
        moduleId = moduleIdCounter++;
        StatusLight.SetActive(false);
        Generate();
    }

    void Generate()
    {
        var colorNames = "KBGCRMYW";
        for (var i = 0; i < grid.Length; i++)
        {
            grid[i] = new Cell { Button = Cells[i], Label = Cells[i].GetComponentInChildren<TextMesh>(), Color = Random.Range(0, 8) };
            grid[i].Button.GetComponent<MeshRenderer>().material = Materials[grid[i].Color];
            grid[i].Label.color = (grid[i].Color & 2) != 0 ? new Color32(50, 50, 50, 255) : new Color32(235, 235, 235, 255);
            grid[i].Label.text = "";
            Cells[i].OnInteract += CellPressed(i);
        }
        for (var i = 0; i < wonGrids.Length; i++)
            wonGrids[i] = null;
        var gridStr = new StringBuilder();
        for (var row = 0; row < 9; row++)
            gridStr.AppendFormat("[Ultimate Tic Tac Toe #{0}] {1}\r\n", moduleId, Enumerable.Range(0, 9).Select(col => colorNames[grid[(col / 3 + 3 * (row / 3)) * 9 + (col % 3 + 3 * (row % 3))].Color]).Join(" "));
        Debug.LogFormat("[Ultimate Tic Tac Toe #{0}] Grid:\r\n{1}", moduleId, gridStr);
    }

    private KMSelectable.OnInteractHandler CellPressed(int cell)
    {
        //Player: O  - Bomb: X
        return delegate
        {
            if (moduleSolved || grid[cell].OPlaced != null)
                return false;

            if (wonGrids[cell / 9] != null || (currentValidBigCell != null && currentValidBigCell.Value != cell / 9))
            {
                BombModule.HandleStrike();
                return false;
            }

            if (place(cell, o: true))   // If this returns true, we either won or got a strike+reset
                return false;
            if (Enumerable.Range(0, 81).All(i => grid[i].OPlaced != null || wonGrids[i / 9] != null))
            {
                // It’s a draw!
                Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] You allowed the game to end in a draw! Strike!", moduleId);
                BombModule.HandleStrike();
                Generate();
            }
            else
                BombTurn(cell % 9);
            return false;
        };
    }

    // Returns true in case of win or strike+reset
    bool place(int cell, bool o)
    {
        Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] {1} placed a {2} in {3}{4}/{5}{6}.",
            moduleId, o ? "Player" : "Bomb", o ? "O" : "X",
            (char) ('A' + (cell / 9) % 3), (cell / 9) / 3 + 1,
            (char) ('A' + (cell % 9) % 3), (cell % 9) / 3 + 1);

        grid[cell].Label.text = o ? "ſ" : "X";
        grid[cell].OPlaced = o;

        var bigGrid = cell / 9;
        foreach (var tictactoe in tictactoes)
            if (tictactoe.All(i => grid[bigGrid * 9 + i].OPlaced == o))
            {
                for (var i = 0; i < 9; i++)
                {
                    grid[bigGrid * 9 + i].Label.text = i == 4 ? (o ? "ſ" : "X") : "";
                    grid[bigGrid * 9 + i].OPlaced = null;
                }
                grid[bigGrid * 9 + 4].Label.characterSize *= 3;
                wonGrids[bigGrid] = o;
                return checkForBigTicTacToe(o);
            }
        return false;
    }

    // Returns true in case of win or strike+reset
    private bool checkForBigTicTacToe(bool o)
    {
        foreach (var tictactoe in tictactoes)
            if (tictactoe.All(i => wonGrids[i] == o))
            {
                if (o)
                {
                    Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] Module solved!", moduleId);
                    BombModule.HandlePass();
                    return true;
                }
                else
                {
                    Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] You allowed the bomb to win the game! Strike!", moduleId);
                    BombModule.HandleStrike();
                    Generate();
                    return true;
                }
                break;
            }
        return false;
    }

    void BombTurn(int bigGrid)
    {
        var validBigGrid = wonGrids[bigGrid] != null ? (int?) null : bigGrid;

        tryAgain:
        // Which colors are in this big grid?
        var whereToPlace = Enumerable.Range(0, 81)
            .Where(i => grid[i].OPlaced == null && (validBigGrid == null || validBigGrid.Value == i / 9) && wonGrids[i / 9] == null)
            .Select(i => new { Index = i, grid[i].Color })
            .OrderBy(inf => inf.Color).ThenBy(inf => inf.Index)
            .FirstOrDefault();
        if (whereToPlace == null)
        {
            // There is no unused square left in this small 3×3 — try the whole board
            validBigGrid = null;
            goto tryAgain;
        }

        place(whereToPlace.Index, o: false);
        currentValidBigCell = (wonGrids[whereToPlace.Index % 9] != null || Enumerable.Range(0, 9).All(i => grid[whereToPlace.Index % 9 * 9 + i].OPlaced != null)) ? (int?) null : whereToPlace.Index % 9;
    }
}
