using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class UltimateTicTacToeScript : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMSelectable[] Cells;
    public Material[] Materials;
    public GameObject StatusLight;
    public KMRuleSeedable RuleSeedable;
    public GameObject[] Borders;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved = false;
    private Material white, black;
    private Coroutine gridBorder;

    struct Cell
    {
        public KMSelectable Button;
        public TextMesh Label;
        public int Color;
        public bool? OPlaced;
    }

    struct MoveResult
    {
        public int? PlayerWins; // if not null, contains the index of the middle cell of the player’s winning row
        public bool BombWins;
        public bool Draw;
        public GameState NewGameState;
    }

    class GameState
    {
        public int? currentValidBigCell;
        public Cell[] grid = new Cell[81];
        public bool?[] wonGrids = new bool?[9];

        public GameState Clone()
        {
            return new GameState { currentValidBigCell = currentValidBigCell, grid = (Cell[]) grid.Clone(), wonGrids = (bool?[]) wonGrids.Clone() };
        }

        public MoveResult MakePlayerMove(int cell, Action<int, bool> log = null, Action<Cell, bool> setPiece = null, Action<Cell[], int, bool> setBigPiece = null)
        {
            return Clone().makePlayerMoveImpl(cell, log, setPiece, setBigPiece);
        }

        MoveResult makePlayerMoveImpl(int cell, Action<int, bool> log, Action<Cell, bool> setPiece, Action<Cell[], int, bool> setBigPiece)
        {
            // Player move
            var result = place(cell, true, log, setPiece, setBigPiece);
            if (result.BombWins || result.PlayerWins != null)
                return result;

            // Check for a draw
            if (Enumerable.Range(0, 81).All(i => grid[i].OPlaced != null || wonGrids[i / 9] != null))
                return new MoveResult { Draw = true };

            // Bomb move
            var bigGrid = cell % 9;
            var validBigGrid = wonGrids[bigGrid] != null ? (int?) null : bigGrid;

            tryAgain:
            // Which colors are in this big grid?
            var whereToPlace = Enumerable.Range(0, grid.Length)
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

            result = place(whereToPlace.Index, false, log, setPiece, setBigPiece);
            if (result.BombWins)
                return result;

            currentValidBigCell = (wonGrids[whereToPlace.Index % 9] != null || Enumerable.Range(0, 9).All(i => grid[whereToPlace.Index % 9 * 9 + i].OPlaced != null)) ? (int?) null : whereToPlace.Index % 9;
            return new MoveResult { NewGameState = this };
        }

        MoveResult place(int cell, bool o, Action<int, bool> log, Action<Cell, bool> setPiece, Action<Cell[], int, bool> setBigPiece)
        {
            if (log != null)
                log(cell, o);

            grid[cell].OPlaced = o;
            if (setPiece != null)
                setPiece(grid[cell], o);
            var bigGrid = cell / 9;
            foreach (var tictactoe in tictactoes)
            {
                if (tictactoe.All(i => grid[bigGrid * 9 + i].OPlaced == o))
                {
                    if (setBigPiece != null)
                        setBigPiece(grid, bigGrid, o);
                    for (var i = 0; i < 9; i++)
                        grid[bigGrid * 9 + i].OPlaced = null;
                    wonGrids[bigGrid] = o;
                    return checkForBigTicTacToe(o);
                }
            }
            return new MoveResult();
        }

        // Returns true in case of win or strike+reset
        MoveResult checkForBigTicTacToe(bool o)
        {
            foreach (var tictactoe in tictactoes)
                if (tictactoe.All(i => wonGrids[i] == o))
                    return o ? new MoveResult { PlayerWins = tictactoe[1] } : new MoveResult { BombWins = true };
            return new MoveResult();
        }
    }

    GameState currentGameState;

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

    struct MaterialInfo
    {
        public Material Material;
        public int Color;
    }
    MaterialInfo[] materialInfos;

    void Start()
    {
        var colorNames = "Black,Blue,Green,Cyan,Red,Magenta,Yellow,White".Split(',');
        moduleId = moduleIdCounter++;
        StartCoroutine(HideStatusLight());
        black = Materials[0];
        white = Materials[7];
        materialInfos = Materials.Select((m, ix) => new MaterialInfo { Material = m, Color = ix }).ToArray();
        var rnd = RuleSeedable.GetRNG();
        Debug.LogFormat("[Ultimate Tic Tac Toe #{0}] Using rule seed: {1}", moduleId, rnd.Seed);
        if (rnd.Seed != 1)
            rnd.ShuffleFisherYates(materialInfos);
        Debug.LogFormat("[Ultimate Tic Tac Toe #{0}] Color values are: {1}", moduleId, materialInfos.Select((mi, ix) => string.Format("{0}: {1}", colorNames[mi.Color], ix)).Join(", "));
        Generate();
    }

    private IEnumerator HideStatusLight()
    {
        yield return null;
        StatusLight.transform.localScale = new Vector3(0, 0, 0);
    }

    void Generate()
    {
        const string colorNames = "KBGCRMYW";
        currentGameState = new GameState();
        for (var i = 0; i < currentGameState.grid.Length; i++)
        {
            currentGameState.grid[i] = new Cell { Button = Cells[i], Label = Cells[i].GetComponentInChildren<TextMesh>(), Color = Random.Range(0, 8) };
            currentGameState.grid[i].Button.GetComponent<MeshRenderer>().material = materialInfos[currentGameState.grid[i].Color].Material;
            currentGameState.grid[i].Label.color = (materialInfos[currentGameState.grid[i].Color].Color & 2) != 0 ? new Color32(0, 0, 0, 255) : new Color32(255, 255, 255, 255);
            currentGameState.grid[i].Label.text = "";
            Cells[i].OnInteract += CellPressed(i);
        }
        for (var i = 0; i < currentGameState.wonGrids.Length; i++)
            currentGameState.wonGrids[i] = null;
        var gridStr = new StringBuilder();
        for (var row = 0; row < 9; row++)
            gridStr.AppendFormat("[Ultimate Tic Tac Toe #{0}] {1}\r\n", moduleId, Enumerable.Range(0, 9).Select(col => colorNames[materialInfos[currentGameState.grid[(col / 3 + 3 * (row / 3)) * 9 + (col % 3 + 3 * (row % 3))].Color].Color]).Join(" "));
        Debug.LogFormat("[Ultimate Tic Tac Toe #{0}] Grid:\r\n{1}", moduleId, gridStr);
    }

    private KMSelectable.OnInteractHandler CellPressed(int cell)
    {
        //Player: O  - Bomb: X
        return delegate
        {
            if (moduleSolved || currentGameState.grid[cell].OPlaced != null)
                return false;

            if (currentGameState.wonGrids[cell / 9] != null)
            {
                Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] You attempted to play into subgrid {1}{2} that has already been won. Strike!", moduleId, (char) ('A' + (cell / 9) % 3), (cell / 9) / 3 + 1);
                BombModule.HandleStrike();
                return false;
            }

            if (currentGameState.currentValidBigCell != null && currentGameState.currentValidBigCell.Value != cell / 9)
            {
                Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] You attempted to play in grid {1}{2} but you are restricted to grid {3}{4}. Strike!",
                    moduleId,
                    (char) ('A' + cell / 9 % 3), cell / 9 / 3 + 1,
                    (char) ('A' + currentGameState.currentValidBigCell.Value % 3), currentGameState.currentValidBigCell.Value / 3 + 1);
                BombModule.HandleStrike();
                if (gridBorder == null)
                    gridBorder = StartCoroutine(blinkBorder());
                return false;
            }
            if (gridBorder != null)
            {
                StopCoroutine(gridBorder);
                Borders[currentGameState.currentValidBigCell.Value].gameObject.SetActive(false);
                gridBorder = null;
            }
            var result = currentGameState.MakePlayerMove(
                cell,
                log: (int c, bool o) =>
                {
                    Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] {1} placed a {2} in {3}{4}/{5}{6}.",
                        moduleId, o ? "Player" : "Bomb", o ? "O" : "X",
                        (char) ('A' + (c / 9) % 3), (c / 9) / 3 + 1,
                        (char) ('A' + (c % 9) % 3), (c % 9) / 3 + 1);
                },
                setPiece: (Cell c, bool o) =>
                {
                    c.Label.text = o ? "ſ" : "X";
                },
                setBigPiece: (Cell[] grid, int big, bool o) =>
                {
                    for (var i = 0; i < 9; i++)
                    {
                        grid[big * 9 + i].Label.text = i == 4 ? (o ? "ſ" : "X") : "";
                        grid[big * 9 + i].Label.color = o ? black.color : white.color;
                        grid[big * 9 + i].Button.GetComponent<MeshRenderer>().material = o ? white : black;
                    }
                    grid[big * 9 + 4].Label.characterSize *= o ? 3 : 4;
                });

            if (result.PlayerWins != null)
            {
                Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] Module solved!", moduleId);
                StartCoroutine(SolveAnimation(result.PlayerWins.Value));
                return false;
            }
            else if (result.BombWins || result.Draw)
            {
                Debug.LogFormat(@"[Ultimate Tic Tac Toe #{0}] {1} Strike!", moduleId, result.Draw ? "You allowed the game to end in a draw!" : "You allowed the bomb to win the game!");
                BombModule.HandleStrike();
                Generate();
                return false;
            }

            currentGameState = result.NewGameState;

            return false;
        };
    }

    private IEnumerator blinkBorder()
    {
        var borders = Enumerable.Range(0, Borders[currentGameState.currentValidBigCell.Value].transform.childCount).Select(ix => Borders[currentGameState.currentValidBigCell.Value].transform.GetChild(ix).gameObject).ToArray();
        Borders[currentGameState.currentValidBigCell.Value].gameObject.SetActive(true);
        while (true)
        {
            var hue = 0f;
            while (hue < 1f)
            {
                yield return null;
                hue += Time.deltaTime;
                for (var i = 0; i < borders.Length; i++)
                {
                    borders[i].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(hue, 1f, 1f);
                }
            }
        }
    }

    private IEnumerator SolveAnimation(int bigCell)
    {
        StatusLight.SetActive(true);
        StatusLight.transform.parent = Cells[bigCell * 9 + 4].transform;
        StatusLight.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        StatusLight.transform.localScale = new Vector3(58.82352f, 58.82352f, 58.82352f);
        var duration = 1.6f;
        var elapsed = 0f;
        var finalPosition = new Vector3(0, 0, -.17f);
        while (elapsed < duration)
        {
            StatusLight.transform.localPosition = Vector3.Lerp(new Vector3(0, 0, 2.5f), finalPosition, Easing.OutCubic(elapsed, 0, 1, duration));
            yield return null;
            elapsed += Time.deltaTime;
        }
        StatusLight.transform.localPosition = finalPosition;
        BombModule.HandlePass();
        moduleSolved = true;
        yield break;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} 19 23 22 45 [Place Os in the given cells. The first digit is the big grid in reading order, the second digit is the small grid in reading order]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;

        if (moduleSolved)
        {
            yield return "sendtochaterror The module is already solved.";
            yield break;
        }
        else if ((m = Regex.Match(command, @"^\s*([1-9 ]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            var buttons = m.Groups[1].Value.Where(ch => ch.Equals(' ') == false).Select(ch => int.Parse(ch.ToString())).ToList();
            if (buttons.Count % 2 != 0)
            {
                yield return "sendtochaterror Invalid command syntax. Please check that have digit groups of 2.";
                yield break;
            }

            yield return null;
            while (buttons.Count > 0)
            {
                Cells[(buttons[0] - 1) * 9 + (buttons[1] - 1)].OnInteract();
                buttons.RemoveAt(0);
                buttons.RemoveAt(0);
                yield return new WaitForSeconds(.25f);
            }
            yield break;
        }
        else
            yield return "sendtochaterror Invalid command.";
        yield break;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        var solution = FindSolution(currentGameState);
        if (solution == null)
            yield break;

        foreach (var ix in solution)
        {
            Cells[ix].OnInteract();
            yield return new WaitForSeconds(.25f);
        }
    }

    struct MoveInfo { public int Move; public GameState GameState; }
    IEnumerable<int> FindSolution(GameState gs)
    {
        var validMoves = new List<MoveInfo>();
        for (var i = 0; i < 81; i++)
            if ((gs.currentValidBigCell == null || gs.currentValidBigCell.Value == i / 9) && gs.wonGrids[i / 9] == null && gs.grid[i].OPlaced == null)
            {
                var result = gs.MakePlayerMove(i);
                if (result.PlayerWins != null)
                    return new[] { i };
                if (result.BombWins || result.Draw)
                    continue;

                // Check if it is still possible to create a tic tac toe for the player
                var ngs = result.NewGameState;
                var bigCellsAvailable = Enumerable.Range(0, 9).Select(bigGrid => ngs.wonGrids[bigGrid] ?? tictactoes.Any(ttt => ttt.All(ix => ngs.grid[9 * bigGrid + ix].OPlaced != false))).ToArray();
                if (tictactoes.Any(ttt => ttt.All(ix => bigCellsAvailable[ix])))
                    validMoves.Add(new MoveInfo { Move = i, GameState = result.NewGameState });
            }

        foreach (var move in validMoves)
        {
            var solution = FindSolution(move.GameState);
            if (solution != null)
                return new[] { move.Move }.Concat(solution);
        }
        return null;
    }
}
