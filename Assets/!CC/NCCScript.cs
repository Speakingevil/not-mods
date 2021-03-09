using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class NCCScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Renderer[] brends;
    public Material[] mats;
    public TextMesh[] nums;

    private readonly int[,] intgrid = new int[8, 8]
    { { 5, 4, 1, 7, 3, 2, 8, 6},
      { 8, 3, 6, 2, 5, 1, 7, 4},
      { 7, 2, 4, 5, 8, 6, 3, 1},
      { 3, 1, 8, 6, 7, 4, 5, 2},
      { 6, 8, 2, 3, 1, 5, 4, 7},
      { 4, 7, 5, 1, 6, 8, 2, 3},
      { 2, 6, 3, 8, 4, 7, 1, 5},
      { 1, 5, 7, 4, 2, 3, 6, 8} };
    private readonly string[] morse = new string[8] { ".-.-.", "-....-", ".-.-.-", "---...", "-..-.", "..--.-", "-...-", "--..--"};
    private IEnumerator[] transmissions = new IEnumerator[4];
    private int[] labels = new int[8];
    private bool[] greens = new bool[4];
    private int[] ops = new int[4];
    private int[] outputs = new int[4];
    private bool flip;
    private bool[] pressed = new bool[4];
    private int[][] answer = new int[2][] { new int[4], new int[4]};
    private bool[] tpheld = new bool[4];

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
        for (int i = 0; i < 4; i++)
        {
            greens[i] = Random.Range(0, 2) == 1;
            brends[i].material = mats[greens[i] ? 1 : 0];
            ops[i] = Random.Range(0, 8);
            while (ops.Where((x, k) => k < i).Contains(ops[i]))
                ops[i] = Random.Range(0, 8);
            for (int j = 2 * i; j < (2 * i) + 2; j++)
            {
                labels[j] = Random.Range(1, 9);
                nums[j].text = labels[j].ToString();
            }
            switch (ops[i])
            {
                case 0:
                    outputs[i] = (labels[2 * i] + labels[(2 * i) + 1]) % 10;
                    break;
                case 1:
                    outputs[i] = Mathf.Abs(labels[2 * i] - labels[(2 * i) + 1]);
                    break;
                case 2:
                    outputs[i] = (((labels[2 * i] * labels[(2 * i) + 1]) - 1) % 9) + 1;
                    break;
                case 3:
                    outputs[i] = Mathf.Max(labels[2 * i], labels[(2 * i) + 1]) / Mathf.Min(labels[2 * i], labels[(2 * i) + 1]);
                    break;
                case 4:
                    outputs[i] = Mathf.Max(labels[2 * i], labels[(2 * i) + 1]) % Mathf.Min(labels[2 * i], labels[(2 * i) + 1]);
                    break;
                case 5:
                    outputs[i] = Xnor(labels[2 * i], labels[(2 * i) + 1], false);
                    break;
                case 6:
                    outputs[i] = Xnor(labels[2 * i], labels[(2 * i) + 1], true);
                    break;
                default:
                    outputs[i] = intgrid[labels[2 * i] - 1, labels[(2 * i) + 1] - 1];
                    break;
            }
            transmissions[i] = TX(ops[i], i);
        }
        Debug.LogFormat("[Not Connection Check #{0}] The operations are:\n[Not Connection Check #{0}] {1}", moduleID, string.Join("\n[Not Connection Check #" + moduleID + "] ", ops.Select((x, k) => "TB"[k / 2].ToString() + "LR"[k % 2].ToString() + ": (" + labels[2 * k] + " " + "+-.:/_=,"[x].ToString() + " " + labels[(2 * k) + 1] + ") \u2192 " + outputs[k]).ToArray()));
        switch (info.GetSerialNumber()[5])
        { 
            case '1':
                answer[0] = outputs.Where(x => x % 2 == 1).OrderBy(x => x).Concat(outputs.Where(x => x % 2 == 0).OrderBy(x => x)).ToArray();
                break;
            case '2':
                answer[0] = outputs.Where(x => x % 2 == 0).OrderBy(x => x).Concat(outputs.Where(x => x % 2 == 1).OrderByDescending(x => x)).ToArray();
                break;
            case '3':
                answer[0] = outputs.Where(x => x % 2 == 1).OrderByDescending(x => x).Concat(outputs.Where(x => x % 2 == 0).OrderBy(x => x)).ToArray();
                break;
            case '4':
                answer[0] = outputs.Where(x => x % 2 == 0).OrderByDescending(x => x).Concat(outputs.Where(x => x % 2 == 1).OrderByDescending(x => x)).ToArray();
                break;
            case '5':
                answer[0] = outputs.OrderByDescending(x => x).ToArray();
                break;
            case '6':
                answer[0] = outputs.Where(x => x % 2 == 0).OrderBy(x => x).Concat(outputs.Where(x => x % 2 == 1).OrderBy(x => x)).ToArray();
                break;
            case '7':
                answer[0] = outputs.Where(x => x % 2 == 1).OrderBy(x => x).Concat(outputs.Where(x => x % 2 == 0).OrderByDescending(x => x)).ToArray();
                break;
            case '8':
                answer[0] = outputs.Where(x => x % 2 == 0).OrderByDescending(x => x).Concat(outputs.Where(x => x % 2 == 1).OrderBy(x => x)).ToArray();
                break;
            case '9':
                answer[0] = outputs.Where(x => x % 2 == 1).OrderByDescending(x => x).Concat(outputs.Where(x => x % 2 == 0).OrderByDescending(x => x)).ToArray();
                break;
            default:
                answer[0] = outputs.OrderBy(x => x).ToArray();
                break;
        }
        Debug.LogFormat("[Not Connection Check #{0}] The values must be submitted in the order: {1}", moduleID, string.Join(", ", answer[0].Select(i => i.ToString()).ToArray()));
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate () { Press(b); return false; };
            if (b < 4)
                button.OnInteractEnded += delegate 
                {
                    if (!flip)
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                        button.AddInteractionPunch(0.4f);
                        StopCoroutine(transmissions[b]);
                        brends[b].material = mats[greens[b] ? 1 : 0];
                    }
                };
        }
    }

    private int Xnor(int a, int b, bool s)
    {
        int x = 0;
        for(int i = 0; i < 3; i++)
        {
            int bit = new int[3] { 1, 2, 4}[i];
            if (((a / bit) % 2 == (b / bit) % 2) == s)
                x += bit;
        }
        return x;
    }

    private IEnumerator TX(int x, int y)
    {
        int b = Random.Range(0, morse[x].Length);
        for(int i = b; i < morse[x].Length; i++)
        {
            brends[y].material = mats[greens[y] ? 0 : 1];
            yield return new WaitForSeconds(morse[x][i] == '.' ? 0.2f : 0.5f);
            brends[y].material = mats[greens[y] ? 1 : 0];
            if (i == morse[x].Length - 1)
            {
                i = -1;
                yield return new WaitForSeconds(0.6f);
            }
            else
                yield return new WaitForSeconds(0.2f);
        }
    }

    private void Press(int b)
    {
        if (!moduleSolved)
        {
            if (b == 4)
            {
                if (!flip)
                {
                    flip = true;
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[4].transform);
                    buttons[4].AddInteractionPunch();
                    for (int i = 0; i < 4; i++)
                        brends[i].material = mats[0];
                }
            }
            else
            {
                if (!flip)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, buttons[b].transform);
                    StartCoroutine(transmissions[b]);
                }
                else if (!pressed[b])
                {
                    Audio.PlaySoundAtTransform("LED" + pressed.Where(x => x == true).Count().ToString(), buttons[b].transform);
                    buttons[b].AddInteractionPunch(0.8f);
                    brends[b].material = mats[1];
                    answer[1][pressed.Where(x => x == true).Count()] = outputs[b];
                    pressed[b] = true;
                    if (pressed.All(x => x == true))
                    {
                        Debug.LogFormat("[Not Connection Check #{0}] The values were submitted in the order: {1}", moduleID, string.Join(", ", answer[1].Select(i => i.ToString()).ToArray()));
                        if (answer[0].SequenceEqual(answer[1]))
                        {
                            moduleSolved = true;
                            module.HandlePass();
                        }
                        else
                        {
                            module.HandleStrike();
                            flip = false;
                            for (int i = 0; i < 4; i++)
                            {
                                pressed[i] = false;
                                brends[i].material = mats[greens[i] ? 1 : 0];
                            }
                        }
                    }
                }
            }
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} tl/tr/bl/br [Selects LEDs. Commands can be chained with spaces if the module is in submission state.] | !{0} toggle [Presses check button.]";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        List<string> possiblecommands = new List<string> { "tl", "tr", "bl", "br" };
        if (command == "toggle")
            if (flip)
            {
                yield return "sendtochaterror!f Already in submission state.";
                yield break;
            }
            else
            {
                for(int i = 0; i < 4; i++)
                    if (tpheld[i])
                    {
                        tpheld[i] = false;
                        yield return null;
                        buttons[i].OnInteractEnded();
                        break;
                    }
                yield return null;
                buttons[4].OnInteract();
                yield break;
            }
        else if (flip)
        {
            string[] commands = command.Split(' ');
            List<int> presses = new List<int> { };
            for(int i = 0; i < commands.Length; i++)
            {
                if (!possiblecommands.Contains(commands[i]))
                {
                    yield return "sendtochaterror!f Invalid subcommand: " + commands[i];
                    yield break;
                }
                else
                {
                    presses.Add(possiblecommands.IndexOf(commands[i]));
                    if (pressed[presses[i]])
                    {
                        yield return "sendtochaterror!f " + commands[i] + " has already been submitted.";
                        yield break;
                    }
                }
            }
            if(presses.Distinct().Count() != presses.Count())
            {
                yield return "sendtochaterror!f Command contains duplicate subcommands.";
                yield break;
            }
            for(int i = 0; i < commands.Length; i++)
            {
                yield return null;
                buttons[presses[i]].OnInteract();
                yield return new WaitForSeconds(0.25f);
            }
        }
        else
        {
            if (!possiblecommands.Contains(command))
            {
                yield return "sendtochaterror!f Invalid command: " + command;
                yield break;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    if (tpheld[i])
                    {
                        tpheld[i] = false;
                        yield return null;
                        buttons[i].OnInteractEnded();
                        break;
                    }
                tpheld[possiblecommands.IndexOf(command)] = true;
                yield return null;
                buttons[possiblecommands.IndexOf(command)].OnInteract();
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (!flip)
        {
            for (int i = 0; i < 4; i++)
                if (tpheld[i])
                {
                    tpheld[i] = false;
                    yield return null;
                    buttons[i].OnInteractEnded();
                    break;
                }
            yield return null;
            buttons[4].OnInteract();
        }
        for(int i = 0; i < 4; i++)
        {
            pressed[i] = false;
            brends[i].material = mats[0];
        }
        for(int i = 0; i < 4; i++)
        {
            yield return null;
            buttons[outputs.Select((x, k) => pressed[k] ? -1 : x).ToList().IndexOf(answer[0][i])].OnInteract();
            yield return new WaitForSeconds(0.25f);
        }
    }
}
