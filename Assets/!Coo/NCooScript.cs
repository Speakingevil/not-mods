using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NCooScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Renderer[] screen;
    public Material[] screenbg;
    public Material[] shapes;
    public TextMesh disptext;
    public GameObject[] obj;

    private int[,] dogrid = new int[9, 9]
    { { 60, 2, 15, 57, 36, 83, 48, 71, 24},
      { 88, 46, 31, 70, 22, 64, 7, 55, 13},
      { 74, 27, 53, 5, 41, 18, 86, 30, 62},
      { 52, 10, 4, 43, 85, 37, 61, 28, 76},
      { 33, 65, 78, 21, 0, 56, 12, 44, 87},
      { 47, 81, 26, 68, 14, 72, 50, 3, 35},
      { 6, 38, 42, 84, 63, 20, 75, 17, 51},
      { 25, 73, 67, 16, 58, 1, 34, 82, 40},
      { 11, 54, 80, 32, 77, 45, 23, 66, 8} };
    private bool[,] openspace = new bool[9,9];
    private bool[,] vertices = new bool[9,9];
    private List<int>[] seq = new List<int>[2] { new List<int> { }, new List<int> { } };
    private List<string> disp = new List<string> { };
    private bool anim;
    private bool[] gud = new bool[3];
    private int targetshape;
    private int[] pos = new int[4];
    private int[] dir = new int[4];
    private bool[] back = new bool[4];
    private int[][] glitched = new int[2][] { new int[0], new int[0]};
    private bool[] glitching = new bool[2];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private bool IsoRight(int[] x, int[] y, int[] z)
    {
        int a = (x[0] - y[0]) * (x[0] - y[0]) + (x[1] - y[1]) * (x[1] - y[1]);
        int b = (y[0] - z[0]) * (y[0] - z[0]) + (y[1] - z[1]) * (y[1] - z[1]);
        int c = (x[0] - z[0]) * (x[0] - z[0]) + (x[1] - z[1]) * (x[1] - z[1]);
        if (a == 0 || b == 0 || c == 0)
            return false;
        return (a == b && a + b == c) || (b == c && b + c == a) || (c == a && c + a == b);
    }

    private bool Colinear(int[] x, int[] y, int[] z)
    {
        int dxy = Mathf.Abs(x[0] - y[0]) + Mathf.Abs(x[1] - y[1]);
        int dyz = Mathf.Abs(y[0] - z[0]) + Mathf.Abs(y[1] - z[1]);
        int dxz = Mathf.Abs(x[0] - z[0]) + Mathf.Abs(x[1] - z[1]);
        return dxz == dxy + dyz || dxy == dyz + dxz || dyz == dxz + dxy;
    }

    private bool Centre(int[] x, int[] y, int[] z, int c0, int c1)
    {
        int dx = Mathf.Abs(x[0] - c0) + Mathf.Abs(x[1] - c1);
        int dy = Mathf.Abs(y[0] - c0) + Mathf.Abs(y[1] - c1);
        int dz = Mathf.Abs(z[0] - c0) + Mathf.Abs(z[1] - c1);
        return dx == dy && dx == dz;
    }

    private float Querp(float a, float b, float c, float t)
    {
        return (a * (1 - t) * (1 - t)) + (2 * b * t * (1 - t)) + (c * t * t);
    }

    private string Rewrite(int input, int scr)
    {
        string d = string.Empty;
        switch (scr)
        {
            default: d = string.Format("[{0},{1}]", input % 9, input / 9); break;
            case 1: d = "ABCDEFGHI"[input % 9].ToString() + ((input / 9) + 1); break;
            case 2: d = string.Format("<{0}, {1}>", input / 9, input % 9); break;
            case 3: d = string.Format("{0}, {1}", (input / 9) + 1, (input % 9) + 1); break;
            case 4: d = string.Format("({0},{1})", input % 9, 8 - (input / 9)); break;
            case 5: d = "ABCDEFGHI"[input % 9].ToString() + "-" + (9 - (input / 9)); break;
            case 6: d = string.Format("\"{0}, {1}\"", 8 - (input / 9), input % 9); break;
            case 7: d = string.Format("{0}/{1}", 9 - (input / 9), (input % 9) + 1); break;
            case 8: d = "[" + input.ToString() + "]"; break;
            case 9: d = (input + 1).ToString() + ((input / 10) == 1 ? "th" : new string[] {"st", "nd", "rd", "th", "th", "th", "th", "th", "th", "th"}[input % 10]); break;
            case 10: d = "#" + (((8 - (input / 9)) * 9) + (input % 9) + 1).ToString(); break;
            case 11:
                int c = ((8 - (input % 9)) * 9) + (input / 9) + 1;
                d = "  二三四五六七八九"[c / 10] + (c > 9 ? "十" : "") + " 一二三四五六七八九"[c % 10];
                d.Replace(" ", ""); break;
            case 12:
                int[] focus = new int[2] { (input / 9) - 4, (input % 9) - 4 };
                if(focus[0] == 0)
                {
                    if (focus[1] == 0) d = "centre";
                    else d = string.Format("{0} {1}", Mathf.Abs(focus[1]), focus[1] < 0 ? "west" : "east");
                }
                else
                {
                    d = string.Format("{0} {1}", Mathf.Abs(focus[0]), focus[0] < 0 ? "north" : "south");
                    if (focus[1] != 0)
                        d += ", " + string.Format("{0} {1}", Mathf.Abs(focus[1]), focus[1] < 0 ? "west" : "east");
                }
                break;
            case 13:
                if ((input / 9) % 3 != 1 || input % 3 != 1)
                {
                    d = "N#S"[(input / 9) % 3].ToString() + "W#E"[input % 3].ToString();
                    d = d.Replace("#", "");
                    d += " from ";
                }
                string e = "centre";
                if (input / 27 != 1 || (input % 9) / 3 != 1)
                {
                    e = "N#S"[input / 27].ToString() + "W#E"[(input % 9) / 3].ToString();
                    e = e.Replace("#", "");
                }
                d += e;
                break;
        }
        return d;
    }

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        int[] failsafe = new int[3]; 
        foreach (GameObject o in obj)
            o.SetActive(false);
        int[][] tverts = new int[2][] { new int[2], new int[2]};
        int[] alt;
        while (true)
        {          
            tverts[0][0] = UnityEngine.Random.Range(1, 8);
            tverts[0][1] = UnityEngine.Random.Range(1, 8);
            tverts[1][0] = UnityEngine.Random.Range(1, 8);
            tverts[1][1] = UnityEngine.Random.Range(1, 8);
            while (tverts[0][0] == tverts[1][0] && tverts[0][1] == tverts[1][1])
            {
                tverts[0][0] = UnityEngine.Random.Range(1, 8);
                tverts[0][1] = UnityEngine.Random.Range(1, 8);
            }
            alt = Enumerable.Range(0, 81).Select(x => new int[] { x / 9, x % 9 }).Where(x => !Colinear(x, tverts[0], tverts[1]) && IsoRight(x, tverts[0], tverts[1])).Select(x => x[0] * 9 + x[1]).ToArray();
            if (alt.Length > 2)
                break;
            if (failsafe[0] > 999)
                break;
            failsafe[0]++;
        }
        for (int i = 0; i < 2; i++)
        {
            vertices[tverts[i][0], tverts[i][1]] = true;
            openspace[tverts[i][0], tverts[i][1]] = true;
            seq[0].Add((tverts[i][0] * 9) + tverts[i][1]);
        }
        int pick = 0;
        while (true)
        {
            pick = UnityEngine.Random.Range(0, alt.Length);
            int[] acoord = new int[2] { alt[pick] / 9, alt[pick] % 9 };
            int[] centre = new int[2] { -1, 0 };
            for (int i = 0; i < 18; i++)
            {
                for (int j = 0; j < 18; j++)
                    if (Centre(tverts[0].Select(x => x * 2).ToArray(), tverts[1].Select(x => x * 2).ToArray(), acoord.Select(x => x * 2).ToArray(), i, j))
                    {
                        centre[0] = i;
                        centre[1] = j;
                        goto steptwo;
                    }
            }
            continue;
        steptwo:;
            int[] fourth = new int[2];
            for (int i = 0; i < 2; i++)
                fourth[i] = (2 * centre[i]) - tverts[0][i] - tverts[1][i] - acoord[i];
            if(fourth[0] >= 0 && fourth[0] <= 8 && fourth[1] >= 0 && fourth[1] <= 8)
            {
                targetshape = (fourth[0] * 9) + fourth[1];
                break;
            }
            if (failsafe[1] > 999)
                break;
            failsafe[1]++;
        }
        for (int i = 0; i < alt.Length; i++)
        {
            if(i == pick)
            {
                vertices[alt[i] / 9, alt[i] % 9] = true;
                seq[0].Add(alt[i]);
            }
            openspace[alt[i] / 9, alt[i] % 9] = true;
        }
        while (true)
        {
            for (int i = 0; i < 81; i++)
            {
                if (!openspace[i / 9, i % 9])
                {
                    for(int j = 0; j < seq[0].Count(); j++)
                        for(int k = 0; k < seq[0].Count(); k++)
                        {
                            if(j != k)
                            {
                                int[] check = new int[2] { i / 9, i % 9 };
                                int[] alpha = new int[2] { seq[0][j] / 9, seq[0][j] % 9 };
                                int[] beta = new int[2] { seq[0][k] / 9, seq[0][k] % 9 };
                                if(IsoRight(check, alpha, beta))
                                {
                                    openspace[i / 9, i % 9] = true;
                                    goto next;
                                }
                            }
                        }
                }
            next:;
            }
            if (Enumerable.Range(0, 81).All(x => openspace[x / 9, x % 9] == true))
                break;
            int t = UnityEngine.Random.Range(0, 81);
            while (openspace[t / 9, t % 9])
                t = UnityEngine.Random.Range(0, 81);
            vertices[t / 9, t % 9] = true;
            seq[0].Add(t);
            openspace[t / 9, t % 9] = true;
            if (failsafe[2] > 999)
                break;
            failsafe[2]++;
        }
        for (int i = 0; i < seq[0].Count(); i++)
            seq[1].Add(seq[0][i]);
        seq[1].Shuffle();
        for (int i = 0; i < seq[1].Count(); i++)
        {
            int x = UnityEngine.Random.Range(0, i == 0 ? 11 : 14);
            disp.Add(Rewrite(seq[1][i], x));
        }
        disptext.text = disp[0];
        Debug.Log(string.Join(", ", failsafe.Select(i => i.ToString()).ToArray()));
        Debug.LogFormat("[Not Coordinates #{0}] The displays were: {1}", moduleID, string.Join(" | ", disp.ToArray()));
        string log = string.Empty;
        for (int i = 0; i < 9; i++)
            log += "[Not Coordinates #" + moduleID + "] " + string.Join(" ", Enumerable.Range(0, 9).Select(x => vertices[i, x] ? "\x25cf" : "\x25cb").ToArray()) + "\n";
        Debug.LogFormat("[Not Coordinates #{0}] The displays give the following coordinates:\n{1}", moduleID, log);
        log = string.Empty;
        for (int i = 0; i < 9; i++)
            log += "[Not Coordinates #" + moduleID + "] " + string.Join(" ", Enumerable.Range(0, 9).Select(x => (i * 9) + x == targetshape ? "\x25ce" : (seq[0].Where((p, q) => q < 3).Contains((i * 9) + x) ? "\x25cf" : "\x25cb")).ToArray()) + "\n";
        Debug.LogFormat("[Not Coordinates #{0}] The square can be found in this location:\n{1}", moduleID, log);
        Debug.LogFormat("[Not Coordinates #{0}] The displayed vertices of the square are: {1}", moduleID, string.Join(" | ", disp.Where((x, k) => seq[0].IndexOf(seq[1][k]) < 3).ToArray()));
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate () { Press(b, gud.All(x => x == true)); return false; };
        }
    }

    private void Press(int b, bool s)
    {
        if(!moduleSolved && !anim)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[b].transform);
            buttons[b].AddInteractionPunch(0.5f);
            StartCoroutine(Push(b));
            if (s)
            {
                if(b == 1)
                {
                    string m = "[Not Coordinates #" + moduleID + "] " + new string[] { "Flat", "Round", "Point", "Ticket", "Bumps", "Ribbon", "Sawtooth", "Track", "Hammer"}[dogrid[(pos[0] * 3) + pos[2], (pos[1] * 3) + pos[3]] / 10] + "-" + new string[] { "Flat", "Round", "Point", "Ticket", "Bumps", "Ribbon", "Sawtooth", "Track", "Twist" }[dogrid[(pos[0] * 3) + pos[2], (pos[1] * 3) + pos[3]] % 10] + " submitted.";
                    if (((pos[0] * 3) + pos[2]) * 9 + (pos[1] * 3) + pos[3] == targetshape)
                    {
                        moduleSolved = true;
                        module.HandlePass();
                        for (int i = 0; i < 2; i++)
                        {
                            glitching[i] = false;
                            obj[i + 1].SetActive(false);
                        }
                        screen[0].material = screenbg[0];
                        disptext.fontSize = 128;
                        disptext.text = "00";
                        Debug.Log(m + " Correct.");
                        Audio.PlaySoundAtTransform("DoubleOSolve", transform);
                    }
                    else
                    {
                        Debug.Log(m + " Incorrect.");
                        module.HandleStrike();
                    }
                }
                else
                {
                    int n = b + (int)info.GetTime() % 2;
                    pos[dir[n]] += back[n] ? 2 : 1; pos[dir[n]] %= 3;
                    Audio.PlaySoundAtTransform("DoubleOPress" + (b + 1 + (int)info.GetTime() % 2).ToString(), transform);
                    UpdateShape();
                }
            }
            else
            {
                if(b == 1)
                {
                    int c = seq[0].IndexOf(seq[1][pos[0]]);
                    string m = "[Not Coordinates #" + moduleID +"] " + disp[pos[0]] + " submitted. ";
                    if (c < 3)
                    {
                        if (!gud[c])
                        {
                            gud[c] = true;
                            switch (gud.Count(x => x == false))
                            {
                                case 2: Debug.Log(m + "Two to go."); break;
                                case 1: Debug.Log(m + "One to go."); break;
                                case 0: Debug.Log(m + "Begin Phase 2."); break;
                            }
                        }
                        if (gud.All(x => x == true))
                        {
                            disptext.text = string.Empty;
                            screen[0].material = screenbg[1];
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, buttons[1].transform);
                            Debug.Log("[Not Coordinates #" + moduleID + "] The fourth vertex corresponds to the shape: " + new string[] { "Flat", "Round", "Point", "Ticket", "Bumps", "Ribbon", "Sawtooth", "Track", "Hammer" }[dogrid[targetshape / 9, targetshape % 9] / 10] + "-" + new string[] { "Flat", "Round", "Point", "Ticket", "Bumps", "Ribbon", "Sawtooth", "Track", "Twist" }[dogrid[targetshape / 9, targetshape % 9] % 10]);
                            dir = new int[] { 0, 1, 2, 3 }.Shuffle();
                            for (int i = 0; i < 4; i++)
                            {
                                pos[i] = UnityEngine.Random.Range(0, 3);
                                back[i] = UnityEngine.Random.Range(0, 2) == 1;
                                Debug.LogFormat("[Not Coordinates #{0}] Pressing {1} on an {2} second moves {3}{4}", moduleID, new string[] { "left", "right"}[i / 2], new string[] { "even", "odd"}[i % 2], new string[] { "large", "small"}[dir[i] / 2], new string[] { "down", "right", "up", "left"}[dir[i] % 2 + (back[i] ? 2 : 0)]);
                            }
                            Debug.Log(string.Join(", ", dir.Select(x => (x + 1).ToString()).ToArray()));
                            Debug.Log(string.Join(", ", new int[] { 1, 2, 3, 4}.Where(x => back[x - 1]).Select(x => x.ToString()).ToArray()));
                            for (int i = 0; i < 2; i++)
                            {
                                obj[i + 1].SetActive(true);
                                for (int j = 0; j < 3; j++)
                                {
                                    int[][] g = new int[2][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.Shuffle(), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.Shuffle() };
                                    glitched[i] = glitched[i].Concat(g[0].Select((x, k) => (x * 9) + g[1][k])).ToArray();
                                }
                            }
                            UpdateShape();
                        }
                    }
                    else
                    {
                        module.HandleStrike();
                        Debug.Log(m + "Incorrect.");
                    }
                }
                else
                {
                    pos[0] += b == 0 ? disp.Count() - 1 : 1;
                    pos[0] %= disp.Count();
                    StartCoroutine(UpdateCoord());
                }
            }
        }
    }

    private IEnumerator Push(int b)
    {
        anim = true;
        Vector3 reset = buttons[b].transform.localPosition;
        Vector3 startpos = buttons[b].transform.localPosition;
        float t = 0;
        while(t < 0.3f)
        {
            buttons[b].transform.localPosition = new Vector3(startpos.x, Querp(startpos.y, startpos.y / 3, startpos.y, t / 0.3f), startpos.z);
            yield return null;
            t += Time.deltaTime;
        }
        buttons[b].transform.localPosition = reset;
        anim = false;
    }

    private IEnumerator UpdateCoord()
    {
        var c = disptext.color;
        float t = 0;
        while(t < 0.15f)
        {
            yield return null;
            t += Time.deltaTime;
            disptext.color = new Color(c.r, c.g, c.b, 1 - (t / 0.15f));
        }
        disptext.text = disp[pos[0]];
        disptext.fontSize = disp[pos[0]].Length > 6 ? 64 : 128;
        while (t > 0f)
        {
            yield return null;
            t -= Time.deltaTime;
            disptext.color = new Color(c.r, c.g, c.b, 1 - (t / 0.15f));
        }
    }

    private void UpdateShape()
    {
        int glopos = (((pos[0] * 3) + pos[2]) * 9) + (pos[1] * 3) + pos[3];
        for (int i = 0; i < 2; i++)
        {
            if (glitched[i].Contains(glopos))
            {
                if (!glitching[i])
                {
                    glitching[i] = true;
                    StartCoroutine(Glitch(i));
                }
            }
            else if (glitching[i])
                glitching[i] = false;
            else
            {
                int shape = dogrid[glopos / 9, glopos % 9];
                screen[i + 1].material = shapes[i == 0 ? shape / 10 : shape % 10];
            }
        }
    }

    private IEnumerator Glitch(int i)
    {
        while (glitching[i])
        {
            yield return new WaitForSeconds(0.1f);
            screen[i + 1].material = shapes[UnityEngine.Random.Range(0, 9)];
        }
        int shape = dogrid[(pos[0] * 3) + pos[2], (pos[1] * 3) + pos[3]];
        screen[i + 1].material = shapes[i == 0 ? shape / 10 : shape % 10];
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Phase 1: !{0} left/submit/right [l/m/r can be used as ahorthands] | !{0} cycle | !{0} <display> [Cycles to display] | Phase 2: !{0} left/right even/odd | !{0} submit";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (gud.Any(x => x == false))
        {
            if (command == "cycle")
            {
                for (int i = 0; i < seq[1].Count(); i++)
                {
                    yield return null;
                    buttons[2].OnInteract();
                    yield return new WaitForSeconds(1.8f);
                }
                yield break;
            }
            int b = new List<string> { "left", "submit", "right", "l", "m", "r" }.IndexOf(command);
            if (b >= 0)
            {
                while (anim)
                    yield return true;
                yield return null;
                buttons[b % 3].OnInteract();
                yield break;
            }
            int p = disp.Select(x => x.Replace(" ", "").ToLowerInvariant()).ToList().IndexOf(command.Replace(" ", ""));
            if(p < 0)
            {
                yield return "sendtochaterror!f " + command + " is not a possible display.";
                yield break;
            }
            int[] q = new int[2] { pos[0] - p, p - pos[0] }.Select(x => (x + disp.Count()) % disp.Count()).ToArray();
            if(q.Min() == q[0])
            {
                while(pos[0] != p)
                {
                    while (anim)
                        yield return true;
                    yield return null;
                    buttons[0].OnInteract();
                }
            }
            else
            {
                while (pos[0] != p)
                {
                    while (anim)
                        yield return true;
                    yield return null;
                    buttons[2].OnInteract();
                }
            }           
        }
        else if(!moduleSolved)
        {
            if(command == "submit" || command == "m")
            {
                yield return null;
                buttons[1].OnInteract();
                yield break;
            }
            string[] commands = command.Split(' ');
            if(commands.Length == 2)
            {
                int t = new List<string> { "even", "odd", "e", "o" }.IndexOf(commands[1]) % 2;
                if(t < 0)
                {
                    yield return "sendtochaterror!f " + command[1] + " is not a valid parity.";
                    yield break;
                }
                int d = (new List<string> { "left", "right", "l", "r" }.IndexOf(commands[0]) % 2) * 2;
                if(d < 0)
                {
                    yield return "sendtochaterror!f " + command[1] + " is not a valid button.";
                    yield break;
                }
                while (anim || (int)info.GetTime() % 2 != t)
                    yield return true;
                yield return null;
                buttons[d].OnInteract();               
            }
            else
                yield return "sendtochaterror!f Invalid command: " + command;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (gud.Any(x => x == false))
        {
            while (anim)
                yield return true;
            int ct = gud.Count(x => x == false);
            for (int i = 0; i < ct; i++)
            {
                int leftInd = pos[0];
                int rightInd = pos[0];
                int leftCt = 0;
                int rightCt = 0;
                while (seq[0].IndexOf(seq[1][leftInd]) >= 3 || gud[seq[0].IndexOf(seq[1][leftInd])])
                {
                    leftInd--;
                    if (leftInd < 0)
                        leftInd = disp.Count - 1;
                    leftCt++;
                }
                while (seq[0].IndexOf(seq[1][rightInd]) >= 3 || gud[seq[0].IndexOf(seq[1][rightInd])])
                {
                    rightInd++;
                    if (rightInd > (disp.Count - 1))
                        rightInd = 0;
                    rightCt++;
                }
                if (leftCt < rightCt)
                {
                    for (int j = 0; j < leftCt; j++)
                    {
                        buttons[0].OnInteract();
                        while (anim)
                            yield return true;
                    }
                }
                else if (rightCt < leftCt)
                {
                    for (int j = 0; j < rightCt; j++)
                    {
                        buttons[2].OnInteract();
                        while (anim)
                            yield return true;
                    }
                }
                else if (leftCt == rightCt)
                {
                    int choice = UnityEngine.Random.Range(0, 2);
                    for (int j = 0; j < (choice == 0 ? leftCt : rightCt); j++)
                    {
                        buttons[choice == 0 ? 0 : 2].OnInteract();
                        while (anim)
                            yield return true;
                    }
                }
                buttons[1].OnInteract();
                while (anim)
                    yield return true;
            }
        }
        var q = new Queue<int[]>();
        var allMoves = new List<Movement>();
        var startPoint = new int[] { pos[0], pos[1], pos[2], pos[3] };
        var target = new int[] { -1, -1, -1, -1 };
        q.Enqueue(startPoint);
        while (q.Count > 0)
        {
            var next = q.Dequeue();
            if (((next[0] * 3) + next[2]) * 9 + (next[1] * 3) + next[3] == targetshape)
            {
                for (int i = 0; i < 4; i++)
                    target[i] = next[i];
                goto readyToSubmit;
            }
            List<int[]> options = new List<int[]>();
            for (int i = 0; i < 4; i++)
            {
                int[] temp = { next[0], next[1], next[2], next[3] };
                temp[dir[i]] += back[i] ? 2 : 1; temp[dir[i]] %= 3;
                options.Add(temp);
            }
            for (int i = 0; i < 4; i++)
            {
                if (!allMoves.Any(x => x.start[0] == options[i][0] && x.start[1] == options[i][1] && x.start[2] == options[i][2] && x.start[3] == options[i][3]))
                {
                    q.Enqueue(options[i]);
                    if (i == 0)
                        allMoves.Add(new Movement { start = next, end = options[i], btn = 0, even = true });
                    else if (i == 1)
                        allMoves.Add(new Movement { start = next, end = options[i], btn = 0, even = false });
                    else if (i == 2)
                        allMoves.Add(new Movement { start = next, end = options[i], btn = 2, even = true });
                    else if (i == 3)
                        allMoves.Add(new Movement { start = next, end = options[i], btn = 2, even = false });
                }
            }
        }
        throw new InvalidOperationException("There is a bug in the TP autosolver." + moduleID);
        readyToSubmit:
        if (allMoves.Count != 0) // Checks for position already being target
        {
            var lastMove = allMoves.First(x => x.end[0] == target[0] && x.end[1] == target[1] && x.end[2] == target[2] && x.end[3] == target[3]);
            var relevantMoves = new List<Movement> { lastMove };
            while (lastMove.start != startPoint)
            {
                lastMove = allMoves.First(x => x.end[0] == lastMove.start[0] && x.end[1] == lastMove.start[1] && x.end[2] == lastMove.start[2] && x.end[3] == lastMove.start[3]);
                relevantMoves.Add(lastMove);
            }
            for (int i = 0; i < relevantMoves.Count; i++)
            {
                while ((int)info.GetTime() % 2 != (relevantMoves[relevantMoves.Count - 1 - i].even ? 0 : 1)) { yield return true; }
                buttons[relevantMoves[relevantMoves.Count - 1 - i].btn].OnInteract();
                while (anim)
                    yield return true;
            }
        }
        buttons[1].OnInteract();
    }

    class Movement
    {
        public int[] start;
        public int[] end;
        public int btn;
        public bool even;
    }
}
