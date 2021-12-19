using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VCFScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] buttons;
    public TextMesh display;

    private readonly string[] words = new string[6] { "RED", "GREEN", "BLUE", "MAGENTA", "YELLOW", "WHITE"};
    private readonly Color[] cols = new Color[6] { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 0, 1), new Color(1, 1, 0), new Color(1, 1, 1) };
    private readonly int[][] grid = new int[12][]
    {
       new int[12] { 33, 18,  3, 30, 29, 19,  0, 16, 11, 13, 10,  1},
       new int[12] { 21, 13, 25,  4, 34, 24,  5,  2, 15, 23, 35, 22},
       new int[12] { 27, 31, 17, 35, 22, 28, 20,  7, 30, 24,  3, 32},
       new int[12] { 14,  8, 11, 15,  1, 10,  9, 21, 34,  4, 31,  6},
       new int[12] {  2, 16, 20,  7, 26, 32, 25, 19, 27, 33, 12,  8},
       new int[12] {  0,  5,  9, 23,  6, 12, 14, 29, 18, 28, 26, 17},
       new int[12] { 17, 34,  4, 21, 27,  0,  8, 11, 35,  7,  1, 10},
       new int[12] { 32, 29, 30, 18, 28,  5, 17, 15,  9, 22, 13, 24},
       new int[12] {  7, 20, 26, 31, 24,  1, 30,  3, 25,  0, 27,  5},
       new int[12] { 12, 33, 16, 25, 35, 22, 34,  6, 23, 14,  4, 31},
       new int[12] {  3, 11, 15,  2, 14,  8, 21, 18, 33, 20, 19, 28},
       new int[12] {  9, 19, 23,  6, 10, 13, 16, 32, 12,  2, 29, 26},
    };
    private int[] slasts = new int[5] { -1, -1, -1, -1, -1};
    private int[] sequence = new int[5];
    private int stage;
    private float[] centre = new float[2];
    private bool[] ans;
    private int sub;
    private bool off;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        foreach(KMSelectable button in buttons)
        {
            bool b = buttons[0] == button;
            button.OnInteract = delegate ()
            {
                if (!moduleSolved && !off)
                {
                    if (!b)
                    {
                        StopCoroutine("Standby");
                    }
                    button.AddInteractionPunch(0.5f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    Debug.LogFormat("[Varicolour Flash #{0}] Pressed {1} {2}", moduleID, b ? "Yes." : "No.", ans[sub] == b ? "Correct." : "Wrong. Entries deleted.");
                    if (b == ans[sub])
                    {
                        sub++;
                        if(sub > stage)
                        {
                            StopCoroutine("Seq");
                            display.text = "";
                            if(stage > 3)
                            {
                                module.HandlePass();
                                moduleSolved = true;
                                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                                display.text = words[slasts[4] / 6];
                                display.color = cols[slasts[4] % 6];
                            }
                            else
                            {
                                stage++;
                                Audio.PlaySoundAtTransform("InputCorrect", transform);
                                Newseq();
                            }
                        }
                    }
                    else
                    {
                        sub = 0;
                        module.HandleStrike();
                    }
                }
                return false;
            };
        }
        buttons[1].OnHighlight = delegate () { if (!moduleSolved && !off && stage > 0) StartCoroutine("Standby"); };
        buttons[1].OnHighlightEnded = delegate () { if (!moduleSolved && !off) StopCoroutine("Standby"); };
        Reset();
    }

    private void Reset()
    {
        sub = 0;
        stage = 0;
        for (int i = 0; i < 2; i++)
            centre[i] = Random.Range(1, 10) + 0.5f;
        List<int> seqone = new List<int> { };
        for (int i = 0; i < 4; i++)
            seqone.Add(grid[(int)centre[0] + (i / 2 == 0 ? -1 : 2)][(int)centre[1] + (i % 2 == 0 ? -1 : 2)]);
        seqone.Shuffle();
        int r = Random.Range(0, 36);
        while (seqone.Contains(r))
            r = Random.Range(0, 36);
        seqone.Add(r);
        sequence = seqone.ToArray();
        slasts[0] = sequence[4];
        Debug.LogFormat("[Varicolour Flash #{0}] Stage 1- The displays are: {1}", moduleID, string.Join(", ", sequence.Select(x => "RGBMYW"[x / 6] + "/" + "RGBMYW"[x % 6]).ToArray()));
        Debug.LogFormat("[Varicolour Flash #{0}] The initial subgrid is {1}{2}-{3}{4}.", moduleID, "ABCDEFGHI"[(int)centre[1] - 1], (int)centre[0], "DEFGHIJKL"[(int)centre[1] - 1], (int)centre[0] + 3);
        Det();
    }

    private void Det()
    {
        int[] subgrid = grid.Where((x, i) => Mathf.Abs(i - centre[0]) < 2f).Select(x => x.Where((y, i) => Mathf.Abs(i - centre[1]) < 2f).ToArray()).SelectMany(x => x).ToArray();
        Debug.LogFormat("[Varicolour Flash #{0}] The {2}subgrid contains the displays: {1}.", moduleID, string.Join(", ", subgrid.Select(x => "RGBMYW"[x / 6] + "/" + "RGBMYW"[x % 6]).ToArray()), stage > 0 ? "new " : "");
        ans = slasts.Take(stage + 1).Select(x => subgrid.Contains(x)).ToArray();
        Debug.LogFormat("[Varicolour Flash #{0}] Press the buttons in the order: {1}.", moduleID, string.Join(", ", ans.Select(x => x ? "Yes" : "No").ToArray()));
        StartCoroutine("Seq");
    }

    private void Newseq()
    {
        sub = 0;
        string[] dirc = new string[4];
        sequence = Enumerable.Range(0, 36).ToArray().Shuffle().Take(5).ToArray();
        slasts[stage] = sequence[4];
        Debug.LogFormat("[Varicolour Flash #{0}] Stage {2}- The displays were: {1}", moduleID, string.Join(", ", sequence.Select(x => "RGBMYW"[x / 6] + "/" + "RGBMYW"[x % 6]).ToArray()), stage + 1);
        for (int j = 0; j < 4; j++)
        {
            List<float> d = new List<float> { };
            for (int i = 0; i < 144; i++)
            {
                if (sequence[j] == grid[i / 12][i % 12])
                {
                    d.Add((i / 12) - centre[0]);
                    d.Add((i % 12) - centre[1]);
                }
            }
            List<float> ad = d.Select(x => Mathf.Abs(x)).ToList();
            float m = ad.Max();
            if (ad.Count(x => x == m) > 1)
                dirc[j] = "Nowhere";
            else
            {
                int[] p = new int[2] { (int)Mathf.Sign(d.First(x => Mathf.Abs(x) == m)) + 1, ad.IndexOf(m) % 2};
                switch (p.Sum())
                {
                    case 0:
                        dirc[j] = "Up";
                        centre[0] -= j + 1;
                        break;
                    case 1:
                        dirc[j] = "Left";
                        centre[1] -= j + 1;
                        break;
                    case 2:
                        dirc[j] = "Down";
                        centre[0] += j + 1;
                        break;
                    default:
                        dirc[j] = "Right";
                        centre[1] += j + 1;
                        break;
                }
            }
        }
        Debug.LogFormat("[Varicolour Flash #{0}] The grid moves: {1}.", moduleID, string.Join(", ", dirc));
        Det();
    }

    private IEnumerator Seq()
    {
        off = true;
        string[] w = sequence.Select(x => words[x / 6]).ToArray();
        Color[] c = sequence.Select(x => cols[x % 6]).ToArray();
        display.text = "";
        yield return new WaitForSeconds(1);
        off = false;
        for(int i = 0; i < 6; i++)
        {
            if (i > 4)
            {
                display.text = "";
                i = -1;
            }
            else
            {
                display.text = w[i];
                display.color = c[i];
            }
            yield return new WaitForSeconds(0.75f);
        }
    }

    private IEnumerator Standby()
    {
        yield return new WaitForSeconds(4.5f);
        StopCoroutine("Seq");
        off = true;
        display.fontSize = 50;
        display.color = cols[5];
        display.text = "RESETTING";
        Audio.PlaySoundAtTransform("ResetAlarm", transform);
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.25f);
            display.text += ".";
        }
        yield return new WaitForSeconds(0.25f);
        display.fontSize = 60;
        Reset();
    }

    bool TPHighlightActive;
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <Yes/No> [Presses buttons. Can be chained with spaces up to the current stage number.] | !{0} reset";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (TPHighlightActive)
        {
            yield return "sendtochaterror!f Cannot interact during reset.";
            yield break;
        }
        command = command.ToUpperInvariant();
        if(command == "RESET")
        {
            yield return null;
            StartCoroutine(TPReset());
            while (TPHighlightActive)
                yield return "trycancel";
            yield break;
        }
        string[] commands = command.Split(' ');
        if(commands.Length > stage + 2)
        {
            yield return "sendtochaterror!f At most " + (stage + 1).ToString() + " button" + (stage > 0 ? "s" : "") + " may be pressed at this stage.";
        }
        if(commands[0] != "PRESS")
        {
            yield return "sendtochaterror!f Button press commands must begin with \"press\".";
            yield break;
        }
        if(commands.Length < 2)
        {
            yield return "sendtochaterror!f Buttons must be specified.";
            yield break;
        }
        List<int> b = new List<int> { };
        for(int i = 1; i < commands.Length; i++)
        {
            if (commands[i] == "YES")
                b.Add(0);
            else if (commands[i] == "NO")
                b.Add(1);
            else
            {
                yield return "sendtochaterror!f Invalid button: " + commands[i];
                yield break;
            }
        }
        for(int i = 0; i < b.Count(); i++)
        {
            yield return null;
            buttons[b[i]].OnInteract();
        }
    }

    private IEnumerator TPReset()
    {
        TPHighlightActive = true;
        buttons[1].OnHighlight();
        float elapsed = 0f;
        while(elapsed < 4.5f)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        buttons[1].OnHighlightEnded();
        TPHighlightActive = false;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if(stage > 0)
            StartCoroutine(TPReset());
        while (TPHighlightActive)
            yield return true;
        for(int i = 0; i < 5; i++)
        {
            while (off)
                yield return true;
            for(int j = 0; j < ans.Length; j++)
            {
                yield return null;
                buttons[ans[j] ? 0 : 1].OnInteract();
            }
        }
    }
}
