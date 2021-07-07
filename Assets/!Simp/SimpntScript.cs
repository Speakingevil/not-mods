using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class SimpntScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable button;
    public KMBombInfo info;
    public GameObject rings;
    public Renderer[] rends;
    public Material[] io;
    public TextMesh btext;

    private int[] vals = new int[9];
    private bool[,] on = new bool[9, 6];
    private bool[][] ans = new bool[2][] { new bool[9], new bool[9] };
    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;
    private int time;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        rings.SetActive(false);
        for(int i = 0; i < 9; i++)
        {
            vals[i] = Random.Range(1, 64);
            for (int j = 0; j < 6; j++)
                if ((vals[i] / (int)Mathf.Pow(2, j)) % 2 == 1)
                    on[i, j] = true;
        }
        Debug.LogFormat("[Simpleton't #{0}] The values are: {1}", moduleID, string.Join(", ", vals.Select(x => x.ToString()).ToArray()));
        string product = vals.Aggregate((long)1, (a, b) => a * b).ToString();
        Debug.LogFormat("[Simpleton't #{0}] The product of these values is {1}.", moduleID, product);
        for (int i = 0; i < 9; i++)
            if (!product.Contains((i + 1).ToString()))
                ans[0][i] = true;
        Debug.LogFormat("[Simpleton't #{0}] The button must be pressed when the last seconds digit is: {1}", moduleID, ans[0].Any(x => x) ? string.Join(", ", Enumerable.Range(0, 9).Where(x => ans[0][x]).Select(x => (x + 1).ToString()).ToArray()) : "0");
        module.OnActivate = delegate () { time = (int)info.GetTime(); StartCoroutine("Seq"); };
        button.OnInteract = delegate () {
            if (!moduleSolved)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                button.AddInteractionPunch();
                int t = (int)info.GetTime() % 10;
                if (t == 0)
                {
                    if (ans[1].All(x => !x))
                    {
                        Debug.LogFormat("[Simpleton't #{0}] Button pressed on a zero.", moduleID);
                        if (ans[0].Any(x => x))
                            module.HandleStrike();
                        else
                        {
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                            btext.text = "VICTORY";
                            moduleSolved = true;
                            module.HandlePass();
                            rings.SetActive(false);
                        }
                    }
                }
                else
                    ans[1][t - 1] = true;
            } return false; };
        button.OnHighlight = delegate () { rings.SetActive(true); };
        button.OnHighlightEnded = delegate () { rings.SetActive(false); };
    }

    private IEnumerator Seq()
    {
        while (!moduleSolved)
        {
            while ((int)info.GetTime() == time)
                yield return new WaitForSeconds(0.1f);
            time = (int)info.GetTime();
            if(time % 10 == 0)
            {
                rends[0].material = io[0];
                yield return new WaitForSeconds(0.08f);
                for(int i = 0; i < 5; i++)
                {
                    rends[i].material = io[1];
                    rends[i + 1].material = io[0];
                    yield return new WaitForSeconds(0.08f);
                }
                rends[5].material = io[1];
                if (ans[1].Any(x => x))
                {
                    Debug.LogFormat("[Simpleton't #{0}] Submitted {1}", moduleID, string.Join(", ", Enumerable.Range(0, 9).Where(x => ans[1][x]).Select(x => (x + 1).ToString()).ToArray()));
                    if (ans[1].SequenceEqual(ans[0]))
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        btext.text = "VICTORY";
                        moduleSolved = true;
                        module.HandlePass();
                        rings.SetActive(false);
                        yield break;
                    }
                    else
                    {
                        module.HandleStrike();
                        ans[1] = new bool[9];
                    }
                }
            }
            else
            {
                for(int j = 0; j < 6; j++)
                    rends[j].material = io[on[(time % 10) - 1, j] ? 0 : 1];
                yield return new WaitForSeconds(0.4f);
                for (int j = 0; j < 6; j++)
                    rends[j].material = io[1];
            }
        }
    }

    //twitch plays
    bool ZenModeActive;
    bool TPHighlightActive;
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} highlight [Highlights the button until all numbers have shown once] | !{0} submit <#> (#₂)... [Submits the specified digit '#' (and '#₂' or more)]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*highlight\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (TPHighlightActive)
            {
                yield return "sendtochaterror Cannot interact with the module while the button is being highlighted!";
                yield break;
            }
            StartCoroutine(HighlightButtonTP());
            while (TPHighlightActive)
                yield return "trycancel";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 1)
            {
                List<int> choices = new List<int>();
                for (int i = 1; i < parameters.Length; i++)
                {
                    int temp = -1;
                    if (!int.TryParse(parameters[i], out temp))
                    {
                        yield return "sendtochaterror!f The specified digit '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                    if (temp < 0 || temp > 9)
                    {
                        yield return "sendtochaterror The specified digit '" + parameters[i] + "' is out of range 0-9!";
                        yield break;
                    }
                    if (!choices.Contains(temp))
                        choices.Add(temp);
                    else
                    {
                        yield return "sendtochaterror Duplicate digits cannot be submitted!";
                        yield break;
                    }
                }
                if ((!ZenModeActive && ((time % 10) < choices.Max())) || (ZenModeActive && ((time % 10) > choices.Min())))
                {
                    while (time % 10 != 0)
                        yield return "trycancel";
                }
                while (choices.Count != 0)
                {
                    if (choices.Contains(time % 10))
                    {
                        choices.Remove(time % 10);
                        button.OnInteract();
                    }
                    yield return null;
                }
                if (!command.Contains('0'))
                {
                    if (ans[0].SequenceEqual(ans[1]))
                        yield return "solve";
                    else
                        yield return "strike";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify at least one digit to submit!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (ans[1].Contains(true))
        {
            if (ZenModeActive)
            {
                int end = time % 10;
                if (end == 0)
                    end = 9;
                for (int i = 0; i < end; i++)
                {
                    if (ans[1][i] != ans[0][i])
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        btext.text = "VICTORY";
                        moduleSolved = true;
                        module.HandlePass();
                        rings.SetActive(false);
                        yield break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i > (time % 10) && ans[1][i] != ans[0][i])
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        btext.text = "VICTORY";
                        moduleSolved = true;
                        module.HandlePass();
                        rings.SetActive(false);
                        yield break;
                    }
                }
            }
        }
        while (TPHighlightActive)
            yield return true;
        if (ans[0].All(x => !x))
        {
            while (time % 10 != 0)
                yield return true;
            button.OnInteract();
        }
        else if (ans[0].SequenceEqual(ans[1]))
        {
            while (!moduleSolved)
                yield return true;
        }
        else
        {
            int waitForDigit = -1;
            int waitLastDigit = -1;
            if (ZenModeActive)
            {
                for (int i = 0; i <= 8; i++)
                {
                    if (ans[0][i] && !ans[1][i])
                    {
                        if (waitForDigit == -1)
                            waitForDigit = i + 1;
                        waitLastDigit = i + 1;
                    }
                }
            }
            else
            {
                for (int i = 8; i >= 0; i--)
                {
                    if (ans[0][i] && !ans[1][i])
                    {
                        if (waitForDigit == -1)
                            waitForDigit = i + 1;
                        waitLastDigit = i + 1;
                    }
                }
            }
            while ((time % 10) != waitForDigit)
                yield return true;
            button.OnInteract();
            if (waitForDigit != waitLastDigit)
            {
                while ((time % 10) != waitLastDigit)
                {
                    if (ans[0][(time % 10) - 1] != ans[1][(time % 10) - 1])
                        button.OnInteract();
                    yield return null;
                }
                button.OnInteract();
            }
            while (!moduleSolved)
                yield return true;
        }
    }

    IEnumerator HighlightButtonTP()
    {
        TPHighlightActive = true;
        int temp = time % 10;
        while (temp == time % 10)
            yield return null;
        button.OnHighlight();
        temp = time % 10;
        while (temp == time % 10)
            yield return null;
        while (temp != time % 10)
            yield return null;
        button.OnHighlightEnded();
        TPHighlightActive = false;
    }
}
