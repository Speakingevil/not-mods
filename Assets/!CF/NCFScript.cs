using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class NCFScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] buttons;
    public TextMesh display;

    private readonly int[,,] grid = new int[9, 9, 2]
        {{{5, -3}, {-3, -1}, {-4, -2}, {4, 1}, {-1, -6}, {6, 4}, {2, 3}, {3, -4}, {6, -5}},
         {{5, 6}, {6, 3}, {1, 4}, {-1, -5}, {2, -4}, {-6, -1}, {-5, -6}, {3, -3}, {-2, 1}},
         {{3, 5}, {-3, 2}, {-2, 4}, {5, -1}, {-6, 5}, {-1, -2}, {4, 6}, {1, 2}, {-6, 6}},
         {{5, -4}, {3, -3}, {6, -2}, {4, -3}, {1, 3}, {2, -4}, {3, 5}, {4, -1}, {5, -2}},
         {{2, -2}, {2, 6}, {5, 1}, {4, -4}, {5, 6}, {4, 3}, {-3, -4}, {2, -6}, {-1, -2}},
         {{6, 3}, {1, 4}, {-1, 3}, {6, 4}, {-4, 5}, {-2, 1}, {5, 3}, {1, -1}, {3, 4}},
         {{1, -5}, {3, -5}, {-4, 1}, {6, -2}, {-3, -6}, {4, 1}, {3, -4}, {3, 2}, {-3, 5}},
         {{4, -5}, {1, 2}, {4, -1}, {-5, -6}, {2, 4}, {3, -5}, {6, -1}, {4, -2}, {5, 1}},
         {{3, 1}, {2, -3}, {5, -6}, {-1, -3}, {-5, 2}, {-4, 2}, {1, 4}, {2, -1}, {4, -6}}};
    private readonly int[,] flowchart = new int[36, 2]
        {{10, 33}, {26, 28}, {14, 10}, {22, 30}, {33, 21}, {8, 25},
         {28, 7}, {24, 13}, {30, 0}, {5, 31}, {35, 11}, {31, 26},
         {2, 22}, {12, 18}, {19, 32}, {25, 16}, {1, 19}, {6, 1},
         {17, 6}, {9, 35}, {16, 8}, {29, 2}, {32, 34}, {3, 4},
         {0, 27}, {34, 29}, {4, 14}, {7, 15}, {21, 3}, {18, 24},
         {23, 5}, {27, 12}, {13, 20}, {11, 9}, {15, 23}, {20, 17}};
    private int[][] seq = new int[4][] { new int[6], new int[6], new int[6], new int[6] };
    private int[] pos = new int[2];
    private int answer;
    private bool hold;

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
        for (int i = 0; i < 2; i++)
        {
            seq[i] = new int[] { 0, 1, 2, 3, 4, 5 }.Shuffle();
            for (int j = 0; j < 6; j++)
                seq[i + 2][j] = seq[i][j];
        }
        Display();
        int[] sn = info.GetSerialNumber().Select(x => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".IndexOf(x.ToString())).ToArray();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int[] ind = new int[2];
                for (int k = 0; k < 2; k++)
                    ind[k] = grid[sn[i * (2 - j)] / 4, sn[i * (2 - j) + (2 * j) + 1] % 9, k];
                Debug.Log("WYMBGR#123456"[ind[0] + 6] + " <=> " + "WYMGBR#123456"[ind[1] + 6]);
                for (int k = 0; k < 2; k++)
                    ind[k] = ind[k] > 0 ? ind[k] - 1 : Array.IndexOf(seq[j + 2], -(ind[k] + 1));
                int t = seq[j + 2][ind[0]];
                seq[j + 2][ind[0]] = seq[j + 2][ind[1]];
                seq[j + 2][ind[1]] = t;
            }
        }
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                Debug.LogFormat("[Not Colour Flash #{0}] The {1} {2} order is: {3}", moduleID, i == 0 ? "displayed" : "transformed", j == 0 ? "word" : "colour", string.Join(", ", seq[j + (2 * i)].Select(x => new string[] { "Red", "Green", "Blue", "Magenta", "Yellow", "White" }[x]).ToArray()));
        bool[] cells = new bool[36] {
        seq.Any(x => x[0] == 0), Array.IndexOf(seq[2], 1) > Array.IndexOf(seq[2], 4), Array.IndexOf(seq[3], 3) > 3, seq.All(x => Array.IndexOf(x, 2) != 2), Array.IndexOf(seq[0], 0) == Array.IndexOf(seq[2], 0), seq.Any(x => x[Array.IndexOf(seq[1], 4)] == 5),
        new int[]{ 0, 5}.Any(x => x == Array.IndexOf(seq[3], 4)), seq.Select(x => Array.IndexOf(x, 1)).Distinct().Count() == 4, seq.Count(x => Array.IndexOf(x, 1) % 2 == 0) == 2, new int[]{ 2, 3}.Any(x => x == Array.IndexOf(seq[2], 3)), seq.Where((x, k) =>  k > 1).All(x => Array.IndexOf(x, 1) > 1), seq.Any(x => x[4] == 5),
        seq.Where((x, k) => k < 2).Any(x => Array.IndexOf(x, 2) > 2), seq.Count(x => Array.IndexOf(x, 5) % 3 == 0) > 1, seq.Count(x => Mathf.Abs(Array.IndexOf(x, 1) - Array.IndexOf(x, 3)) == 1) > 0, seq.Where((x, k) => k > 1).All(x => Array.IndexOf(x, 0) < Array.IndexOf(x, 3)), seq[1][5] % 2 == 0, Mathf.Abs(Array.IndexOf(seq[2], 2) - Array.IndexOf(seq[2], 3)) == 1,
        seq.Any(x => x[5] == 3), seq.Any(x => Array.IndexOf(x, 0) < Array.IndexOf(x, 1) && Array.IndexOf(x, 1) < Array.IndexOf(x, 2)), !seq[2].Where((x, k) => x == seq[3][k]).Any(), seq[0].Where((x, k) => x == seq[1][k]).Count() > 1, seq[0][5] < 3, seq.Any(x => x[1] == 3),
        seq[2].Where((x, k) => x == seq[3][k]).Count() > 1, seq.All(x => x[0] != 1), seq[1].Where((x, k) => x == seq[3][k]).Count() > 2, seq.Select(x => Array.IndexOf(x, 4)).Distinct().Count() == 2, seq.Any(x => Array.IndexOf(x, 0) % 2 == 1 && Array.IndexOf(x, 5) % 2 == 0), seq.Count(x => Array.IndexOf(x, 5) % 2 == 0) % 2 == 1,
        seq.Where((x, k) => k > 1).Any(x => Array.IndexOf(x, 2) == 5 - Array.IndexOf(x, 4)), seq.All(x => Array.IndexOf(x, 5) != 1), Enumerable.Range(0, 6).Any(x => seq[0][x] == seq[2][5 - x]), seq.Where((x, k) => k > 1).All(x => Mathf.Abs(Array.IndexOf(x, 2) - Array.IndexOf(x, 5)) == 1), Array.IndexOf(seq[0], 5) % 2 == Array.IndexOf(seq[2], 5) % 2 && Array.IndexOf(seq[1], 5) % 2 == Array.IndexOf(seq[3], 5) % 2, Enumerable.Range(0, 6).Count(x => Mathf.Abs( Array.IndexOf(seq[1], x) - Array.IndexOf(seq[3], x)) == 1) > 1};
        List<int> flow = new List<int> { info.GetPortCount() % 2 == info.GetPortPlateCount() % 2 ? (seq[1][0] * 6) + seq[0][0] : (seq[0][0] * 6) + seq[1][0]};
        for(int i = 0; i < 11; i++)
        {
            flow.Add(cells[flow[i]] ? flowchart[flow[i], 0] : flowchart[flow[i], 1]);
            if (flow.Where((x, k) => k <= i).Contains(flow[i + 1]))
                break;
        }
        answer = flow.Last();
        Debug.LogFormat("[Not Colour Flash #{0}] The cells in the flowchart were visited in this order: {1}", moduleID, string.Join(", ", flow.Select(x => "RGBMYW"[x / 6] + "/" + "RGBMYW"[x % 6]).ToArray()));
        Debug.LogFormat("[Not Colour Flash #{0}] The word \"{1}\" in the colour {2} should be submitted.", moduleID, new string[] { "Red", "Green", "Blue", "Magenta", "Yellow", "White" }[info.GetBatteryCount() % 3 > 0 ? answer / 6 : answer % 6], new string[] { "Red", "Green", "Blue", "Magenta", "Yellow", "White" }[info.GetBatteryCount() % 3 > 0 ? answer % 6 : answer / 6]);
        foreach (KMSelectable button in buttons)
        {
            bool b = button == buttons[0];
            button.OnInteract += delegate () { StartCoroutine(b ? "HoldY" : "HoldN"); return false; };
            button.OnInteractEnded += delegate () { Release(b); };
        }
    }

    private IEnumerator HoldY()
    {
        if (!moduleSolved)
        {
            buttons[0].AddInteractionPunch(0.2f);
            yield return new WaitForSeconds(0.4f);
            hold = true;
            Debug.LogFormat("[Not Colour Flash #{0}] Submitted \"{1}\" in {2}.", moduleID, new string[] { "Red", "Green", "Blue", "Magenta", "Yellow", "White" }[seq[0][pos[0]]], new string[] { "Red", "Green", "Blue", "Magenta", "Yellow", "White" }[seq[1][pos[1]]]);
            if (info.GetBatteryCount() % 3 == 0 ? (seq[1][pos[1]] * 6) + seq[0][pos[0]] == answer : (seq[0][pos[0]] * 6) + seq[1][pos[1]] == answer)
            {
                moduleSolved = true;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                module.HandlePass();
            }
            else
                module.HandleStrike();
        }
    }

    private IEnumerator HoldN()
    {
        if (!moduleSolved)
        {
            buttons[0].AddInteractionPunch(0.2f);
            yield return new WaitForSeconds(0.4f);
            hold = true;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
            pos[0] = 0;
            pos[1] = 0;
            Display();
        }
    }

    private void Release(bool b)
    {
        if (!moduleSolved)
        {
            StopAllCoroutines();
            if (hold)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
                buttons[b ? 0 : 1].AddInteractionPunch();
            }
            else
            {
                Audio.PlaySoundAtTransform("tick", transform);
                pos[b ? 0 : 1]++;
                pos[b ? 0 : 1] %= 6;
                Display();
            }
            hold = false;
        }
    }

    private void Display()
    {
        display.text = new string[] { "RED", "GREEN", "BLUE", "MAGENTA", "YELLOW", "WHITE" }[seq[0][pos[0]]];
        display.color = new Color[] { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 0, 1), new Color(1, 1, 0), new Color(1, 1, 1) }[seq[1][pos[1]]];
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cycle words/colours | !{0} <word> <colour> | !{0} submit | !{0} reset";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        if(command == "SUBMIT")
        {
            yield return null;
            buttons[0].OnInteract();
            yield return new WaitForSeconds(0.5f);
            buttons[0].OnInteractEnded();
            yield break;
        }
        else if (command == "RESET")
        {
            yield return null;
            buttons[1].OnInteract();
            yield return new WaitForSeconds(0.5f);
            buttons[1].OnInteractEnded();
            yield break;
        }
        string[] commands = command.Split(' ');
        if (commands.Length != 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        if (commands[0] == "CYCLE")
        {
            if (commands[1] == "WORDS")
            {
                for (int i = 0; i < 6; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    yield return null;
                    buttons[0].OnInteract();
                    yield return null;
                    buttons[0].OnInteractEnded();
                }
                yield break;
            }
            else if (commands[1] == "COLOURS")
            {
                for (int i = 0; i < 6; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    yield return null;
                    buttons[1].OnInteract();
                    yield return null;
                    buttons[1].OnInteractEnded();
                }
                yield break;
            }
            yield return "sendtochaterror!f " + commands[1] + " is an invalid cycling command.";
            yield break;
        }
        else
        {
            int[] taps = new int[2];
            for(int i = 0; i < 2; i++)
            {
                if (commands[i].Length == 1)
                    taps[i] = Array.IndexOf(new string[] { "R", "G", "B", "M", "Y", "W" }, commands[i]);
                else
                    taps[i] = Array.IndexOf(new string[] { "RED", "GREEN", "BLUE", "MAGENTA", "YELLOW", "WHITE" }, commands[i]);
                if(taps[i] < 0)
                {
                    yield return "sendtochaterror!f " + commands[i] + " cannot be displayed.";
                    yield break;
                }
            }
            for(int i = 0; i < 2; i++)
            {
                while(seq[i][pos[i]] != taps[i])
                {
                    yield return null;
                    buttons[i].OnInteract();
                    yield return null;
                    buttons[i].OnInteractEnded();
                }
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        int[] taps = info.GetBatteryCount() % 3 == 0 ? new int[2] { answer % 6, answer / 6 } : new int[2] { answer / 6, answer % 6};
        for (int i = 0; i < 2; i++)
        {
            while (seq[i][pos[i]] != taps[i])
            {
                yield return null;
                buttons[i].OnInteract();
                yield return null;
                buttons[i].OnInteractEnded();
            }
        }
        yield return null;
        buttons[0].OnInteract();
        yield return new WaitForSeconds(0.5f);
        buttons[0].OnInteractEnded();
    }
}
