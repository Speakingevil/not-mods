using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using KModkit;

public class JCFScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] buttons;
    public TextMesh display;

    private readonly int[,] grid = new int[11, 11]
    {
        { 0, 1, 2, 3, 4, 5, 1, 4, 2, 0, 5},
        { 3, 4, 1, 4, 0, 2, 5, 1, 5, 3, 2},
        { 2, 1, 5, 0, 3, 1, 4, 2, 3, 4, 0},
        { 4, 2, 0, 1, 5, 4, 3, 0, 1, 2, 5},
        { 2, 5, 3, 4, 3, 0, 1, 2, 3, 5, 0},
        { 5, 4, 5, 2, 1, 3, 0, 3, 4, 0, 1},
        { 1, 3, 4, 5, 2, 5, 2, 1, 0, 3, 4},
        { 0, 2, 1, 3, 4, 3, 5, 0, 5, 4, 2},
        { 3, 5, 2, 0, 1, 2, 3, 5, 2, 1, 4},
        { 4, 0, 3, 2, 3, 1, 2, 4, 0, 5, 3},
        { 5, 3, 4, 1, 0, 4, 1, 3, 5, 2, 1}
    };
    private string[][] mazes = new string[4][] {
        new string[23]
        {
            "XXXXXXXXXXXXXXXXXXXXXXX",
            "X                 X   X",
            "X XXXXX XXXXXXXXX X X X",
            "X X             X X X X",
            "X X XXXXX XXXXX X X X X",
            "X X   X       X     X X",
            "X X X X XXX X X XXX X X",
            "X   X X   X X       X X",
            "X XXX X X X XXX XXX X X",
            "X   X X X     X X     X",
            "X X X X X XXX X X XXX X",
            "X X     X           X X",
            "X X XXXXX X XXXXX X X X",
            "X X       X     X X   X",
            "X XXX X XXX XXX X X X X",
            "X     X     X       X X",
            "XXXXX XXX X X XXXXXXX X",
            "X         X           X",
            "X XXX XXXXX X XXX X XXX",
            "X X         X   X X   X",
            "X X XXXXX XXX X X XXX X",
            "X             X       X",
            "XXXXXXXXXXXXXXXXXXXXXXX",
        },
        new string[23]
        {
            "XXXXXXXXXXXXXXXXXXXXXXX",
            "X   X                 X",
            "X X X XXXXX XXXXX XXX X",
            "X X X     X X       X X",
            "X X X XXX X X XXXXX X X",
            "X X   X   X X     X   X",
            "X X XXX X X X X X X X X",
            "X       X     X X   X X",
            "X X XXX X XXX X XXX X X",
            "X X X         X       X",
            "X X X XXXXXXXXX XXX XXX",
            "X X     X       X     X",
            "X XXX X X XXX X X XXX X",
            "X     X       X     X X",
            "X XXX XXX XXX X X X X X",
            "X       X X   X X X X X",
            "XXXXXXX X X XXX X X X X",
            "X         X X     X   X",
            "X XXX XXX X X X XXX X X",
            "X     X   X   X     X X",
            "X XXXXX XXX X X X XXX X",
            "X           X   X     X",
            "XXXXXXXXXXXXXXXXXXXXXXX",
        },
        new string[23]
        {
            "XXXXXXXXXXXXXXXXXXXXXXX",
            "X         X           X",
            "X XXX XXX X XXX X XXX X",
            "X X     X X X   X     X",
            "X X XXX X X X XXX XXXXX",
            "X           X   X     X",
            "X XXX XXX X X X X XXX X",
            "X         X   X     X X",
            "XXXXXXX XXXXX XXXXX X X",
            "X       X         X   X",
            "X X XXX X X XXX X X XXX",
            "X X       X   X X     X",
            "X XXX XXXXXXX X XXX X X",
            "X   X X       X     X X",
            "X X X X XXX XXXXX XXX X",
            "X X           X       X",
            "X XXXXXXXXX X X X XXXXX",
            "X     X     X   X     X",
            "XXX X X XXX X X XXXXX X",
            "X   X       X X       X",
            "X XXX XXX X X X X XXX X",
            "X         X     X     X",
            "XXXXXXXXXXXXXXXXXXXXXXX",
        },
        new string[23]
        {
            "XXXXXXXXXXXXXXXXXXXXXXX",
            "X         X       X   X",
            "X XXXXXXX X XXXXX X X X",
            "X       X   X       X X",
            "X XXXXX X XXX XXX XXX X",
            "X X     X     X   X   X",
            "X X X XXX XXX X XXX X X",
            "X   X         X     X X",
            "X XXX XXXXXXX XXX X X X",
            "X   X     X   X   X   X",
            "X X X XXX X XXX X X XXX",
            "X X   X       X X     X",
            "X X XXX XXXXX X XXXXX X",
            "X     X     X         X",
            "XXX X X X X X XXX XXXXX",
            "X   X   X X   X       X",
            "X XXX XXX XXX X X XXX X",
            "X X     X X     X   X X",
            "X X XXX X X XXX X X X X",
            "X           X     X X X",
            "X XXXXXXXXX X XXXXX X X",
            "X           X         X",
            "XXXXXXXXXXXXXXXXXXXXXXX",
        },       
    };
    private string[] words = new string[6] { "RED", "GREEN", "BLUE", "MAGENTA", "YELLOW", "WHITE" };
    private Color[] cols = new Color[6] { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 0, 1), new Color(1, 1, 0), new Color(1, 1, 1) };
    private string[] maze = new string[23];
    private int[][] exits = new int[2][] { new int[2], new int[2] };
    private int[][] locs = new int[2][] { new int[2], new int[2] };
    private bool[,] up = new bool[2, 2];
    private int[] shifts = new int[2];
    private int[][] bars = new int[2][] { new int[5], new int[5] };
    private int[][] initlocs = new int[2][] { new int[2], new int[2]};


    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        int[] m = new int[2] { info.GetSerialNumberLetters().Last() - 'A', info.GetSerialNumberNumbers().Sum() % 6 };
        m[0] = m[0] < 6 ? 0 : (m[0] < 13 ? 1 : (m[0] < 20 ? 2 : 3));
        switch (m[1])
        {
            case 0: maze = mazes[m[0]]; break;
            case 1:
                for (int i = 0; i < 23; i++)
                    for (int j = 0; j < 23; j++)
                        maze[i] += mazes[m[0]][22 - j][i];
                break;
            case 2:
                for (int i = 0; i < 23; i++)
                    for (int j = 0; j < 23; j++)
                        maze[i] += mazes[m[0]][j][22 - i];
                break;
            case 3:
                for (int i = 0; i < 23; i++)
                    for (int j = 0; j < 23; j++)
                        maze[i] += mazes[m[0]][22 - i][j];
                break;
            case 4:
                for (int i = 0; i < 23; i++)
                    for (int j = 0; j < 23; j++)
                        maze[i] += mazes[m[0]][i][22 - j];
                break;
            default:
                for (int i = 0; i < 23; i++)
                    for (int j = 0; j < 23; j++)
                        maze[i] += mazes[m[0]][22 - i][22 - j];
                break;
        }
        Debug.LogFormat("[Juxtacolour Flash #{0}] The configuration of the walls that must be avoided:\n[Juxtacolour Flash #{0}] {1}", moduleID, string.Join("\n[Juxtacolour Flash #" + moduleID + "] ", Enumerable.Range(0, 23).Select(x => string.Join("", maze[x].Select(z => z == 'X' ? "\u25a0" : "\u25a1").ToArray())).ToArray()));
        for (int i = 0; i < 2; i++)
        {
            int ch = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(info.GetSerialNumber()[i]);
            exits[i][0] = (ch / 6) * 2;
            exits[i][1] = (ch % 6) * 2;
            up[1, i] = Random.Range(0, 2) == 1;
            shifts[i] = Random.Range(-2, 3);
            locs[i] = new int[2] { Random.Range(2, 9), Random.Range(2, 9)};
        }
        while (Mathf.Abs(exits[0][0] - locs[0][0]) < 3 && Mathf.Abs(exits[0][1] - locs[0][1]) < 3)
            locs[0] = new int[2] { Random.Range(2, 9), Random.Range(2, 9) };
        while ((Mathf.Abs(exits[1][0] - locs[1][0]) < 3 && Mathf.Abs(exits[1][1] - locs[1][1]) < 3) || (Mathf.Abs(locs[0][0] - locs[1][0]) < 4 && Mathf.Abs(locs[0][1] - locs[1][1]) < 4))
            locs[1] = new int[2] { Random.Range(2, 9), Random.Range(2, 9) };
        for (int i = 0; i < 4; i++)
            initlocs[i / 2][i % 2] = locs[i / 2][i % 2];
        up[0, Random.Range(0, 2)] = true;
        for (int i = 0; i < 2; i++)
        {
            Debug.LogFormat("[Juxtacolour Flash #{0}] The initial {1} cell is located at {2}{3}, scrolling {4}ally.", moduleID, i == 0 ? "word" : "colour", "ABCDEFGHIJK"[locs[i][1]], locs[i][0] + 1, up[0, i] ? "vertic" : "horizont");
            Debug.LogFormat("[Juxtacolour Flash #{0}] The {1} exit cell is located at {2}{3}.", moduleID, i == 0 ? "word" : "colour", "ABCDEFGHIJK"[exits[i][1]], exits[i][0] + 1);
            bars[i] = Bar(locs[i], up[0, i]);
        }
        foreach(KMSelectable button in buttons)
        {
            int b = button == buttons[0] ? 0 : 1;
            button.OnInteract = delegate () { if(!moduleSolved) Press(b); return false; };
        }
        StartCoroutine("Seq");
    }

    private int[] Bar(int[] a, bool b)
    {
        int o = 0;
        if (b)
        {
            if (a[0] < 2)
                o = 2;
            else if (a[0] > 8)
                o = 8;
            else
                o = a[0];
            return Enumerable.Range(-2, 5).Select(x => ((o + x) * 11) + a[1]).ToArray();
        }
        else
        {
            if (a[1] < 2)
                o = 2;
            else if (a[1] > 8)
                o = 8;
            else
                o = a[1];
            return Enumerable.Range(-2, 5).Select(x => (a[0] * 11) + o + x).ToArray();
        }
    }

    private void Press(int b)
    {
        buttons[b].AddInteractionPunch(0.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[b].transform);
        Debug.Log(display.text + " in " + words[System.Array.IndexOf(cols, display.color)]);
        int tar = bars[b][shifts[b] + 2];
        if (tar != (locs[b][0] * 11) + locs[b][1])
        {
            int i = 0;
            if (up[0, b])
            {
                if (tar < (locs[b][0] * 11) + locs[b][1])
                {
                    while (tar < ((locs[b][0] - i) * 11) + locs[b][1])
                    {
                        if (maze[2 * (locs[b][0] - i)][(2 * locs[b][1]) + 1] == 'X')
                        {
                            module.HandleStrike();
                            Debug.LogFormat("[Juxtacolour Flash #{0}] Invalid move: Hit wall north of {1}{2}.", moduleID, "ABCDEFGHIJK"[locs[b][1]], locs[b][0] - i + 1);
                            return;
                        }
                        i++;
                    }
                }
                else
                {
                    while (tar > ((locs[b][0] + i) * 11) + locs[b][1])
                    {
                        if (maze[(2 * (locs[b][0] + i)) + 2][(2 * locs[b][1]) + 1] == 'X')
                        {
                            module.HandleStrike();
                            Debug.LogFormat("[Juxtacolour Flash #{0}] Invalid move: Hit wall south of {1}{2}.", moduleID, "ABCDEFGHIJK"[locs[b][1]], locs[b][0] + i + 1);
                            return;
                        }
                        i++;
                    }
                }
            }
            else
            {
                if (tar < (locs[b][0] * 11) + locs[b][1])
                {
                    while (tar < (locs[b][0] * 11) + locs[b][1] - i)
                    {
                        if (maze[(2 * locs[b][0]) + 1][2 * (locs[b][1] - i)] == 'X')
                        {
                            module.HandleStrike();
                            Debug.LogFormat("[Juxtacolour Flash #{0}] Invalid move: Hit wall west of {1}{2}.", moduleID, "ABCDEFGHIJK"[locs[b][1] - i], locs[b][0] + 1);
                            return;
                        }
                        i++;
                    }
                }
                else
                {
                    while (tar > (locs[b][0] * 11) + locs[b][1] + i)
                    {
                        if (maze[(2 * locs[b][0]) + 1][(2 * (locs[b][1] + i)) + 2] == 'X')
                        {
                            module.HandleStrike();
                            Debug.LogFormat("[Juxtacolour Flash #{0}] Invalid move: Hit wall east of {1}{2}.", moduleID, "ABCDEFGHIJK"[locs[b][1] + i], locs[b][0] + 1);
                            return;
                        }
                        i++;
                    }
                }
            }
        }
        if(exits[b][0] == tar / 11 && exits[b][1] == tar % 11 && exits[1 - b][0] == locs[1 - b][0] && exits[1 - b][1] == locs[1 - b][1])
        {
            moduleSolved = true;
            Audio.PlaySoundAtTransform("JSolve", transform);
            Debug.LogFormat("[Juxtacolour Flash #{0}] Both exits reached.", moduleID);
            StopCoroutine("Seq");
            display.text = words[grid[exits[0][0], exits[0][1]]];
            display.color = cols[grid[exits[1][0], exits[1][1]]];
            module.HandlePass();
            return;
        }
        int[] premove = Bar(new int[2] { tar / 11, tar % 11 }, !up[0, b]);
        int[] intersects = premove.Where(x => bars[1 - b].Contains(x) && !exits.Select(z => (z[0] * 11) + z[1]).Contains(x)).ToArray();
        if (intersects.Count() > 0)
        {
            module.HandleStrike();
            Audio.PlaySoundAtTransform("Smash", transform);
            Debug.LogFormat("[Juxtacolour Flash #{0}] Invalid move: Bars intersected at {1}.", moduleID, string.Join(", ", intersects.Select(x => "ABCDEFGHIJK"[x % 11] + ((x / 11) + 1).ToString()).ToArray()));
            if((locs[0][0] == locs[1][0] && Mathf.Abs(locs[0][1] - locs[1][1]) < 3) || (locs[0][1] == locs[1][1] && Mathf.Abs(locs[0][0] - locs[1][0]) < 3))
            {
                Audio.PlaySoundAtTransform("Reset", transform);
                Debug.LogFormat("[Juxtacolour Flash #{0}] No valid moves. Resetting cell positions.", moduleID);
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                        locs[i][j] = initlocs[i][j];
                    bars[i] = Bar(locs[i], up[0, b]);
                }
            }
            return;
        }
        Audio.PlaySoundAtTransform("Move", buttons[b].transform);
        Debug.LogFormat("[Juxtacolour Flash #{0}] Valid move: {1} cell from {2}{3}-{6} to {4}{5}-{7}.", moduleID, b == 0 ? "Word" : "Colour", "ABCDEFGHIJK"[locs[b][1]], locs[b][0] + 1, "ABCDEFGHIJK"[tar % 11], (tar / 11) + 1, up[0, b] ? "V" : "H", up[0, b] ? "H" : "V");
        locs[b][0] = tar / 11;
        locs[b][1] = tar % 11;
        up[0, b] ^= true;
        up[1, b] = Random.Range(0, 2) == 0;
        shifts[b] = 0;
        bars[b] = Bar(locs[b], up[0, b]);
    }

    private IEnumerator Seq()
    {
        int[] b = new int[2];
        while (!moduleSolved)
        {  
            for(int i = 0; i < 2; i++)
                b[i] = bars[i][shifts[i] + 2];
            display.text = words[grid[b[0] / 11, b[0] % 11]];
            display.color = cols[grid[b[1] / 11, b[1] % 11]];
            yield return new WaitForSeconds(0.75f);
            for(int i = 0; i < 2; i++)
            {
                if (shifts[i] < -1)
                    up[1, i] = true;
                else if (shifts[i] > 1)
                    up[1, i] = false;
                shifts[i] += up[1, i] ? 1 : -1;
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use [!{0} press no RGB] to wait for those word colors to cycle and then press on the last entry. Similarly, use [!{0} press yes RGB] to wait for those words to cycle. Colors are abbreviated by their first letter. “press” is optional.";
#pragma warning restore 414
    
    struct ScannerState
    {
        public bool[] validUpStates;
        public int finalScannerPos;
    }
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        Match m = Regex.Match(command, @"^(?:PRESS\s+)?(Y(?:ES)?|NO?)\s+([RGBMYW]{1,8})$");
        if (!m.Success)
            yield break;    

        int[] requestedSequence = m.Groups[2].Value.Select(x => "RGBMYW".IndexOf(x)).ToArray();
        int[] reversedRequestedSequence = requestedSequence.Reverse().ToArray();
        int usedScanner = m.Groups[1].Value[0] == 'Y' ? 0 : 1;
        int[] usedBar = bars[usedScanner];
        int[] pingPong = { 0, 1, 2, 3, 4, 3, 2, 1, 0, 1, 2, 3, 4, 3, 2, 1 };
        int[] displayedSequence = pingPong.Select(x => grid[usedBar[x] / 11, usedBar[x] % 11]).ToArray();
        
        List<int> startIxs = new List<int>();
        for (int skipIx = 0; skipIx < 16 - requestedSequence.Length; skipIx++)
        {
            if (displayedSequence.Skip(skipIx).Take(requestedSequence.Length).SequenceEqual(requestedSequence))
                startIxs.Add(skipIx);
        }
        if (startIxs.Count == 0)
            yield return string.Format("sendtochaterror The requested sequence {0} cannot be found in the cycle.", requestedSequence.Select(x => words[x][0]).Join(""));

        List<ScannerState> answerStates = new List<ScannerState>();
        foreach (int startIx in startIxs)
        {
            int scannerIx = startIx % 8;
            bool[] possibleValuesOfUpArray;
            if (scannerIx == 0 || scannerIx == 4)
                possibleValuesOfUpArray = new[] { true, false };
            else if (scannerIx < 4)
                possibleValuesOfUpArray = new[] { true };
            else if (scannerIx > 4)
                possibleValuesOfUpArray = new[] { false };
            else throw new System.ArgumentException();
            int finalIx = startIx + requestedSequence.Length - 1 ;
            answerStates.Add(new ScannerState() { validUpStates = possibleValuesOfUpArray, finalScannerPos = pingPong[finalIx] - 2 });
        }
        yield return null;
        while (!answerStates.Any(st => st.validUpStates.Contains(up[1,usedScanner]) && st.finalScannerPos == shifts[usedScanner]))
            yield return "trycancel";
        yield return new WaitForSeconds(0.1f);
        buttons[usedScanner].OnInteract();
    }
}
