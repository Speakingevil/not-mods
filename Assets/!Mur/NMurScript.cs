using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class NMurScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo edgework;
    public KMSelectable[] buttons;
    public TextMesh[] displays;

    private List<int>[] dispinfo = new List<int>[3] { new List<int> { }, new List<int> { }, new List<int> { } };
    private List<List<int[]>> turns = new List<List<int[]>> { };
    private int[] selection = new int[3];
    private int redroom;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnActivate = Activate;      
        dispinfo[0] = new int[6] { 0, 1, 2, 3, 4, 5 }.Shuffle().Where((x, k) => k < 5).ToList();
        dispinfo[1] = new int[6] { 0, 1, 2, 3, 4, 5 }.Shuffle().Where((x, k) => k < 5).ToList();
        dispinfo[2] = new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8}.Shuffle().Where((x, k) => k < 6).ToList();
        for (int i = 0; i < 3; i++)
            selection[i] = Random.Range(0, dispinfo[i].Count());
        redroom = Random.Range(0, 6);
        List<int[]> suspectlist = new List<int[]> { };
        for(int i = 0; i < 5; i++)
        {
            int[] suspect = new int[2];
            suspect[0] = dispinfo[2][i + (i >= redroom ? 1 : 0)];
            suspect[1] = dispinfo[1][i];
            suspectlist.Add(suspect);
        }
        turns.Add(suspectlist);
    }
    
    private void Activate()
    {
        bool[] edgeinfo = new bool[3] { edgework.IsIndicatorPresent(Indicator.TRN), edgework.GetPorts().Where(x => x == "StereoRCA").Count() > 2 || edgework.GetPortPlates().Where(x => x.Length == 1).Any(x => x[0] == "StereoRCA"), edgework.GetSerialNumberNumbers().Sum() > 14 };
        Debug.Log(edgework.GetSerialNumberNumbers().Sum());
        Debug.LogFormat("[Not Murder #{0}] Building layout:\n[Not Murder #{0}] {1}\n[Not Murder #{0}] {2}", moduleID, string.Join(" | ", dispinfo[2].Where((x, k) => k < 3).Select(x => new string[] { "Bal", "Bil", "Con", "Din", "Hal", "Kit", "Lib", "Lou", "Stu" }[x]).ToArray()), string.Join(" | ", dispinfo[2].Where((x, k) => k > 2).Select(x => new string[] { "Bal", "Bil", "Con", "Din", "Hal", "Kit", "Lib", "Lou", "Stu" }[x]).ToArray()));
        Debug.LogFormat("[Not Murder #{0}] Initial state:\n[Not Murder #{0}] {1}", moduleID, string.Join("\n[Not Murder #" + moduleID + "] ", turns[0].Select((x, k) => new string[] { "Miss Scarlett", "Colonel Mustard", "Reverand Green", "Mrs Peacock", "Professor Plum", "Mrs White" }[dispinfo[0][k]] + " was in the " + new string[] { "Ballroom", "Billiard Room", "Conservatory", "Dining Room", "Hall", "Kitchen", "Library", "Lounge", "Study" }[x[0]] + " with the " + new string[] { "Candlestick", "Dagger", "Lead Pipe", "Revolver", "Rope", "Spanner" }[x[1]] + ".").ToArray()));
        for(int i = 0; i < 5; i++)
        {
            int[][] suspectlist = new int[5][] { new int[2], new int[2], new int[2], new int[2], new int[2]};
            for (int j = 0; j < 5; j++)
            {
                int[] adj = new int[][] { new int[] { 1, 3 }, new int[] { 0, 2, 4 }, new int[] { 1, 5 }, new int[] { 0, 4 }, new int[] { 1, 3, 5 }, new int[] { 2, 4 } }[dispinfo[2].IndexOf(turns[i][j][0])].Select(x => dispinfo[2][x]).ToArray();
                switch (dispinfo[0][j])
                {
                    case 0:
                        int[] adjtos = new int[adj.Length];
                        for (int k = 0; k < adj.Length; k++)
                            adjtos[k] = turns[i].Select(x => x[0]).Count(x => x == adj[k]);
                        int max = adjtos.Max();
                        if (max == 0)
                            suspectlist[j][0] = dispinfo[2][(dispinfo[2].IndexOf(turns[i][j][0]) + 3) % 6];
                        else if (adjtos.Count(x => x == max) > 1)
                            suspectlist[j][0] = turns[i][j][0];
                        else
                            suspectlist[j][0] = adj[Array.IndexOf(adjtos, max)];
                        break;
                    case 1:
                        if(turns[i][j][1] == 3)
                        {
                            bool[] adjtom = new bool[adj.Length];
                            for(int k = 0; k < adj.Length; k++)
                                adjtom[k] = turns[i].Select(x => x[0]).Count(x => x == adj[k]) == 1;
                            if (adjtom.All(x => x == true))
                                suspectlist[j][0] = turns[i][j][0];
                            else if (adjtom.Count(x => x == false) == 1)
                                suspectlist[j][0] = adj[Array.IndexOf(adjtom, false)];
                            else if((turns[i][j][0] == dispinfo[2][1] && adjtom[Array.IndexOf(adj, dispinfo[2][2])]) || (turns[i][j][0] == dispinfo[2][4] && adjtom[Array.IndexOf(adj, dispinfo[2][3])]))
                                suspectlist[j][0] = dispinfo[2][(dispinfo[2].IndexOf(turns[i][j][0]) + 3) % 6];
                            else
                                suspectlist[j][0] = dispinfo[2][new int[] { 1, 2, 5, 0, 3, 4}[dispinfo[2].IndexOf(turns[i][j][0])]];
                        }
                        else
                        {
                            if (adj.Contains(dispinfo[2][1]))
                                suspectlist[j][0] = dispinfo[2][1];
                            else
                                suspectlist[j][0] = dispinfo[2][4];
                            for (int k = 0; k < adj.Length; k++)
                            {
                                if(turns[i].Where(x => adj[k] == x[0]).Any(x => x[1] == 3))
                                {
                                    suspectlist[j][0] = adj[k];
                                    break;
                                }
                            }
                        }
                        break;
                    case 2:
                        if (turns.Select(x => x[j][0]).Where((x, k) => x % 3 == 2 && k > 0).Count() % 2 == 0)
                            suspectlist[j][0] = dispinfo[2][new int[] { 1, 2, 5, 0, 3, 4 }[dispinfo[2].IndexOf(turns[i][j][0])]];
                        else
                            suspectlist[j][0] = dispinfo[2][new int[] { 3, 0, 1, 4, 5, 2 }[dispinfo[2].IndexOf(turns[i][j][0])]];
                        break;
                    case 3:
                        List<int> adjtob = new List<int> { };
                        for (int k = 0; k < adj.Length; k++)
                            if (!turns.Select(x => x[j][0]).Contains(adj[k]))
                                adjtob.Add(adj[k]);
                        if (adjtob.Count() == 0)
                            suspectlist[j][0] = dispinfo[2][new int[] { 3, 0, 1, 4, 5, 2 }[dispinfo[2].IndexOf(turns[i][j][0])]];
                        else
                            suspectlist[j][0] = adjtob.Min();
                        break;
                    case 4:
                        if (i > 0 && turns[i][j][0] > 5 && turns[i][j][0] != turns[i - 1][j][0])
                            suspectlist[j][0] = turns[i][j][0];
                        else
                        {
                            int[] adjtop = new int[adj.Length];
                            for (int k = 0; k < adj.Length; k++)
                                adjtop[k] = turns[i].Select(x => x[0]).Count(x => x == adj[k]);
                            if (adjtop.Count(x => x == 0) == 1)
                                suspectlist[j][0] = adj[Array.IndexOf(adjtop, 0)];
                            else
                                suspectlist[j][0] = adj.Min();
                        }
                        break;
                    default:
                        List<int> adjtow = new List<int> { };
                        for (int k = 0; k < adj.Length; k++)
                            if (!turns[i].Where(x => adj[k] == x[0]).Any(x => x[1] == 1 || x[1] == 3))
                                adjtow.Add(adj[k]);
                        if (adjtow.Count() == 0)
                            suspectlist[j][0] = turns[i][j][0];
                        else if (adjtow.Count() == 1)
                            suspectlist[j][0] = adjtow[0];
                        else if (adjtow.Count(x => x > 2) == 1)
                            suspectlist[j][0] = adjtow.Max();
                        else if (adjtow.Count(x => x > 2) == 0)
                            suspectlist[j][0] = adjtow.Min();
                        else
                            suspectlist[j][0] = adjtow.Where(x => x > 2).Min();
                        break;
                }
            }
            for (int j = 0; j < 5; j++)
                suspectlist[j][1] = turns[i][j][1];
            for(int j = 0; j < 6; j++)
            {
                List<int> present = new List<int> { };
                List<int> items = new List<int> { };
                for(int k = 0; k < 5; k++)
                    if (suspectlist[k][0] == dispinfo[2][j])
                    {
                        present.Add(k);
                        items.Add(suspectlist[k][1]);
                    }
                if(present.Count() == 2)
                {
                    suspectlist[present[0]][1] = items[1];
                    suspectlist[present[1]][1] = items[0];
                }
            }
            turns.Add(suspectlist.ToList());
            Debug.LogFormat("[Not Murder #{0}] Turn {2}:\n[Not Murder #{0}] {1}", moduleID, string.Join("\n[Not Murder #" + moduleID + "] ", turns[i + 1].Select((x, k) => new string[] { "Miss Scarlett", "Colonel Mustard", "Reverand Green", "Mrs Peacock", "Professor Plum", "Mrs White" }[dispinfo[0][k]] + " was in the " + new string[] { "Ballroom", "Billiard Room", "Conservatory", "Dining Room", "Hall", "Kitchen", "Library", "Lounge", "Study" }[x[0]] + " with the " + new string[] { "Candlestick", "Dagger", "Lead Pipe", "Revolver", "Rope", "Spanner" }[x[1]] + ".").ToArray()), i + 1);
        }
        displays[0].text = new string[] { "MISS SCARLETT", "COLONEL MUSTARD", "REVERAND GREEN", "MRS PEACOCK", "PROFESSOR PLUM", "MRS WHITE" }[dispinfo[0][selection[0]]];
        displays[0].color = new Color[] { new Color(1f, 0.05f, 0.05f), new Color(0.85f, 0.85f, 0.15f), new Color(0.1f, 0.8f, 0.1f), new Color(0.3f, 0.3f, 1f), new Color(0.8f, 0.15f, 0.55f), new Color(1f, 1f, 1f) }[dispinfo[0][selection[0]]];
        displays[1].text = new string[] { "CANDLESTICK", "DAGGER", "LEAD PIPE", "REVOLVER", "ROPE", "SPANNER" }[dispinfo[1][selection[1]]];
        displays[1].color = new Color(0.8f, 0.8f, 0.8f);
        displays[2].text = new string[] { "BALLROOM", "BILLIARD ROOM", "CONSERVATORY", "DINING ROOM", "HALL", "KITCHEN", "LIBRARY", "LOUNGE", "STUDY" }[dispinfo[2][selection[2]]];
        displays[2].color = selection[2] == redroom ? new Color(1f, 0.05f, 0.05f) : new Color(0.8f, 0.8f, 0.8f);
        foreach (KMSelectable button in buttons)
        {
            int b = Array.IndexOf(buttons, button);
            button.OnInteract += delegate () { ButtonPress(b, edgeinfo[b / 2]); return false; };
        }
    }

    private void ButtonPress(int b, bool d)
    {
        if (!moduleSolved)
        {
            buttons[b].AddInteractionPunch(b == 6 ? 1 : 0.5f);
            if (b == 6)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[6].transform);
                int[] submission = new int[2];
                for (int i = 1; i < 3; i++)
                    submission[i - 1] = dispinfo[i][selection[i]];
                if (turns[5][selection[0]].SequenceEqual(submission.Reverse()))
                {
                    Debug.LogFormat("[Not Murder #{0}] Accusation: {1} left with the {3} through the {2}. Correct.", moduleID, new string[] { "Miss Scarlett", "Colonel Mustard", "Reverand Green", "Mrs Peacock", "Professor Plum", "Mrs White" }[dispinfo[0][selection[0]]], new string[] { "Ballroom", "Billiard Room", "Conservatory", "Dining Room", "Hall", "Kitchen", "Library", "Lounge", "Study" }[submission[1]], new string[] { "Candlestick", "Dagger", "Lead Pipe", "Revolver", "Rope", "Spanner" }[submission[0]]);
                    if (dispinfo[0].Count() == 1)
                    {
                        moduleSolved = true;
                        module.HandlePass();
                    }
                    else
                    {
                        turns[5].RemoveAt(selection[0]);
                        for (int i = 0; i < 2; i++)
                        {
                            dispinfo[i].RemoveAt(selection[i]);
                            selection[i] %= dispinfo[i].Count();
                        }
                        displays[0].text = new string[] { "MISS SCARLETT", "COLONEL MUSTARD", "REVERAND GREEN", "MRS PEACOCK", "PROFESSOR PLUM", "MRS WHITE" }[dispinfo[0][selection[0]]];
                        displays[0].color = new Color[] { new Color(1f, 0.05f, 0.05f), new Color(0.85f, 0.85f, 0.15f), new Color(0.1f, 0.8f, 0.1f), new Color(0.3f, 0.3f, 1f), new Color(0.8f, 0.15f, 0.55f), new Color(1f, 1f, 1f) }[dispinfo[0][selection[0]]];
                        displays[1].text = new string[] { "CANDLESTICK", "DAGGER", "LEAD PIPE", "REVOLVER", "ROPE", "SPANNER" }[dispinfo[1][selection[1]]];
                    }
                }
                else
                {
                    Debug.LogFormat("[Not Murder #{0}] Accusation: {1} left with the {3} through the {2}. Incorrect.", moduleID, new string[] { "Miss Scarlett", "Colonel Mustard", "Reverand Green", "Mrs Peacock", "Professor Plum", "Mrs White" }[dispinfo[0][selection[0]]], new string[] { "Ballroom", "Billiard Room", "Conservatory", "Dining Room", "Hall", "Kitchen", "Library", "Lounge", "Study" }[submission[1]], new string[] { "Candlestick", "Dagger", "Lead Pipe", "Revolver", "Rope", "Spanner" }[submission[0]]);
                    module.HandleStrike();
                }
            }
            else
            {
                selection[b / 2] = (selection[b / 2] + (((b % 2 == 0) ^ d) ? (dispinfo[b / 2].Count() - 1) : 1)) % dispinfo[b / 2].Count();
                Audio.PlayGameSoundAtTransform(dispinfo[0].Count() > 4 && selection[b / 2] == 0 ? KMSoundOverride.SoundEffect.ButtonPress : KMSoundOverride.SoundEffect.BigButtonPress, buttons[b].transform);
                switch(b / 2)
                {
                    case 0:
                        displays[0].text = new string[] { "MISS SCARLETT", "COLONEL MUSTARD", "REVERAND GREEN", "MRS PEACOCK", "PROFESSOR PLUM", "MRS WHITE" }[dispinfo[0][selection[0]]];
                        displays[0].color = new Color[] { new Color (1f, 0.05f, 0.05f), new Color(0.85f, 0.85f, 0.15f), new Color (0.1f, 0.8f, 0.1f), new Color(0.3f, 0.3f, 1f), new Color(0.8f, 0.15f, 0.55f), new Color (1f, 1f, 1f) }[dispinfo[0][selection[0]]];
                        break;
                    case 1:
                        displays[1].text = new string[] { "CANDLESTICK", "DAGGER", "LEAD PIPE", "REVOLVER", "ROPE", "SPANNER" }[dispinfo[1][selection[1]]];
                        break;
                    case 2:
                        displays[2].text = new string[] { "BALLROOM", "BILLIARD ROOM", "CONSERVATORY", "DINING ROOM", "HALL", "KITCHEN", "LIBRARY", "LOUNGE", "STUDY" }[dispinfo[2][selection[2]]];
                        displays[2].color = selection[2] == redroom ? new Color(1f, 0.05f, 0.05f) : new Color(0.8f, 0.8f, 0.8f);
                        break;
                }
            }
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cycle suspects/items/rooms left/right | !{0} select <suspect/item/room> (Exclude the titles of suspects, enter \"Pipe\" for the item \"Lead Pipe\", and exclude the word \"Room\" from two word room names) | !{0} accuse";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if(command == "accuse")
        {
            yield return null;
            buttons[6].OnInteract();
        }
        else
        {
            string[] commands = command.Split(' ');
            if(commands[0] == "cycle")
            {
                if(commands.Length != 3)
                {
                    yield return "sendtochaterror!f Invalid command length.";
                    yield break;
                }
                int press = 0;
                if(!new string[] { "suspects", "items", "rooms" }.Contains(commands[1]))
                {
                    yield return "sendtochaterror!f " + commands[1] + " is not a valid display.";
                    yield break;
                }
                press = 2 * Array.IndexOf(new string[] { "suspects", "items", "rooms" }, commands[1]);
                if (commands[2] != "left" && commands[2] != "right")
                {
                    yield return "sendtochaterror!f " + commands[2] + " is not a valid direction.";
                    yield break;
                }
                if (commands[2] == "right")
                    press++;
                for(int i = 0; i < dispinfo[press / 2].Count(); i++)
                {
                    yield return null;
                    buttons[press].OnInteract();
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else if(commands[0] == "select")
            {
                string[][] checknames = new string[3][] { new string[] { "scarlett", "mustard", "green", "peacock", "plum", "white" }, new string[] { "candlestick", "dagger", "pipe", "revolver", "rope", "spanner" }, new string[] { "ballroom", "billiard", "conservatory", "dining", "hall", "kitchen", "library", "lounge", "study" } };
                int[] index = new int[3] { -1, -1, -1 };
                for(int i = 1; i < commands.Length; i++)
                {
                    bool check = false;
                    for (int j = 0; j < 3; j++)
                    {
                        if (checknames[j].Contains(commands[i]))
                        {
                            if (index[j] == -1)
                            {
                                check = true;
                                index[j] = Array.IndexOf(checknames[j], commands[i]);
                                break;
                            }
                            else
                            {
                                yield return "sendtochaterror!f Cannot select multiple" + new string[] { "suspect", "item", "room" }[j] + "s.";
                                yield break;
                            }
                        }
                    }
                    if (!check)
                    {
                        yield return "sendtochaterror!f " + commands[i] + " is not a valid suspect, item, or room.";
                        yield break;
                    }
                }
                for (int j = 0; j < 3; j++)
                    if (index[j] != -1 && !dispinfo[j].Contains(index[j]))
                    {
                        yield return "sendtochaterror!f Selected " + new string[] { "suspect", "item", "room" }[j] + " is not present on the module.";
                        yield break;
                    }
                for(int i = 0; i < 3; i++)
                {
                    while(index[i] != -1 && dispinfo[i][selection[i]] != index[i])
                    {
                        yield return null;
                        buttons[i * 2].OnInteract();
                    }
                }
            }
            else
            {
                yield return "sendtochaterror!f Invalid Command.";
                yield break;
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        int n = dispinfo[0].Count();
        for (int i = 0; i < n; i++)
        {
            while (turns[5][selection[0]][0] != dispinfo[2][selection[2]])
            {
                yield return null;
                buttons[5].OnInteract();
            }
            while (turns[5][selection[0]][1] != dispinfo[1][selection[1]])
            {
                yield return null;
                buttons[3].OnInteract();
            }
            yield return null;
            buttons[6].OnInteract();
        }
    }
}
