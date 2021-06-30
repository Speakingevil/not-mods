using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UCFScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] buttons;
    public TextMesh display;

    private readonly string[] coldisps = new string[6] { "RED", "GREEN", "BLUE", "YELLOW", "MAGENTA", "WHITE" };
    private readonly Color[] cols = new Color[6] { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 1, 0), new Color(1, 0, 1), new Color(1, 1, 1) };
    private readonly string[,] textdisps = new string[10, 10] {
        { "IVORY", "CHERRY", "NAVY", "OLIVE", "LEMON", "MAHOGANY", "BONE", "SAGE", "MERLOT", "ROSE"},
        { "SEAFOAM", "HONEY", "FUCHSIA", "SCARLET", "PICKLE", "FLAMINGO", "DIJON", "PEACOCK", "PINK", "CHIFFON"},
        { "BLONDE", "BRICK", "LAPIS", "PINE", "GARNET", "COBALT", "COTTON", "CORAL", "CRIMSON", "EMERALD"},
        { "DENIM", "FANDANGO", "CONIFER", "SALT", "MAYA", "RUBY", "FOREST", "DOLLY", "PLUM", "EGGSHELL"},
        { "MANTIS", "PEARL", "CINNABAR", "FLAX", "SHAMROCK", "AZURE", "PHLOX", "FROST", "INDIGO", "TURMERIC"},
        { "ORCHID", "CERULEAN", "CANARY", "ALGAE", "LINEN", "MUSTARD", "SANGUINE", "EGGPLANT", "SPRING", "RUST"},
        { "CARDINAL", "JADE", "LACE", "ATOLL", "LILAC", "SWAMP", "MALIBU", "CLARET", "CHALK", "CREAM"},
        { "GIMBLET", "BOUQUET", "SIENNA", "MAUVE", "MARIGOLD", "COCONUT", "FERN", "LAGOON", "MINT", "JAVA"},
        { "CERAMIC", "MATISSE", "PEAR", "SNOW", "MILANO", "MULBERRY", "TACHA", "BURGUNDY", "CHENIN", "BLOSSOM"},
        { "PRUSSIA", "MANGO", "SPINEL", "CHINO", "LAUREL", "MARINER", "MARBLE", "CHERUB", "GLACIER", "TOPAZ"} };
    private readonly int[,] syncols = new int[10, 10] {
        { 5, 0, 2, 1, 3, 0, 5, 1, 0, 4},
        { 1, 3, 4, 0, 1, 4, 3, 2, 4, 5},
        { 3, 0, 2, 1, 0, 2, 5, 4, 0, 1},
        { 2, 4, 1, 5, 2, 0, 1, 3, 4, 5},
        { 1, 5, 0, 3, 1, 2, 4, 5, 2, 3},
        { 4, 2, 3, 1, 5, 3, 0, 4, 1, 0},
        { 0, 1, 5, 2, 4, 1, 2, 0, 5, 3},
        { 3, 4, 0, 4, 3, 5, 1, 2, 1, 2},
        { 5, 2, 1, 5, 0, 4, 3, 0, 3, 4},
        { 2, 3, 4, 3, 1, 2, 5, 4, 2, 0} };
    private readonly int[,] patopts = new int[7,8] {
        { 0, 1, 2, 3, 21, 22, 31, 32},
        { 1, 3, 10, 12, 21, 23, 30, 32},
        { 2, 3, 10, 13, 20, 23, 30, 31},
        { 0, 3, 11, 12, 21, 22, 30, 33},       
        { 0, 1, 2, 3, 10, 20, 30, 33},
        { 0, 3, 13, 23, 30, 31, 32, 33},
        { 1, 11, 12, 13, 20, 21, 22, 32}};
    private readonly int[,][] seqtable = new int[6, 6][] {
        { new int[3]{ 0, 1, 2}, new int[3]{ 5, 2, 4}, new int[3]{ 1, 3, 0}, new int[3]{ 4, 5, 1}, new int[3]{ 2, 0, 3}, new int[3]{ 3, 4, 5} },
        { new int[3]{ 3, 2, 4}, new int[3]{ 2, 5, 1}, new int[3]{ 4, 0, 2}, new int[3]{ 3, 1, 5}, new int[3]{ 1, 4, 0}, new int[3]{ 5, 1, 3} },
        { new int[3]{ 5, 3, 0}, new int[3]{ 0, 4, 3}, new int[3]{ 2, 1, 5}, new int[3]{ 1, 0, 2}, new int[3]{ 3, 2, 4}, new int[3]{ 4, 5, 1} },
        { new int[3]{ 2, 5, 3}, new int[3]{ 4, 0, 5}, new int[3]{ 3, 4, 1}, new int[3]{ 0, 3, 4}, new int[3]{ 5, 1, 0}, new int[3]{ 1, 2, 4} },
        { new int[3]{ 1, 0, 5}, new int[3]{ 3, 1, 0}, new int[3]{ 5, 2, 4}, new int[3]{ 2, 4, 3}, new int[3]{ 4, 3, 2}, new int[3]{ 0, 5, 2} },
        { new int[3]{ 4, 2, 1}, new int[3]{ 1, 3, 2}, new int[3]{ 0, 5, 3}, new int[3]{ 5, 2, 0}, new int[3]{ 0, 4, 5}, new int[3]{ 2, 0, 4} }};
    private int[][][] initseq = new int[2][][] { new int[2][] { new int[6] { 0, 1, 2, 3, 4, 5 }, new int[6] { 0, 1, 2, 3, 4, 5 } }, new int[2][] { new int[12] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5}, new int[12] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5} } };
    private int[][] colseq = new int[6][];
    private int[,] patterns = new int[6, 4];
    private int[] tarseq = new int[3];
    private int[] sub = new int[2];
    private int[][] accept = new int[6][];
    private int stage;
    private bool[] play = new bool[2];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                initseq[i][j] = initseq[i][j].Shuffle();
                Debug.LogFormat("[Uncolour Flash #{0}] The {1} of the {2} sequence are: {3}", moduleID, j == 0 ? "words" : "colours", i == 0 ? "Yes" : "No", string.Join(", ", initseq[i][j].Select(x => coldisps[x]).ToArray()));
            }
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 3; j++)
                patterns[i, j] = Random.Range(0, 7);
        int[] tarind = new int[2] { initseq[0][1][Array.IndexOf(initseq[0][0], 5)], initseq[0][0][5]};
        int[][] tarcell = new int[2][] { Enumerable.Range(0, 12).Where(x => initseq[1][1][x] == tarind[0]).Select(x => initseq[1][0][x]).ToArray(), Enumerable.Range(0, 12).Where(x => initseq[1][0][x] == tarind[1]).Select(x => initseq[1][1][x]).ToArray()};
        tarseq = seqtable[tarcell[0][0], tarcell[0][1]];
        Debug.LogFormat("[Uncolour Flash #{0}] The three target colour sequences, in order, are: {1}", moduleID, string.Join(", ", tarseq.Select(x => coldisps[x]).ToArray()));
        int c = Enumerable.Range(0, 6).Any(x => initseq[0][0][x] == initseq[0][1][x]) ? tarcell[1][0] : tarcell[1][1];
        int[] order = Enumerable.Range(0, 12).ToArray();
        Debug.LogFormat("[Uncolour Flash #{0}] The {1} list of rules is to be applied.", moduleID, coldisps[c]);
        for(int i = 0; i < 6; i++)
        {
            order = order.Shuffle();
            List<int> seq = new List<int> { };
            int tl = patterns[i, 0] * 10 + patterns[i, 1];
            for(int j = 0; j < 8; j++)
            {
                seq.Add(tl + patopts[patterns[i, 2], j]);
                seq[j] = T(seq[j] / 10, seq[j] % 10, (c * 6) + initseq[1][1][order[j]]);
            }
            int r = Random.Range(0, 100);
            for (int j = 8; j < 11; j++)
            {
                while (CheckDecoy(seq, r, i, order[j], c, tl))
                    r = Random.Range(0, 100);
                seq.Add(r);
            }
            int pos = 0;
            if(patterns[i, 0] < 3)
            {
                if (patterns[i, 1] < 3)
                    pos = 1;
                else if (patterns[i, 1] > 3)
                    pos = 2;
            }
            else if(patterns[i, 0] > 3)
            {
                if (patterns[i, 1] < 3)
                    pos = 3;
                else if (patterns[i, 1] > 3)
                    pos = 4;
            }
            patterns[i, 3] = pos;
            switch (patterns[i, 2])
            {
                case 1:
                    while (CheckDecoy(seq, r, i, order[11], c, tl) || syncols[r / 10, r % 10] != new int[5] { initseq[0][1][i], 0, 1, 2, 5 }[pos])
                        r = Random.Range(0, 100);
                    break;
                case 2:
                    while (CheckDecoy(seq, r, i, order[11], c, tl) || PatThree(r, pos, patterns[i, 0], patterns[i, 1], order[11], c))
                        r = Random.Range(0, 100);
                    break;
                case 6:
                    while (CheckDecoy(seq, r, i, order[11], c, tl) || textdisps[r / 10, r % 10].Length != pos + 4)
                        r = Random.Range(0, 100);
                    break;
                default:
                    while (CheckDecoy(seq, r, i, order[11], c, tl))
                        r = Random.Range(0, 100);
                    break;
            }
            seq.Add(r);
            colseq[i] = Enumerable.Range(0, 12).Select(x => seq[Array.IndexOf(order, x)]).ToArray();
            switch(patterns[i, 2])
            {
                case 0:
                    switch (pos)
                    {
                        case 0: accept[i] = Enumerable.Range(0, 12).Where(x => order[x] > 7).ToArray(); break;
                        case 1: accept[i] = Enumerable.Range(0, 12).Where(x => order[x] < 4).ToArray(); break;
                        case 2: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 2, 3, 5, 7 }.Contains(order[x])).ToArray(); break;
                        case 3: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 0, 1, 4, 6 }.Contains(order[x])).ToArray(); break;
                        default: accept[i] = Enumerable.Range(0, 12).Where(x => order[x] > 3 && order[x] < 8).ToArray(); break;
                    }
                    break;
                case 1:
                    accept[i] = Enumerable.Range(0, 12).Where(x => syncols[colseq[i][x] / 10, colseq[i][x] % 10] == new int[5] { initseq[0][1][i], 0, 1, 2, 5 }[pos]).ToArray();
                    break;
                case 2:
                    accept[i] = Enumerable.Range(0, 12).Where(x => !PatThree(colseq[i][x], pos, patterns[i, 0], patterns[i, 1], x, c)).ToArray();
                    break;
                case 3:
                    switch (pos)
                    {
                        case 0: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 2, 3, 4, 5 }.Contains(order[x])).ToArray(); break;
                        case 1: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 2, 3, 6, 7 }.Contains(order[x])).ToArray(); break;
                        case 2: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 0, 3, 5, 6 }.Contains(order[x])).ToArray(); break;
                        case 3: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 1, 2, 4, 7 }.Contains(order[x])).ToArray(); break;
                        default: accept[i] = Enumerable.Range(0, 12).Where(x => new int[4] { 0, 1, 4, 5 }.Contains(order[x])).ToArray(); break;
                    }
                    break;
                case 4:
                    accept[i] = new int[] { Enumerable.Range(0, 12).Where(x => order[x] < 7).ElementAt(pos) };
                    break;
                case 5:
                    accept[i] = new int[] { pos == 0 ? Enumerable.Range(0, 12).Where(x => order[x] < 8).ElementAt(7) : Enumerable.Range(0, 12).Where(x => order[x] > 7).ElementAt(pos - 1) };
                    break;
                default:
                    accept[i] = Enumerable.Range(0, 12).Where(x => textdisps[colseq[i][x] / 10, colseq[i][x] % 10].Length == pos + 4).ToArray();
                    break;
            }
        }
        LogSeq(tarseq[0]);
        foreach(KMSelectable button in buttons)
        {
            bool yes = Array.IndexOf(buttons, button) == 0;
            button.OnInteract = delegate () { Press(yes); return false; };
        }
    }

    private int T(int x, int y, int t)
    {
        switch (t)
        {
            case 0: return (x * 10) + ((y + 9) % 10);
            case 1: return (((x + 8) % 10) * 10) + y;
            case 2: return (x * 10) + ((y + Array.IndexOf(initseq[0][0], 2) + 1) % 10);
            case 3: return (((x + Array.IndexOf(initseq[0][0], 0) + 1) % 10) * 10) + y;
            case 7: return (x * 10) + ((y + (8 - Array.IndexOf(initseq[0][1], 1))) % 10);
            case 8: return (((x + 2) % 10) * 10) + y;
            case 9: return (x * 10) + ((y + (8 - Array.IndexOf(initseq[0][0], 5))) % 10);
            case 10: return (((x + 6) % 10) * 10) + y;
            case 11: return (x * 10) + ((y + 1) % 10);
            case 12: return (((x + (8 - Array.IndexOf(initseq[0][1], 3))) % 10) * 10) + y;
            case 13: return (x * 10) + ((y + Array.IndexOf(initseq[0][1], 5) + 1) % 10);
            case 14: return (((x + (8 - Array.IndexOf(initseq[0][1], 4))) % 10) * 10) + y;
            case 16: return (x * 10) + ((y + 8) % 10);
            case 17: return (x * 10) + ((y + Enumerable.Range(0, 12).Count(q => initseq[1][0][q] == initseq[1][1][q])) % 10);
            case 18: return (((x + 1) % 10) * 10) + y;
            case 20: return (x * 10) + ((y + (8 - Array.IndexOf(initseq[0][1], 2))) % 10);
            case 21: return (((x + Enumerable.Range(0, 12).Count(q => initseq[1][0][q] == initseq[1][1][q])) % 10) * 10) + y;
            case 23: return (((x + 5) % 10) * 10) + y;
            case 25: return (((x + Array.IndexOf(initseq[0][0], 3) + 1) % 10) * 10) + y;
            case 26: return (x * 10) + ((y + 7) % 10);
            case 27: return (((x + 4) % 10) * 10) + y;
            case 28: return (x * 10) + ((y + (8 - Array.IndexOf(initseq[0][0], 4))) % 10);
            case 29: return (x * 10) + ((y + (9 - Enumerable.Range(0, 12).Count(q => initseq[1][0][q] == initseq[1][1][q]))) % 10);
            case 30: return (x * 10) + ((y + Array.IndexOf(initseq[0][0], 1) + 1) % 10);
            case 31: return (((x + (9 - Enumerable.Range(0, 12).Count(q => initseq[1][0][q] == initseq[1][1][q]))) % 10) * 10) + y;
            case 33: return (((x + Array.IndexOf(initseq[0][1], 0) + 1) % 10) * 10) + y;
            case 34: return (x * 10) + ((y + 1) % 10);
            case 35: return (x * 10) + ((y + 5) % 10);
            case 4:
            case 6:
            case 19:
            case 32: return (x * 10) + (9 - y);
            default: return ((9 - x) * 10) + y;
        }
    }

    private bool CheckDecoy(List<int> s, int r, int i, int j, int c, int tl)
    {
        if (s.Contains(r))
            return true;
        int tr = T(r / 10, r % 10, (c * 6) + initseq[1][1][j]);
        if (Enumerable.Range(0, 8).Select(q => patopts[patterns[i, 2], q] + tl).Contains(tr))
            return true;
        return false;
    }

    private bool PatThree(int r, int p, int x, int y, int i, int c)
    {
        switch (p)
        {
            case 0: return (r / 9 - x) <= -1 || (r / 9 - x) >= 4 || (r % 9 - y) <= -1 || (r % 9 - y) >= 4;
            case 1: return T(r / 10, r % 10, (c * 6) + initseq[1][1][i]) / 10 != 9;
            case 2: return T(r / 10, r % 10, (c * 6) + initseq[1][1][i]) % 10 != 0;
            case 3: return T(r / 10, r % 10, (c * 6) + initseq[1][1][i]) % 10 != 9;
            default: return T(r / 10, r % 10, (c * 6) + initseq[1][1][i]) / 10 != 0;
        }
    }

    private void LogSeq(int t)
    {
        Debug.LogFormat("[Uncolour Flash #{0}] The {1} sequence is: {2}", moduleID, coldisps[t], string.Join(", ", colseq[t].Select(x => textdisps[x / 10, x % 10]).ToArray()));
        Debug.LogFormat("[Uncolour Flash #{0}] The sequence yields pattern {1}, located at the {2}{3}.", moduleID, "ABCDEFG"[patterns[t, 2]].ToString(), new string[] { "centre", "top-left", "top-right", "bottom-left", "bottom-right"}[patterns[t, 3]], patterns[t, 3] == 0 ? "" : " quadrant");     
        Debug.LogFormat("[Uncolour Flash #{0}] The {1} display{2} valid.", moduleID, string.Join(", ", accept[t].Select(x => (x + 1).ToString() + (x == 0 ? "st" : (x == 1 ? "nd" : (x == 2 ? "rd" : "th")))).ToArray()), accept[t].Length == 1 ? " is" : "s are");
    }

    private IEnumerator Colcycle()
    {
        play[1] = true;
        for(int i = 0; i < 12; i++)
        {
            sub[1] = i;
            display.text = textdisps[colseq[sub[0]][i] / 10, colseq[sub[0]][i] % 10];
            yield return new WaitForSeconds(0.4f);
            display.text = string.Empty;
            if(i < 11)
               yield return new WaitForSeconds(0.1f);
        }
        for(int i = 0; i < 2; i++)
        {
            play[i] = false;
            sub[i] = -1;
        }
    }

    private IEnumerator YesSeq()
    {
        play[0] = true;
        for(int i = 0; i < 6; i++)
        {
            sub[0] = initseq[0][1][i];
            display.text = coldisps[initseq[0][0][i]];
            display.color = cols[sub[0]];
            yield return new WaitForSeconds(1);
        }
        display.text = string.Empty;
        sub[0] = -1;
        play[0] = false;
    }

    private IEnumerator NoSeq()
    {
        play[1] = true;
        for (int i = 0; i < 12; i++)
        {
            display.text = coldisps[initseq[1][0][i]];
            display.color = cols[initseq[1][1][i]];
            yield return new WaitForSeconds(0.4f);
            display.text = string.Empty;
            if (i < 11)
                yield return new WaitForSeconds(0.1f);
        }
        play[1] = false;
    }

    private void Press(bool y)
    {
        if (!moduleSolved)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, buttons[y ? 0 : 1].transform);
            buttons[y ? 0 : 1].AddInteractionPunch(0.3f);
            if (y)
            {
                if (!play[0] && !play[1])
                    StartCoroutine("YesSeq");
                else if(play[0] && play[1])
                {
                    StopCoroutine("Colcycle");
                    for (int i = 0; i < 2; i++)
                        play[i] = false;
                    display.text = string.Empty;
                    Debug.LogFormat("[Uncolour Flash #{0}] Submitted the {1} display of the {2} sequence.", moduleID, (sub[1] + 1).ToString() + (sub[1] == 0 ? "st" : (sub[1] == 1 ? "nd" : (sub[1] == 2 ? "rd" : "th"))), coldisps[sub[0]]);
                    if (sub[0] == tarseq[stage] && accept[sub[0]].Contains(sub[1]))
                    {                       
                        if(stage < 2)
                        {
                            Audio.PlaySoundAtTransform("BeepStage", transform);
                            stage++;
                            LogSeq(tarseq[stage]);
                        }
                        else
                        {
                            Audio.PlaySoundAtTransform("BeepSolve", transform);
                            moduleSolved = true;
                            module.HandlePass();
                            display.text = coldisps[sub[0]];
                            display.color = cols[sub[0]];
                        }
                    }
                    else
                        module.HandleStrike();
                }
            }
            else
            {
                if(!play[0] && !play[1])
                    StartCoroutine("NoSeq");
                else if(play[0] && !play[1])
                {
                    StopCoroutine("YesSeq");
                    StartCoroutine("Colcycle");
                }
            }
        }
    }
}