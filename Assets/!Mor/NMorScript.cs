using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class NMorScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public GameObject[] tobjects;
    public GameObject transmissionswitch;
    public GameObject house;
    public GameObject lid;
    public GameObject hinge;
    public GameObject tbacking;
    public KMSelectable[] tbuttons;
    public KMSelectable tswitch;
    public KMSelectable tsubmit;
    public Renderer[] leds;
    public Renderer[] trends;
    public Material[] io;
    public Material[] tmats;
    public TextMesh[] displays;

    private readonly string[] keywords = new string[204] {
        "ABORT", "AFTER", "AGONY", "ALIGN", "AMONG", "AMBER", "ANGST", "AZURE",
        "BAKER", "BAYOU", "BEACH", "BLACK", "BOGUS", "BOXES", "BRASH", "BUDGE",
        "CABLE", "CAULK", "CHIEF", "CLOVE", "CODEX", "CRAZE", "CRISP", "CRUEL",
        "DECOY", "DEPTH", "DISCO", "DITCH", "DOUGH", "DRAWS", "DREAM", "DWARF",
        "EARLY", "EIGHT", "ELBOW", "ENJOY", "EPOCH", "EQUIP", "EVICT", "EXACT",
        "FACET", "FALSE", "FIBRE", "FJORD", "FLAKE", "FLAWS", "FRESH", "FUNGI",
        "GENUS", "GHOST", "GLOBE", "GOURD", "GRAPH", "GRAVY", "GROWS", "GUIDE",
        "HAIKU", "HAVOC", "HELIX", "HERTZ", "HONEY", "HOTEL", "HUMID", "HYDRA",
        "IAMBS", "ICHOR", "IMAGE", "INDEX", "INGOT", "IRATE", "IRONS", "ITCHY",
        "JACKS", "JAUNT", "JERKY", "JIVES", "JOKER", "JOUST", "JUICE", "JUMBO",
        "KANJI", "KAPUT", "KENDO", "KETCH", "KLUTZ", "KNAVE", "KNOWS", "KUGEL",
        "LARGE", "LAWNS", "LIGHT", "LIMBO", "LOCUS", "LOUSY", "LUNCH", "LYRIC",
        "MACHO", "MAGIC", "MAJOR", "MAZES", "MERCY", "MIXER", "MOTIF", "MUSIC",
        "NEXUS", "NICHE", "NIGHT", "NODAL", "NOTCH", "NOVEL", "NURSE", "NYMPH",
        "OCEAN", "OFTEN", "OGHAM", "OLIVE", "OMEGA", "ONSET", "ORBIT", "OTHER",
        "PANIC", "PHONE", "PIANO", "PIVOT", "PLUMB", "POLAR", "PRAWN", "PRISM",
        "QOPHS", "QUACK", "QUALM", "QUERY", "QUINT", "QUIRK", "QUITS", "QUOTA",
        "RADIX", "RAINY", "RECON", "RHYME", "RIVAL", "ROAST", "ROUND", "RULES",
        "SALTY", "SCARF", "SCHWA", "SHAPE", "SOLID", "SPIKY", "SQUID", "STRAW",
        "TABLE", "TAWNY", "THIRD", "TILDE", "TOPIC", "TORUS", "TREND", "TWEAK",
        "UMBRA", "UNARY", "UNBOX", "UNCLE", "UNIFY", "UNZIP", "UPSET", "URBAN",
        "VAULT", "VENOM", "VIDEO", "VINYL", "VIXEN", "VOICE", "VOLTS", "VOWEL",
        "WAFER", "WEIRD", "WHELK", "WITCH", "WORMS", "WOVEN", "WRATH", "WRONG",
        "XENIA", "XERIC", "XYLEM", "XYSTI",
        "YACHT", "YEARS", "YIELD", "YODEL", "YOLKS", "YOUNG", "YOUTH", "YOWLS",
        "ZEALS", "ZEBRA", "ZEROS", "ZESTY", "ZILCH", "ZINGY", "ZLOTY", "ZONED"};
    private string[] word = new string[2];
    private int[] order = new int[5];
    private int[][] tets = new int[2][] { new int[4], new int[4] };
    private int[][] codes = new int[3][] { new int[7], new int[7], new int[8] };
    private List<int>[] answer = new List<int>[2] { new List<int> { }, new List<int> { } };
    private IEnumerator transmit;
    private bool transmitting = true;
    private bool[] submission = new bool[2];

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
        word[0] = keywords[Random.Range(0, 204)];
        order = new int[5] { 0, 1, 2, 3, 4 }.Shuffle();
        word[1] = string.Join("", order.Select(x => word[0][x].ToString()).ToArray());
        transmit = Transmit(word[1]);
        StartCoroutine(transmit);
        Debug.LogFormat("[Not Morsematics #{0}] The transmission reads \"{1}\"", moduleID, word[1]);
        Debug.LogFormat("[Not Morsematics #{0}] The unscrambled word is \"{1}\"", moduleID, word[0]);
        if (word[1] == string.Join("", word[0].OrderBy(x => x).Select(x => x.ToString()).ToArray()) || word[1] == string.Join("", word[0].OrderByDescending(x => x).Select(x => x.ToString()).ToArray()))
            codes[0] = new int[7] { 1, 2, 3, 4, 5, 6, 0 };
        else if (word[1].Where((x, k) => k < 4 && Mathf.Abs(x - word[1][k + 1]) == 1).Count() > 0)
            codes[0] = new int[7] { 3, 6, 2, 0, 1, 4, 5 };
        else if (word[1].Where((x, k) => k < 4 && Mathf.Abs(x - word[1][k + 1]) > 9).Count() < 1)
            codes[0] = new int[7] { 4, 1, 0, 6, 3, 5, 2 };
        else if (word[1].Where((x, k) => x == word[0][k]).Count() == 1)
            codes[0] = new int[7] { 2, 4, 6, 1, 0, 5, 3 };
        else if ("AEIOU".Contains(word[1][0].ToString()))
            codes[0] = new int[7] { 5, 2, 4, 3, 6, 0, 1 };
        else if ("AEIOU".Contains(word[1][4].ToString()))
            codes[0] = new int[7] { 6, 5, 1, 2, 4, 3, 0 };
        else if (word[1].Where((x, k) => x == word[0][k]).Count() < 1)
            codes[0] = new int[7] { 4, 3, 2, 1, 0, 6, 5 };
        else
            codes[0] = new int[7] { 0, 1, 2, 3, 4, 5, 6, };
        Debug.LogFormat("[Not Morsematics #{0}] The shape subsequence is {1}", moduleID, string.Join("", codes[0].Select(x => "IJZTOSL"[x].ToString()).ToArray()));
        if ((word[1] == word[0]) || (word[1] == string.Join("", word[0].Select((x, k) => word[0][4 - k].ToString()).ToArray())))
            codes[1] = new int[7] { 3, 0, 4, 6, 5, 2, 1 };
        else if (word[1].Where((x, k) => "AEIOU".Contains(x.ToString()) && x == word[0][k]).Count() > 0)
            codes[1] = new int[7] { 2, 1, 0, 5, 4, 3, 6 };
        else if (word[1][2] == word[0][2])
            codes[1] = new int[7] { 5, 1, 2, 6, 4, 3, 0 };
        else if (word[1][0] == word[0][4] || word[1][4] == word[0][0])
            codes[1] = new int[7] { 6, 4, 1, 0, 3, 5, 2 };
        else if ("AEIOUY".Contains(word[0][4]))
            codes[1] = new int[7] { 4, 0, 3, 5, 2, 6, 1 };
        else if ("AEIOUY".Contains(word[0][0]))
            codes[1] = new int[7] { 3, 2, 4, 1, 6, 0, 5 };
        else if (word[1].Where((x, k) => !"AEIOU".Contains(x.ToString()) && x == word[0][k]).Count() > 0)
            codes[1] = new int[7] { 4, 6, 1, 5, 0, 3, 2 };
        else
            codes[1] = new int[7] { 0, 1, 2, 3, 4, 5, 6 };
        Debug.LogFormat("[Not Morsematics #{0}] The colour subsequence is {1}", moduleID, string.Join("", codes[1].Select(x => "VMRBGCY"[x].ToString()).ToArray()));
        tswitch.OnInteract += delegate ()
        {
            if (!moduleSolved)
            {
                if (!submission[0] && !submission[1])
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, tswitch.transform);
                    tswitch.transform.localPosition = new Vector3(-tswitch.transform.localPosition.x, 0.01925f, 0.086f);
                    if (transmitting)
                    {
                        StopCoroutine(transmit);
                        for (int i = 0; i < 3; i++)
                            leds[i].material = io[1];
                    }
                    else
                        StartCoroutine(transmit);
                    transmitting ^= true;
                }
                else
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, tswitch.transform);
                    tswitch.AddInteractionPunch(0.5f);
                }
            }
            return false;
        };
        tsubmit.OnInteract += delegate ()
        {
            if (!submission[0] && !moduleSolved)
            {
                submission[0] = true;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, tswitch.transform);
                tsubmit.AddInteractionPunch();
                if (submission[1])
                {
                    StopCoroutine("Countdown");
                    StartCoroutine(MoveHatch(false));
                }
                else
                {
                    if (transmitting)
                    {
                        StopCoroutine(transmit);
                        for (int i = 0; i < 3; i++)
                            leds[i].material = io[1];
                        tswitch.transform.localPosition = new Vector3(-0.0064f, 0.01925f, 0.086f);
                        transmitting = false;
                    }
                    Generate();
                    StartCoroutine(MoveHatch(false));
                }
            }
            return false;
        };
        foreach (KMSelectable button in tbuttons)
        {
            int b = Array.IndexOf(tbuttons, button);
            button.OnInteract += delegate () { TPress(b); return false; };
            button.OnHighlight += delegate () { displays[1].text = "VMRBGCY"[tets[1][Array.IndexOf(tets[0], b)]].ToString(); };
            button.OnHighlightEnded += delegate () { displays[1].text = string.Empty; };
        }
        for (int i = 0; i < 7; i++)
            tobjects[i].SetActive(false);
    }

    private void Generate()
    {
        answer[0].Clear();
        answer[1].Clear();
        for (int i = 0; i < 2; i++)
            tets[i] = new int[] { 0, 1, 2, 3, 4, 5, 6 }.Shuffle();
        for (int i = 0; i < 7; i++)
        {
            tobjects[tets[0][i]].SetActive(true);
            trends[tets[0][i]].material = tmats[tets[1][i]];
            if (i > 3)
                tobjects[tets[0][i]].SetActive(false);
        }
        List<int>[] presenttets = new List<int>[2];
        for (int i = 0; i < 2; i++)
            presenttets[i] = tets[i].Where((x, k) => k < 4).ToList();
        Debug.LogFormat("[Not Morsematics #{0}] The present pieces are: {1}", moduleID, string.Join(", ", presenttets[0].Select((x, k) => new string[7] { "Violet", "Magenta", "Red", "Blue", "Green", "Cyan", "Yellow" }[presenttets[1][k]] + " " + "IJZTOSL"[x].ToString()).ToArray()));
        if (info.GetSerialNumberLetters().Any(x => word[1].Contains(x.ToString())))
            answer[0] = MasterCommand("A1B2C3D4");
        else if (string.Join("", info.GetOnIndicators().ToArray()).Any(x => word[1].Contains(x.ToString())))
            answer[0] = MasterCommand("12AB34CD");
        else if (string.Join("", info.GetOffIndicators().ToArray()).Any(x => word[1].Contains(x.ToString())))
            answer[0] = MasterCommand("AB12CD34");
        else if (info.GetIndicators().Count() == 0)
            answer[0] = MasterCommand("1A2B3C4D");
        else if (info.GetBatteryCount() - info.GetBatteryHolderCount() < 2)
            answer[0] = MasterCommand("1234ABCD");
        else
            answer[0] = MasterCommand("ABCD1234");
        List<string> logconds = new List<string> { };
        if (!presenttets[0].Contains(4) && !presenttets[1].Contains(0))
        {
            int x = answer[0][0];
            answer[0][0] = answer[0][7];
            answer[0][7] = x;
            logconds.Add("1");
        }
        if (presenttets[0].Any(x => (x == 2 || x == 5) && presenttets[1][presenttets[0].IndexOf(x)] == 1))
        {
            int x = answer[0][1];
            answer[0][1] = answer[0][2];
            answer[0][2] = x;
            logconds.Add("2");
        }
        if (presenttets[1].Any(x => x > 4 && presenttets[0][presenttets[1].IndexOf(x)] == 0))
        {
            int x = answer[0][3];
            answer[0][3] = answer[0][5];
            answer[0][5] = x;
            logconds.Add("3");
        }
        if (presenttets[1].All(x => x != 2 && x != 3))
        {
            int x = answer[0][2];
            answer[0][2] = answer[0][4];
            answer[0][4] = x;
            logconds.Add("4");
        }
        if (presenttets[0].All(x => x != 1 && x != 6))
        {
            int x = answer[0][1];
            answer[0][1] = answer[0][6];
            answer[0][6] = x;
            logconds.Add("5");
        }
        if (presenttets[0].Any(x => (x == 3 || x == 4) && presenttets[1][presenttets[0].IndexOf(x)] == 3))
        {
            int x = answer[0][3];
            answer[0][3] = answer[0][6];
            answer[0][6] = x;
            logconds.Add("6");
        }
        if (!presenttets[1].Contains(4))
        {
            int x = answer[0][0];
            answer[0][0] = answer[0][4];
            answer[0][4] = x;
            logconds.Add("7");
        }
        if (!presenttets[0].Contains(0))
        {
            int x = answer[0][5];
            answer[0][5] = answer[0][7];
            answer[0][7] = x;
            logconds.Add("8");
        }
        Debug.LogFormat("[Not Morsematics #{0}] {1}Condition{2}{3} appl{4}", moduleID, logconds.Count() == 0 ? "No " : "", logconds.Count() == 1 ? "" : "s", logconds.Count() == 0 ? "" : " " + string.Join(", ", logconds.ToArray()), logconds.Count() == 1 ? "ies" : "y");
        Debug.LogFormat("[Not Morsematics #{0}] The keys should be pressed in this order: {1}", moduleID, string.Join("", answer[0].Select(i => "IJZTOSL"[i].ToString()).ToArray()));
    }

    private List<int> MasterCommand(string order)
    {
        List<int> x = new List<int> { };
        string y = string.Empty;
        int[][] subcommands = new int[2][] { new int[4], new int[4] };
        for (int i = 0; i < 2; i++)
            subcommands[i] = codes[i].Where(p => tets[i].Where((q, k) => k < 4).Contains(p)).ToArray();
        for (int i = 0; i < 8; i++)
        {
            if ("ABCD".Contains(order[i].ToString()))
            {
                int a = subcommands[0]["ABCD".IndexOf(order[i])];
                x.Add(a);
                y += "IJZTOSL"[a];
            }
            else
            {
                int a = tets[0][Array.IndexOf(tets[1], subcommands[1]["1234".IndexOf(order[i])])];
                int b = subcommands[1]["1234".IndexOf(order[i])];
                x.Add(a);
                y += "VMRBGCY"[b];
            }
        }
        Debug.LogFormat("[Not Morsematics #{0}] The master command is {1}", moduleID, y);
        return x;
    }

    private IEnumerator Transmit(string t)
    {
        bool[][,] letters = new bool[26][,]
        {
            new bool[5,3]{ { false, true, false}, { true, false, true}, { true, true, true}, { true, false, true}, { true, false, true} },
            new bool[5,3]{ { true, true, false}, { true, false, true}, { true, true, false}, { true, false, true}, { true, true, false} },
            new bool[5,3]{ { false, true, false}, { true, false, true}, { true, false, false}, { true, false, true}, { false, true, false} },
            new bool[5,3]{ { true, true, false}, { true, false, true }, { true, false, true }, { true, false, true }, { true, true, false } },
            new bool[5,3]{ { true, true, true}, { true, false, false}, { true, true, true}, { true, false, false}, { true, true, true} },
            new bool[5,3]{ { true, true, true}, { true, false, false}, { true, true, true}, { true, false, false}, { true, false, false} },
            new bool[5,3]{ { false, true, true}, { true, false, false}, { true, false, true}, { true, false, true}, { false, true, true} },
            new bool[5,3]{ { true, false, true }, { true, false, true }, { true, true, true}, { true, false, true }, { true, false, true } },
            new bool[5,3]{ { true, true, true}, { false, true, false}, { false, true, false}, { false, true, false}, { true, true, true} },
            new bool[5,3]{ { true, true, true}, { false, false, true}, { false, false, true}, { false, false, true}, { true, true, false } },
            new bool[5,3]{ { true, false, true}, { true, true, false}, { true, false, false}, { true, true, false}, { true, false, true} },
            new bool[5,3]{ { true, false, false}, { true, false, false}, { true, false, false}, { true, false, false}, { true, true, true} },
            new bool[5,3]{ { true, false, true}, { true, true, true}, { true, false, true},{  true, false, true}, { true, false, true} },
            new bool[5,3]{ { true, true, true}, { true, false, true}, { true, false, true},{  true, false, true}, { true, false, true} },
            new bool[5,3]{ { false, true, false}, { true, false, true}, { true, false, true}, { true, false, true}, { false, true, false} },
            new bool[5,3]{ { true, true, false}, { true, false, true}, { true, true, false}, { true, false, false}, { true, false, false} },
            new bool[5,3]{ { false, true, false}, { true, false, true}, { true, false, true}, { false, true, false}, { false, false, true} },
            new bool[5,3]{ { true, true, false}, { true, false, true}, { true, true, false}, { true, true, false}, { true, false, true} },
            new bool[5,3]{ { false, true, true}, { true, false, false}, { false, true, false}, { false, false, true}, { true, true, false} },
            new bool[5,3]{ { true, true, true}, { false, true, false}, { false, true, false}, { false, true, false}, { false, true, false} },
            new bool[5,3]{ { true, false, true }, { true, false, true }, { true, false, true}, { true, false, true }, { true, true, true } },
            new bool[5,3]{ { true, false, true }, { true, false, true }, { true, false, true}, { true, false, true }, { false, true, false } },
            new bool[5,3]{ { true, false, true }, { true, false, true }, { true, false, true}, { true, true, true }, { true, false, true } },
            new bool[5,3]{ { true, false, true }, { true, false, true }, { false, true, false}, { true, false, true }, { true, false, true } },
            new bool[5,3]{ { true, false, true }, { true, false, true }, { false, true, false}, { false, true, false}, { false, true, false} },
            new bool[5,3]{ { true, true, true}, { false, false, true}, { false, true, false}, { true, false, false}, { true, true, true} }
        };
        bool[][,] display = new bool[5][,] { new bool[5, 3], new bool[5, 3], new bool[5, 3], new bool[5, 3], new bool[5, 3] };
        for (int i = 0; i < 5; i++)
            display[i] = letters[t[i] - 'A'];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 3; k++)
                    leds[k].material = io[display[i][j, k] ? 0 : 1];
                yield return new WaitForSeconds(0.33f);
            }
            for (int k = 0; k < 3; k++)
                leds[k].material = io[1];
            yield return new WaitForSeconds(0.33f);
            if (i == 4)
            {
                i = -1;
                yield return new WaitForSeconds(0.67f);
            }
        }
    }

    private IEnumerator MoveHatch(bool autosolve)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        if (submission[1])
        {
            displays[2].text = "--";
            if (!autosolve)
            {
                if (answer[1].Count() < 1)
                    Debug.LogFormat("[Not Morsematics #{0}] Submitted nothing.", moduleID);
                else
                    Debug.LogFormat("[Not Morsematics #{0}] Submitted keypresses {1}", moduleID, string.Join("", answer[1].Select(x => "IJZTOSL"[x].ToString()).ToArray()));
            }
            else
                Debug.LogFormat("[Not Morsematics #{0}] Incorrect keypress detected. Forcing correct submission due to autosolve.", moduleID);
            StopCoroutine("Countdown");
            bool correct = answer[0].SequenceEqual(answer[1]);
            if (correct || autosolve)
            {
                moduleSolved = true;
                Audio.PlaySoundAtTransform("InputCorrect", transform);
                displays[0].color = new Color(0, 1, 0);
            }
            else
            {
                module.HandleStrike();
                displays[0].color = new Color(1, 0, 0);
            }
            for (int i = 0; i < 10; i++)
            {
                lid.transform.RotateAround(hinge.transform.position, hinge.transform.right, 18);
                yield return new WaitForSeconds(0.05f);
            }
            house.transform.localPosition -= new Vector3(0, 0.008f, 0);
            if (correct || autosolve)
                module.HandlePass();
            else
            {
                tswitch.transform.localPosition = new Vector3(0.0064f, 0.01925f, 0.086f);
                transmitting = true;
                StartCoroutine(transmit);
            }
            submission[0] = false;
            submission[1] = false;
            for (int i = 0; i < 7; i++)
                tobjects[i].SetActive(false);
        }
        else
        {
            displays[2].text = "99";
            displays[0].color = new Color(1, 1, 1);
            displays[0].text = string.Empty;
            house.transform.localPosition += new Vector3(0, 0.008f, 0);
            for (int i = 0; i < 10; i++)
            {
                lid.transform.RotateAround(hinge.transform.position, hinge.transform.right, -18);
                yield return new WaitForSeconds(0.05f);
            }
            submission[0] = false;
            submission[1] = true;
            StartCoroutine("Countdown");
        }
    }

    private IEnumerator Countdown()
    {
        int time = 99;
        for (int i = 0; i < 99; i++)
        {
            displays[2].text = (time < 10 ? "0" : "") + time.ToString();
            yield return new WaitForSeconds(1);
            time--;
        }
        displays[2].text = "00";
        Debug.LogFormat("[Not Morsematics #{0}] Time's up. Auto-submitting.", moduleID);
        StartCoroutine(MoveHatch(false));
    }

    private void TPress(int b)
    {
        if (!moduleSolved && submission[1])
        {
            Audio.PlaySoundAtTransform("Keypress", tbuttons[b].transform);
            tbuttons[b].AddInteractionPunch(0.8f);
            if (answer[1].Count() < 8)
            {
                answer[1].Add(b);
                displays[0].text += "AMHDBKJ"[b].ToString();
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} activate [Opens hatch] | !{0} submit <IJZTOSL> [Enters sequence of tetris pieces] | !{0} toggle [Flips transmission switch]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (command == "toggle")
        {
            if (submission[0] || submission[1])
            {
                yield return "sendtochaterror!f Cannot toggle transmission in this state.";
                yield break;
            }
            else
            {
                yield return null;
                tswitch.OnInteract();
            }
        }
        else if (command == "activate")
        {
            if (submission[0] || submission[1])
            {
                yield return "sendtochaterror!f Submission state is already active.";
                yield break;
            }
            else
            {
                yield return null;
                tsubmit.OnInteract();
            }
        }
        else
        {
            if (submission[0] || !submission[1])
            {
                yield return "sendtochaterror!f Cannot input sequence in transmission state.";
                yield break;
            }
            string[] commands = command.Split(' ');
            if (commands.Length == 2 && commands[0] == "submit")
            {
                if (commands[1].Length != 8)
                {
                    yield return "sendtochaterror!f Input sequence must contain exactly 8 entries.";
                    yield break;
                }
                commands[1] = commands[1].ToUpperInvariant();
                string possibleinputs = string.Join("", tets[0].Select(x => "IJZTOSL"[x].ToString()).Where((x, k) => k < 4).ToArray());
                for (int i = 0; i < 8; i++)
                    if (!possibleinputs.Contains(commands[1][i].ToString()))
                    {
                        yield return "sendtochaterror " + commands[1][i].ToString() + " is not a present key shape.";
                        yield break;
                    }
                for (int i = 0; i < 8; i++)
                {
                    yield return null;
                    tbuttons["IJZTOSL".IndexOf(commands[1][i].ToString())].OnInteract();
                    yield return new WaitForSeconds(0.05f);
                }
                yield return null;
                yield return "solve";
                tsubmit.OnInteract();
            }
            else
                yield return "sendtochaterror!f Invalid submission command.";
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (!moduleSolved)
        {
            if (!submission[0] && submission[1])
            {
                for (int i = 0; i < answer[1].Count; i++)
                {
                    if (answer[1][i] != answer[0][i])
                    {
                        StartCoroutine(MoveHatch(true));
                        while (submission[1]) yield return true;
                        yield break;
                    }
                }
            }
            while (submission[0] && submission[1])
                yield return true;
            if (!submission[1])
            {
                if (!submission[0])
                {
                    yield return null;
                    tsubmit.OnInteract();
                }
                while (submission[0])
                    yield return null;
            }
            int start = answer[1].Count;
            for (int i = start; i < 8; i++)
            {
                yield return null;
                tbuttons[answer[0][i]].OnInteract();
                yield return new WaitForSeconds(0.05f);
            }
            yield return null;
            tsubmit.OnInteract();
        }
        while (submission[1]) yield return true;
    }
}
