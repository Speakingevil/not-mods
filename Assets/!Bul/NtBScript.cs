using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

public class NtBScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMColorblindMode cbmode;
    public List<KMSelectable> buttons;
    public GameObject[] bulb;
    public GameObject matstore;
    public Renderer[] brends;
    public Material[] bulbcols;
    public Material[] screwcols;
    public GameObject[] lights;
    public TextMesh cbtext;
    public AudioClip[] sound;

    private readonly string a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly int[,,] bcommands = new int[6, 26, 4] {
    { { 1, 3, 1, 2 }, { 2, 1, 0, 0 }, { 0, 1, 3, 3 }, { 1, 0, 0, 3 }, { 3, 0, 1, 2 }, { 0, 1, 2, 1 }, { 1, 3, 3, 2 }, { 0, 3, 0, 1 }, { 3, 2, 1, 1 }, { 2, 0, 1, 2 }, { 0, 2, 3, 0 }, { 0, 3, 1, 3 }, { 0, 0, 3, 1 }, { 3, 0, 2, 1 }, { 3, 0, 1, 1 }, { 2, 3, 3, 0 }, { 3, 0, 0, 1 }, { 0, 1, 2, 3 }, { 1, 2, 2, 0 }, { 3, 0, 2, 3 }, { 0, 2, 2, 1 }, { 3, 0, 1, 3 }, { 2, 3, 1, 1 }, { 0, 3, 0, 2 }, { 1, 0, 1, 3 }, { 1, 0, 1, 2 } },
    { { 0, 0, 2, 3 }, { 0, 1, 2, 2 }, { 0, 3, 2, 1 }, { 2, 0, 3, 3 }, { 0, 1, 0, 3 }, { 0, 2, 1, 1 }, { 0, 0, 1, 3 }, { 1, 2, 2, 3 }, { 2, 1, 3, 1 }, { 0, 2, 3, 1 }, { 3, 1, 0, 0 }, { 3, 1, 2, 2 }, { 0, 1, 3, 1 }, { 3, 2, 0, 1 }, { 1, 2, 0, 0 }, { 0, 1, 1, 2 }, { 3, 0, 2, 2 }, { 1, 0, 0, 2 }, { 1, 2, 1, 0 }, { 3, 3, 2, 0 }, { 3, 1, 1, 0 }, { 3, 2, 1, 3 }, { 2, 1, 2, 3 }, { 2, 1, 2, 0 }, { 2, 1, 3, 3 }, { 3, 2, 0, 2 } },
    { { 2, 3, 0, 1 }, { 3, 3, 0, 1 }, { 1, 2, 0, 2 }, { 1, 3, 0, 2 }, { 3, 1, 3, 0 }, { 0, 0, 1, 2 }, { 1, 2, 3, 0 }, { 1, 1, 2, 3 }, { 2, 1, 3, 2 }, { 2, 3, 2, 0 }, { 1, 1, 3, 0 }, { 3, 1, 2, 0 }, { 2, 1, 1, 3 }, { 0, 2, 1, 2 }, { 2, 1, 1, 0 }, { 0, 2, 1, 0 }, { 2, 2, 1, 0 }, { 1, 1, 0, 2 }, { 1, 3, 2, 2 }, { 0, 2, 3, 2 }, { 0, 3, 1, 2 }, { 2, 0, 2, 3 }, { 2, 3, 2, 1 }, { 1, 2, 3, 1 }, { 2, 2, 0, 3 }, { 2, 2, 3, 0 } },
    { { 3, 1, 0, 2 }, { 3, 2, 1, 2 }, { 2, 0, 1, 3 }, { 3, 1, 0, 3 }, { 2, 0, 0, 1 }, { 1, 1, 2, 0 }, { 2, 0, 3, 2 }, { 3, 0, 0, 2 }, { 3, 3, 1, 2 }, { 1, 0, 3, 3 }, { 3, 2, 0, 0 }, { 0, 3, 1, 1 }, { 0, 0, 2, 1 }, { 0, 3, 2, 0 }, { 2, 3, 1, 2 }, { 3, 1, 2, 1 }, { 3, 2, 3, 0 }, { 1, 2, 3, 3 }, { 3, 2, 2, 1 }, { 1, 3, 2, 1 }, { 0, 3, 3, 1 }, { 0, 3, 1, 0 }, { 0, 0, 3, 2 }, { 3, 1, 3, 2 }, { 3, 2, 2, 0 }, { 2, 2, 3, 1 } },
    { { 0, 1, 1, 3 }, { 3, 0, 3, 1 }, { 1, 2, 0, 1 }, { 3, 2, 1, 0 }, { 3, 3, 0, 2 }, { 1, 1, 3, 2 }, { 1, 3, 0, 3 }, { 1, 3, 1, 0 }, { 2, 3, 0, 3 }, { 2, 1, 0, 1 }, { 3, 1, 2, 3 }, { 1, 2, 3, 2 }, { 2, 0, 1, 0 }, { 3, 1, 1, 2 }, { 2, 3, 1, 3 }, { 0, 1, 0, 2 }, { 2, 1, 3, 0 }, { 3, 0, 2, 0 }, { 2, 3, 1, 0 }, { 0, 3, 2, 2 }, { 2, 0, 1, 1 }, { 0, 2, 1, 3 }, { 2, 0, 0, 3 }, { 1, 0, 2, 2 }, { 1, 3, 0, 0 }, { 0, 3, 2, 3 } },
    { { 3, 1, 0, 1 }, { 1, 1, 0, 3 }, { 1, 0, 2, 3 }, { 3, 3, 2, 1 }, { 1, 0, 3, 2 }, { 1, 3, 2, 0 }, { 1, 3, 3, 0 }, { 0, 1, 2, 0 }, { 0, 2, 0, 1 }, { 0, 3, 3, 2 }, { 1, 0, 3, 0 }, { 2, 1, 0, 3 }, { 2, 0, 2, 1 }, { 1, 3, 0, 1 }, { 0, 2, 3, 3 }, { 1, 2, 1, 3 }, { 3, 0, 3, 2 }, { 2, 3, 3, 1 }, { 1, 0, 2, 0 }, { 0, 2, 2, 3 }, { 2, 1, 0, 2 }, { 0, 1, 3, 0 }, { 2, 3, 0, 2 }, { 1, 3, 2, 3 }, { 1, 2, 0, 3 }, { 2, 2, 1, 3 } } };
    private readonly string[] wordlist = new string[26] { "AMPLITUDE", "BOULEVARD", "CHEMISTRY", "DUPLICATE", "EIGHTFOLD", "FILAMENTS", "GOLDSMITH", "HARLEQUIN", "INJECTORS", "JUXTAPOSE", "KILOHERTZ", "LABYRINTH", "MOUSTACHE", "NEIGHBOUR", "OBSCURITY", "PENUMBRAL", "QUICKSAND", "RHAPSODIC", "SQUAWKING", "TRIGLYPHS", "UNIVERSAL", "VEXATIONS", "WHIZBANGS", "XENOGLYPH", "YARDSTICK", "ZIGAMORPH" };
    private readonly Color[] lightcols = new Color[6] { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 1, 0), new Color(0.5f, 0, 1), new Color(1, 1, 1)};
    private readonly int[,] wordmods = new int[5, 8] {
    { 0, 6, 7, 4, 5, 3, 1, 2},
    { 0, 5, 1, 6, 2, 4, 7, 3},
    { 0, 1, 2, 3, 4, 5, 6, 7},
    { 0, 4, 3, 7, 6, 1, 2, 5},
    { 0, 7, 4, 1, 3, 2, 5, 6} };
    private string[] words = new string[4];
    private List<int> indices;
    private bool[] target = new bool[4];
    private bool?[] ans = new bool?[4];
    private int[] properties;
    private bool[] transmit = new bool[2];
    private bool cb;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        matstore.SetActive(false);
        cb = cbmode.ColorblindModeActive;
        float l = transform.lossyScale.x;
        foreach (GameObject light in lights)
        {
            light.GetComponent<Light>().range *= l;
            light.SetActive(false);
        }
        properties = new int[3] { Random.Range(0, 6), Random.Range(0, 5), Random.Range(0, 2)};
        lights[0].GetComponent<Light>().color = lights[1].GetComponent<Light>().color = lightcols[properties[0]];
        brends[0].material = bulbcols[properties[0]];
        brends[1].material = screwcols[properties[1]];
        if (properties[2] == 1)
            for (int i = 0; i < 2; i++)
            {
                Vector3 b = bulb[i + 1].transform.localPosition;
                bulb[i + 1].transform.localPosition = new Vector3(-b.x, b.y, b.z);
            }
        if (cb)
            cbtext.text = new string[] { "RED", "GREEN", "BLUE", "YELLOW", "PURPLE", "WHITE"}[properties[0]];
        Debug.LogFormat("[Not The Bulb #{0}] The bulb is {1} with a {2} cap. The O button is on the {3}.", moduleID, new string[] { "red", "green", "blue", "yellow", "purple", "white"}[properties[0]], new string[] { "copper", "silver", "gold", "plastic", "carbon fibre"}[properties[1]], new string[] { "left", "right"}[properties[2]]);
        int[] r = new int[2] { Random.Range(0, 9), Random.Range(0, 26) };
        Debug.LogFormat("[Not The Bulb #{0}] The encryption is: +{1} left shift, +{2} Caesar shift.", moduleID, r[0], r[1]);
        words[0] = wordlist[Random.Range(0, 26)];
        words[1] = string.Join("", Enumerable.Range(0, 9).Select(i => a[(a.IndexOf(words[0][(i + r[0]) % 9]) + r[1]) % 26].ToString()).ToArray());
        Debug.LogFormat("[Not The Bulb #{0}] The transmission is \"{1}\". Decrypting yields \"{2}\".", moduleID, words[1], words[0]);
        words[2] = wordlist[a.IndexOf(words[1][0].ToString())];
        words[3] = string.Join("", Enumerable.Range(0, 9).Select(i => a[(a.IndexOf(words[2][(i + r[0]) % 9]) + r[1]) % 26].ToString()).ToArray());
        Debug.LogFormat("[Not The Bulb #{0}] The target word is \"{1}\". Encrypting yields \"{2}\".", moduleID, words[2], words[3]);
        indices = words[3].Select(x => a.IndexOf(x)).ToList();
        Ansgen();
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate () { Press(b); return false; };
        }
    }

    private void Ansgen()
    {
        for (int i = 0; i < 4; i++)
        {
            int k = bcommands[properties[0], indices[0], i];
            target[i] = properties[2] == 0 ? (k % 2 == 1) : (k % 3 != 0);
        }
        Debug.LogFormat("[Not The Bulb #{0}] {3} transmission: {1} \u2192 {2}.", moduleID, a[indices[0]], string.Join("", target.Select(x => x ? "R" : "L").ToArray()), new string[] { "Final", "Eighth", "Seventh", "Sixth", "Fifth", "Fourth", "Third", "Second", "First"}[indices.Count() - 1]);
    }

    private void Press(int b)
    {
        if (!moduleSolved)
        {
            buttons[b].AddInteractionPunch();
            if (transmit[0])
            {
                module.HandleStrike();
                Debug.LogFormat("[Not The Bulb #{0}] Error: Selection occured during transmission.", moduleID);
            }
            if (b != 1)
            {
                Audio.PlaySoundAtTransform("ButtonClick", buttons[b].transform);
                if (!transmit[1])
                    transmit[1] = true;
                for (int i = 0; i < 3; i++)
                    ans[i] = ans[i + 1];
                ans[3] = b == 2;
            }
            else
            {
                if (!transmit[1])
                    StartCoroutine(Transmission());
                else
                {
                    Debug.LogFormat("[Not The Bulb #{0}] Submitted {1}.", moduleID, string.Join("", ans.Select(x => x == null ? "" : ( (bool)x ? "R" : "L")).ToArray()));
                    if (ans.Select((x, i) => x == target[i]).All(x => x))
                    {
                        if (indices.Count() < 2)
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            StartCoroutine(Solve());
                        }
                        else
                        {
                            indices.RemoveAt(0);
                            int soundmod = Random.Range(0, 3) != 0 ? Random.Range(1, 8) : 0;
                            string old = string.Join("", indices.Select(x => a[x].ToString()).ToArray());
                            switch (wordmods[properties[1], soundmod])
                            {
                                case 1: indices = indices.Select(x => (x + 1) % 26).ToList(); break;
                                case 2: indices = indices.Select(x => (x + 25) % 26).ToList(); break;
                                case 3: indices = indices.Select(x => (x + 13) % 26).ToList(); break;
                                case 4: indices = indices.Select(x => 25 - x).ToList(); break;
                                case 5: indices = Enumerable.Range(0, indices.Count()).Select(x => indices[(x + 1) % indices.Count()]).ToList(); break;
                                case 6: indices = Enumerable.Range(0, indices.Count()).Select(x => indices[(x + indices.Count() - 1) % indices.Count()]).ToList(); break;
                                case 7: indices.Reverse(); break;
                            }
                            if (soundmod > 0)
                            {
                                if (soundmod > 6)
                                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
                                else
                                    Audio.PlaySoundAtTransform(sound[soundmod - 1].name, transform);
                                Debug.LogFormat("[Not The Bulb #{0}] {1}: {2} \u2192 {3}", moduleID, new string[] { "Select", "Click", "Tap", "Ding", "Key", "Bumper", "Fakestrike" }[soundmod - 1], old, string.Join("", indices.Select(x => a[x].ToString()).ToArray()));
                            }
                            else
                                Debug.LogFormat("[Not The Bulb #{0}] Silence. No modifications made.", moduleID);
                            StartCoroutine(Flash());
                            Ansgen();
                        }
                    }
                    else
                    {
                        module.HandleStrike();
                        indices = words[3].Select(x => a.IndexOf(x)).ToList();
                        Debug.LogFormat("[Not The Bulb #{0}] Strike. Resetting to first submission.", moduleID);
                        ans = new bool?[4];
                        transmit[1] = false;
                        Ansgen();
                    }
                }
            }
        }
    }

    private IEnumerator Transmission()
    {
        string b = "ABCDEFGHIJLMNOPQRSTUVWXYZK";
        transmit[0] = true;
        for (int i = 0; i < 9; i++)
        {
            int[] tap = new int[2] { (b.IndexOf(words[1][i]) / 5) + 1, (b.IndexOf(words[1][i]) % 5) + 1 };
            yield return new WaitForSeconds(0.4f);
            for (int j = 0; j < tap[0]; j++)
            {
                for (int k = 0; k < 2; k++) lights[k].SetActive(true);
                yield return new WaitForSeconds((1f - (0.05f * tap[0])) / tap[0]);
                for (int k = 0; k < 2; k++) lights[k].SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(0.2f);
            for (int j = 0; j < tap[1]; j++)
            {
                for (int k = 0; k < 2; k++) lights[k].SetActive(true);
                yield return new WaitForSeconds((1f - (0.05f * tap[1])) / tap[1]);
                for (int k = 0; k < 2; k++) lights[k].SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }
        }
        transmit[0] = false;
    }

    private IEnumerator Flash()
    {
        for (int k = 0; k < 2; k++) lights[k].SetActive(true);
        yield return new WaitForSeconds(0.25f);
        for (int k = 0; k < 2; k++) lights[k].SetActive(false);
    }

    private IEnumerator Solve()
    {
        cbtext.text = "";
        Audio.PlaySoundAtTransform("ButtonClick", bulb[0].transform);
        Audio.PlaySoundAtTransform("Unscrew", transform);
        for(int i = 0; i < 20; i++)
        {
            bulb[0].transform.localPosition += new Vector3(0, 0.075f, 0);
            bulb[0].transform.Rotate(0, 27, 0);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < 20; i++)
        {
            if (Application.isEditor)
                bulb[0].transform.Translate(new Vector3(0, 0, -0.0075f * i), Space.World);
            else
                bulb[0].transform.Translate(new Vector3(0, -0.0075f * i, 0), Space.World);
            yield return new WaitForSeconds(0.02f);
        }
        Audio.PlaySoundAtTransform("Break", bulb[0].transform);
        bulb[0].SetActive(false);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <B/I/O> [Presses the bulb, I button, or O button] | !{0} colorblind [Toggles colorblind mode] | Presses can be chained, for ex: !{0} IOOIB";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (cb)
            {
                cb = false;
                cbtext.text = "";
            }
            else
            {
                cb = true;
                cbtext.text = new string[] { "RED", "GREEN", "BLUE", "YELLOW", "PURPLE", "WHITE" }[properties[0]];
            }
            yield break;
        }
        if (command.Split(' ').Length > 1)
        {
            yield return "sendtochaterror Too many parameters!";
            yield break;
        }
        string[] valids = { "B", "I", "O" };
        for (int i = 0; i < command.Length; i++)
        {
            if (!valids.Contains(command[i].ToString().ToUpper()))
            {
                yield return "sendtochaterror!f The specified character '" + command[i] + "' is invalid!";
                yield break;
            }
        }
        yield return null;
        for (int i = 0; i < command.Length; i++)
        {
            if (command[i].EqualsAny('B', 'b'))
                buttons[1].OnInteract();
            else if (command[i].EqualsAny('I', 'i'))
            {
                if (properties[2] == 0)
                    buttons[2].OnInteract();
                else
                    buttons[0].OnInteract();
            }
            else if (command[i].EqualsAny('O', 'o'))
            {
                if (properties[2] == 0)
                    buttons[0].OnInteract();
                else
                    buttons[2].OnInteract();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (transmit[0]) yield return true;
        int start = 9 - indices.Count;
        for (int i = start; i < 9; i++)
        {
            int start2 = 0;
            if (ans.Select((x, a) => x == target[a]).All(x => x))
                start2 = 4;
            else if (ans[1] == target[0] && ans[2] == target[1] && ans[3] == target[2])
                start2 = 3;
            else if (ans[2] == target[0] && ans[3] == target[1])
                start2 = 2;
            else if (ans[3] == target[0])
                start2 = 1;
            for (int j = start2; j < 4; j++)
            {
                if (target[j])
                    buttons[2].OnInteract();
                else
                    buttons[0].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
            buttons[1].OnInteract();
            if (i != 8)
                yield return new WaitForSeconds(0.2f);
        }
    }
}
