using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class XCFScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] buttons;
    public TextMesh display;

    private readonly string[] disptext = new string[8] { "BLACK", "BLUE", "GREEN", "CYAN", "RED", "MAGENTA", "YELLOW", "WHITE" };
    private readonly Color[] dispcols = new Color[8] { new Color(0.33f, 0.33f, 0.33f), new Color(0, 0, 1), new Color(0, 1, 0), new Color(0, 1, 1), new Color(1, 0, 0), new Color(1, 0, 1), new Color(1, 1, 0), new Color(1, 1, 1) };
    private readonly string[] morse = new string[36] { "###-###-###-###-###", "#-###-###-###-###", "#-#-###-###-###", "#-#-#-###-###", "#-#-#-#-###", "#-#-#-#-#", "###-#-#-#-#", "###-###-#-#-#", "###-###-###-#-#", "###-###-###-###-#", "#-###", "###-#-#-#", "###-#-###-#", "###-#-#", "#", "#-#-###-#", "###-###-#", "#-#-#-#", "#-#", "#-###-###-###", "###-#-###", "#-###-#-#", "###-###", "###-#", "###-###-###", "#-###-###-#", "###-###-#-###", "#-###-#", "#-#-#", "###", "#-#-###", "#-#-#-###", "#-###-###", "###-#-#-###", "###-#-###-###", "###-###-#-#" };
    private readonly string[] functions = new string[8] { "SLIM", "15BRO", "20DGT", "34XYZ", "6WUF", "7HPJ", "8CAKE", "9QVN"};
    private int[] startvals = new int[6];
    private string[] transmissions = new string[6];
    private int[] dispints = new int[2];
    private int[] offsets = new int[6];
    private int[] finalvals = new int[4];
    private int starttime;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnActivate = Activate;
        for (int k = 0; k < 2; k++)
        {
            bool accept = false;
            while (!accept)
            {
                for (int i = 0; i < 3; i++)
                {
                    startvals[i + (3 * k)] = Random.Range(0, 36);
                    transmissions[i + (3 * k)] = morse[startvals[i + (3 * k)]] + "---";
                    offsets[i + (3 * k)] = Random.Range(0, transmissions[i + (3 * k)].Length);
                }
                int f = transmissions.Where((x, p) => p / 3 == k).Aggregate(1, (a, b) => a * b.Length);
                bool[] check = new bool[8];
                List<string> t = new List<string> { "---", "--#", "-#-", "-##", "#--", "#-#", "##-", "###" };
                for (int i = 0; i < f; i++)
                {
                    string s = string.Empty;
                    for (int j = 0; j < 3; j++)
                        s += transmissions[j + (3 * k)][(i + offsets[j + (3 * k)]) % transmissions[j + (3 * k)].Length].ToString();
                    check[t.IndexOf(s)] = true;
                    if (check.All(x => x))
                    {
                        accept = true;
                        Debug.LogFormat("[Cruel Colour Flash #{0}] The received {2} characters are: {1}", moduleID, string.Join(" ", startvals.Where((x, p) => p / 3 == k).Select(x => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString()).ToArray()), new string[] { "word", "colour" }[k]);
                        break;
                    }
                }
            }
        }
    }

    private void Activate()
    {
        starttime = (int)info.GetTime();
        int[] rule = new int[3];
        StartCoroutine(Transmit(transmissions));
        if (startvals.Where((p, q) => q < 3).Distinct().Count() < 3)
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i + 3], startvals[i], i + 3);
        else if (startvals.Where((p, q) => q > 2).Distinct().Count() < 3)
        {
            rule[0] = 1;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i + 3], startvals[i], i + 3);
        }
        else if (startvals.Distinct().Count() < 5)
        {
            rule[0] = 2;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i], startvals[i + 3], i);
        }
        else if (startvals.Count(x => x < 10) > 2)
        {
            rule[0] = 3;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i + 3], startvals[i], i + 3);
        }
        else if (startvals.All(x => x > 9))
        {
            rule[0] = 4;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i], startvals[i + 3], i);
        }
        else if (startvals.Count(x => info.GetSerialNumber().Contains("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString())) > 1)
        {
            rule[0] = 5;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i + 3], startvals[i], i + 3);
        }
        else if (startvals.All(x => !info.GetSerialNumber().Contains("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString())))
        {
            rule[0] = 6;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i], startvals[i + 3], i);
        }
        else if ("0123456789".Contains(info.GetSerialNumber()[info.GetBatteryCount() % 6].ToString()))
        {
            rule[0] = 7;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i + 3], startvals[i], i + 3);
        }
        else
        {
            rule[0] = 8;
            for (int i = 0; i < 3; i++)
                finalvals[i] = Charfunction(startvals[i], startvals[i + 3], i);
        }
        Debug.LogFormat("[Cruel Colour Flash #{0}] The {1} function rule applies.", moduleID, new string[] { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th" }[rule[0]]);
        Debug.LogFormat("[Cruel Colour Flash #{0}] The output characters are: {1}", moduleID, string.Join(" ", finalvals.Where((x, p) => p < 3).Select(x => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString()).ToArray()));
        int[] f = finalvals.Where((p, q) => q < 3).ToArray();
        if (f.Distinct().Count() < 2)
            finalvals[3] = f[0];
        else if (f.Distinct().Count() < 3)
        {
            rule[1] = 1;
            finalvals[3] = f.Where(x => x != f.OrderBy(y => y).ToArray()[1]).ToArray()[0];
        }
        else if (f.Count(x => startvals.Contains(x)) == 2)
        {
            rule[1] = 2;
            int[] g = f.Where(x => startvals.Contains(x)).ToArray();
            finalvals[3] = g.Sum() % 36;
        }
        else if (f.Count(x => startvals.Contains(x)) == 1)
        {
            rule[1] = 3;
            int[] g = f.Where(x => !startvals.Contains(x)).ToArray();
            finalvals[3] = Mathf.Abs(g[0] - g[1]);
        }
        else if (f.Sum() > 74)
        {
            rule[1] = 4;
            finalvals[3] = f[0];
        }
        else if (startvals.Sum() > 164)
        {
            rule[1] = 5;
            finalvals[3] = f[1];
        }
        else if (startvals.Sum() < f.Sum() * 2)
        {
            rule[1] = 6;
            finalvals[3] = f[2];
        }
        else if (f.Any(x => info.GetSerialNumber().Contains("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString())))
        {
            rule[1] = 7;
            finalvals[3] = f.OrderBy(x => x).ToArray()[1];
        }
        else if (f.SequenceEqual(f.OrderBy(x => x)))
        {
            rule[1] = 8;
            finalvals[3] = f[2] - f[0];
        }
        else if (f.SequenceEqual(f.OrderByDescending(x => x)))
        {
            rule[1] = 8;
            finalvals[3] = f[0] - f[2];
        }
        else if (f.Max() > startvals.Where((p, q) => q < 3).Max())
        {
            rule[1] = 9;
            finalvals[3] = f.Min();
        }
        else if (f.Min() > startvals.Where((p, q) => q > 2).Min())
        {
            rule[1] = 10;
            finalvals[3] = f.Max();
        }
        else
        {
            rule[1] = 11;
            finalvals[3] = ((f.Sum() - 1) % 35) + 1;
        }   
        Debug.LogFormat("[Cruel Colour Flash #{0}] The {1} final value rule applies.", moduleID, new string[] { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th"}[rule[1]]);
        Debug.LogFormat("[Cruel Colour Flash #{0}] The final character is {1}", moduleID, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[finalvals[3]].ToString());
        if(finalvals[3] < 10)
        {
            if (startvals.Contains(finalvals[3]))
                rule[2] = 0;
            else if (info.GetSerialNumberNumbers().Sum() < finalvals[3])
                rule[2] = 1;
            else if ((starttime / 60) % 10 == finalvals[3])
                rule[2] = 2;
            else if (info.GetPortCount() > finalvals[3])
                rule[2] = 3;
            else if (info.GetModuleNames().Count() % 10 == finalvals[3])
                rule[2] = 4;
            else if (info.GetBatteryCount() > finalvals[3])
                rule[2] = 5;
            else
                rule[2] = 6;
            Debug.LogFormat("[Cruel Colour Flash #{0}] The {1} button rule applies.{2}", moduleID, new string[] { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th"}[rule[2]], rule[2] == 6 ? " (Solve dependent)" : "");
        }
        buttons[0].OnInteract = delegate () { Press(0, finalvals[3]); return false; };
        buttons[1].OnInteract = delegate () { Press(1, finalvals[3]); return false; };
    }

    private IEnumerator Transmit(string[] t)
    {
        int c = 0;
        int f = t.Aggregate(1, (a, b) => a * b.Length);
        int[] bits = new int[3] { 4, 2, 1 };
        while (!moduleSolved)
        {
            int[] v = new int[2];
            for (int i = 0; i < 6; i++)
                if (t[i][(c + offsets[i]) % t[i].Length] == '#')
                    v[i / 3] += bits[i % 3];
            display.text = disptext[v[0]];
            display.color = dispcols[v[1]];
            dispints = v;
            yield return new WaitForSeconds(0.66f);
            c = (c + 1) % f;
        }
    }

    private int Charfunction(int x, int y, int k)
    {
        switch (Array.FindIndex(functions, s => s.Contains("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[y].ToString())))
        {
            case 0:
                return x % 2 == 0 ? x / 2 : (x * 2) % 36;
            case 1:
                return k < 3 ? startvals[(k + 1) % 3] : startvals[((k + 1) % 3) + 3];
            case 2:
                int m = string.Join("", startvals.Where((p, q) => q / 3 == k / 3).Select(p => morse[p]).ToArray()).Count(p => p == '-') + 3;
                return x < 18 ? (x + m) % 36 : x - m;
            case 3:
                return 35 - x;
            case 4:
                return (x += startvals.Where((p, q) => q / 3 == k / 3).Min()) % 36;
            case 5:
                return x % 3 == 0 ? x / 3 : (x + startvals.Where((p, q) => q / 3 == k / 3 && p < 10).Sum()) % 36; 
            case 6:
                return (x * (startvals.Count(p => p < 10) + 1)) % 36; 
            default:
                return 35 - (k < 3 ? startvals[(k + 2) % 3] : startvals[((k + 2) % 3) + 3]);
        }
    }

    private void Press(int b, int f)
    {
        if (!moduleSolved)
        {
            buttons[b].AddInteractionPunch(0.2f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[b].transform);
            Debug.LogFormat("[Cruel Colour Flash #{0}] The {1} button was pressed at {2} when the word \"{3}\" was displayed in the colour {4}", moduleID, new string[] { "Yes", "No" }[b], info.GetFormattedTime(), disptext[dispints[0]].ToLowerInvariant(), disptext[dispints[1]].ToLowerInvariant());
            switch (f)
            {
                case 10:
                    moduleSolved = b == 0 && dispints[0] == 7;
                    break;
                case 11:
                    moduleSolved = b == 0 && dispints[0] % 2 == 1 && dispints[1] % 2 == 1;
                    break;
                case 12:
                    moduleSolved = b == 1 && (int)info.GetTime() % 7 == 0;
                    break;
                case 13:
                    moduleSolved = b == 1 && dispints[1] == 0;
                    break;
                case 14:
                    moduleSolved = b == 0 && new int[] { 1, 2, 4 }.Contains(dispints[0]) && new int[] { 3, 5, 6 }.Contains(dispints[1]);
                    break;
                case 15:
                    moduleSolved = b == 1 && dispints[1] == 7;
                    break;
                case 16:
                    moduleSolved = b == 0 && (dispints[0] / 2) % 2 == 1 && (dispints[1] / 2) % 2 == 1;
                    break;
                case 17:
                    moduleSolved = b == 1 && new int[] { 3, 5, 6 }.Contains(dispints[0]) && new int[] { 3, 5, 6 }.Contains(dispints[1]);
                    break;
                case 18:
                    moduleSolved = b == 1 && new int[] { 1, 2, 4 }.Contains(dispints[0]) && new int[] { 1, 2, 4 }.Contains(dispints[1]);
                    break;
                case 19:
                    moduleSolved = b == 0 && ((int)info.GetTime() / 60) % 2 == 0 && (int)info.GetTime() % 2 == 0;
                    break;
                case 20:
                    moduleSolved = b == 0 && dispints[0] == 0;
                    break;
                case 21:
                    moduleSolved = b == 1 && new int[] { 3, 5, 6 }.Contains(dispints[0]) && new int[] { 1, 2, 4 }.Contains(dispints[1]);
                    break;
                case 22:
                    moduleSolved = b == 0 && ((int)info.GetTime() % 60) < 10;
                    break;
                case 23:
                    moduleSolved = b == 1 && dispints[0] == dispints[1];
                    break;
                case 24:
                    moduleSolved = b == 1 && dispints[0] == 0;
                    break;
                case 25:
                    moduleSolved = b == 0 && new int[] { 1, 2, 4 }.Contains(dispints[0]) && new int[] { 1, 2, 4 }.Contains(dispints[1]);
                    break;
                case 26:
                    moduleSolved = b == 1 && ((int)info.GetTime() / 60) % 2 == 0 && (int)info.GetTime() % 2 == 1;
                    break;
                case 27:
                    moduleSolved = b == 0 && dispints[0] > 3 && dispints[1] > 3;
                    break;
                case 28:
                    moduleSolved = b == 0 && new int[] { 3, 5, 6 }.Contains(dispints[0]) && new int[] { 3, 5, 6 }.Contains(dispints[1]);
                    break;
                case 29:
                    moduleSolved = b == 1 && Mathf.Abs(starttime - (int)info.GetTime()) % 7 == 0;
                    break;
                case 30:
                    moduleSolved = b == 0 && dispints[0] < 4 && dispints[1] < 4;
                    break;
                case 31:
                    moduleSolved = b == 0 && (dispints[0] / 2) % 2 == 0 && (dispints[1] / 2) % 2 == 0;
                    break;
                case 32:
                    moduleSolved = b == 1 && dispints[0] % 2 == 0 && dispints[1] == 0;
                    break;
                case 33:
                    moduleSolved = b == 1 && ((int)info.GetTime() / 60) % 2 == 1 && (int)info.GetTime() % 2 == 1;
                    break;
                case 34:
                    moduleSolved = b == 0 && dispints[0] == dispints[1];
                    break;
                case 35:
                    moduleSolved = b == 1 && dispints[0] == 7;
                    break;
                default:
                    if (startvals.Contains(f))
                        moduleSolved = b == 0;
                    else if (info.GetSerialNumberNumbers().Sum() < f)
                        moduleSolved = b == 1;
                    else if ((starttime / 60) % 10 == f)
                        moduleSolved = b == 0;
                    else if (info.GetPortCount() > f)
                        moduleSolved = b == 1;
                    else if (info.GetModuleNames().Count() % 10 == f)
                        moduleSolved = b == 0;
                    else if (info.GetBatteryCount() > f)
                        moduleSolved = b == 1;
                    else if (info.GetSolvedModuleNames().Count() % 2 == f % 2)
                        moduleSolved = b == 0;
                    else
                        moduleSolved = b == 1;
                    moduleSolved &= (int)info.GetTime() % 10 == f;
                    break;
            }
            if (moduleSolved)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                module.HandlePass();
                StopAllCoroutines();
            }
            else
                module.HandleStrike();
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <yes/no> <req> (req2)... [Presses the yes or no button when the specified requirement is met (optionally include multiple requirements and press when all are met)] | Format all requirements as <type>:<condition> or <type>:<condition>;<condition2> to wait for either condition. The type must be time (in seconds), minutes, seconds, colour, or word. The possible conditions for each type are as follows: time/seconds -> a number, minutes/seconds -> ""even"" or ""odd"", colour/word -> a colour name";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify a button to press and at least 1 requirement for when to press it!";
            }
            else if (parameters.Length == 2)
            {
                if (parameters[1].ToLower().EqualsAny("yes", "no"))
                    yield return "sendtochaterror Please specify at least 1 requirement for when to press the " + parameters[1].ToLower() + " button!";
                else
                    yield return "sendtochaterror!f The specified button to press '" + parameters[1] + "' is invalid!";
            }
            else if (parameters.Length >= 3)
            {
                if (!parameters[1].ToLower().EqualsAny("yes", "no"))
                {
                    yield return "sendtochaterror!f The specified button to press '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                List<string> reqTypes = new List<string>();
                List<List<string>> reqConditions = new List<List<string>>();
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i].ToLower().StartsWith("time:"))
                    {
                        if (parameters[i].Length == 5)
                        {
                            yield return "sendtochaterror Please specify at least 1 condition for the time requirement type!";
                            yield break;
                        }
                        List<string> condList = new List<string>();
                        string[] conditions = parameters[i].Split(':')[1].Split(';');
                        for (int j = 0; j < conditions.Length; j++)
                        {
                            int temp = -1;
                            if (!int.TryParse(conditions[j], out temp))
                            {
                                yield return "sendtochaterror!f The specified time requirement condition '" + conditions[j] + "' is not a number!";
                                yield break;
                            }
                            if (temp < 0)
                            {
                                yield return "sendtochaterror The specified time requirement condition '" + conditions[j] + "' cannot be negative!";
                                yield break;
                            }
                            condList.Add(temp.ToString());
                        }
                        reqTypes.Add("time");
                        reqConditions.Add(condList);
                    }
                    else if (parameters[i].ToLower().StartsWith("minutes:"))
                    {
                        if (parameters[i].Length == 8)
                        {
                            yield return "sendtochaterror Please specify at least 1 condition for the minutes requirement type!";
                            yield break;
                        }
                        List<string> condList = new List<string>();
                        string[] conditions = parameters[i].Split(':')[1].Split(';');
                        for (int j = 0; j < conditions.Length; j++)
                        {
                            if (!conditions[j].ToLower().EqualsAny("even", "odd"))
                            {
                                yield return "sendtochaterror!f The specified minutes requirement condition '" + conditions[j] + "' is not even or odd!";
                                yield break;
                            }
                            condList.Add(conditions[j].ToLower());
                        }
                        reqTypes.Add("minutes");
                        reqConditions.Add(condList);
                    }
                    else if (parameters[i].ToLower().StartsWith("seconds:"))
                    {
                        if (parameters[i].Length == 8)
                        {
                            yield return "sendtochaterror Please specify at least 1 condition for the seconds requirement type!";
                            yield break;
                        }
                        List<string> condList = new List<string>();
                        string[] conditions = parameters[i].Split(':')[1].Split(';');
                        for (int j = 0; j < conditions.Length; j++)
                        {
                            int temp = -1;
                            if (!int.TryParse(conditions[j], out temp))
                            {
                                if (conditions[j].ToLower().EqualsAny("even", "odd"))
                                {
                                    condList.Add(conditions[j].ToLower());
                                    continue;
                                }
                                yield return "sendtochaterror!f The specified seconds requirement condition '" + conditions[j] + "' is not \"even\", \"odd\", or a number!";
                                yield break;
                            }
                            if (temp < 0 || temp > 59)
                            {
                                yield return "sendtochaterror The specified seconds requirement condition '" + conditions[j] + "' is not in range 0-59!";
                                yield break;
                            }
                            condList.Add(temp.ToString());
                        }
                        reqTypes.Add("seconds");
                        reqConditions.Add(condList);
                    }
                    else if (parameters[i].ToLower().StartsWith("colour:"))
                    {
                        if (parameters[i].Length == 7)
                        {
                            yield return "sendtochaterror Please specify at least 1 condition for the colour requirement type!";
                            yield break;
                        }
                        List<string> condList = new List<string>();
                        string[] conditions = parameters[i].Split(':')[1].Split(';');
                        for (int j = 0; j < conditions.Length; j++)
                        {
                            if (!disptext.Contains(conditions[j].ToUpper()))
                            {
                                yield return "sendtochaterror!f The specified colour requirement condition '" + conditions[j] + "' is not a colour name!";
                                yield break;
                            }
                            condList.Add(conditions[j].ToUpper());
                        }
                        reqTypes.Add("colour");
                        reqConditions.Add(condList);
                    }
                    else if (parameters[i].ToLower().StartsWith("word:"))
                    {
                        if (parameters[i].Length == 5)
                        {
                            yield return "sendtochaterror Please specify at least 1 condition for the word requirement type!";
                            yield break;
                        }
                        List<string> condList = new List<string>();
                        string[] conditions = parameters[i].Split(':')[1].Split(';');
                        for (int j = 0; j < conditions.Length; j++)
                        {
                            if (!disptext.Contains(conditions[j].ToUpper()))
                            {
                                yield return "sendtochaterror!f The specified word requirement condition '" + conditions[j] + "' is not a colour name!";
                                yield break;
                            }
                            condList.Add(conditions[j].ToUpper());
                        }
                        reqTypes.Add("word");
                        reqConditions.Add(condList);
                    }
                }
                while (true)
                {
                    List<bool> reqMet = new List<bool>();
                    for (int i = 0; i < reqTypes.Count; i++)
                    {
                        bool found = false;
                        if (reqTypes[i].Equals("time"))
                        {
                            for (int j = 0; j < reqConditions[i].Count; j++)
                            {
                                if ((int)info.GetTime() == int.Parse(reqConditions[i][j]))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        else if (reqTypes[i].Equals("minutes"))
                        {
                            for (int j = 0; j < reqConditions[i].Count; j++)
                            {
                                if ((int)info.GetTime() / 60 % 2 == 0 && reqConditions[i][j].Equals("even"))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                                else if ((int)info.GetTime() / 60 % 2 == 1 && reqConditions[i][j].Equals("odd"))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        else if (reqTypes[i].Equals("seconds"))
                        {
                            for (int j = 0; j < reqConditions[i].Count; j++)
                            {
                                if ((int)info.GetTime() % 60 % 2 == 0 && reqConditions[i][j].Equals("even"))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                                else if ((int)info.GetTime() % 60 % 2 == 1 && reqConditions[i][j].Equals("odd"))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                                else if (!reqConditions[i][j].EqualsAny("even", "odd"))
                                {
                                    if ((int)info.GetTime() % 60 == int.Parse(reqConditions[i][j]))
                                    {
                                        reqMet.Add(true);
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (reqTypes[i].Equals("minutes"))
                        {
                            for (int j = 0; j < reqConditions[i].Count; j++)
                            {
                                if ((int)info.GetTime() / 60 % 2 == 0 && reqConditions[i][j].Equals("even"))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                                else if ((int)info.GetTime() / 60 % 2 == 1 && reqConditions[i][j].Equals("odd"))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        else if (reqTypes[i].Equals("colour"))
                        {
                            for (int j = 0; j < reqConditions[i].Count; j++)
                            {
                                if (disptext[dispints[1]].Equals(reqConditions[i][j]))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        else if (reqTypes[i].Equals("word"))
                        {
                            for (int j = 0; j < reqConditions[i].Count; j++)
                            {
                                if (disptext[dispints[0]].Equals(reqConditions[i][j]))
                                {
                                    reqMet.Add(true);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (!found)
                            reqMet.Add(false);
                    }
                    if (!reqMet.Contains(false))
                        break;
                    yield return "trycancel Halted waiting to press the " + parameters[1].ToLower() + " button due to a request to cancel.";
                }
                if (parameters[1].ToLower().Equals("yes"))
                    buttons[0].OnInteract();
                else
                    buttons[1].OnInteract();
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        bool valid = false;
        int b = -1;
        while (valid == false)
        {
            switch (finalvals[3])
            {
                case 10:
                    valid = dispints[0] == 7;
                    if (b == -1)
                        b = 0;
                    break;
                case 11:
                    valid = dispints[0] % 2 == 1 && dispints[1] % 2 == 1;
                    if (b == -1)
                        b = 0;
                    break;
                case 12:
                    valid = (int)info.GetTime() % 7 == 0;
                    if (b == -1)
                        b = 1;
                    break;
                case 13:
                    valid = dispints[1] == 0;
                    if (b == -1)
                        b = 1;
                    break;
                case 14:
                    valid = new int[] { 1, 2, 4 }.Contains(dispints[0]) && new int[] { 3, 5, 6 }.Contains(dispints[1]);
                    if (b == -1)
                        b = 0;
                    break;
                case 15:
                    valid = dispints[1] == 7;
                    if (b == -1)
                        b = 1;
                    break;
                case 16:
                    valid = (dispints[0] / 2) % 2 == 1 && (dispints[1] / 2) % 2 == 1;
                    if (b == -1)
                        b = 0;
                    break;
                case 17:
                    valid = new int[] { 3, 5, 6 }.Contains(dispints[0]) && new int[] { 3, 5, 6 }.Contains(dispints[1]);
                    if (b == -1)
                        b = 1;
                    break;
                case 18:
                    valid = new int[] { 1, 2, 4 }.Contains(dispints[0]) && new int[] { 1, 2, 4 }.Contains(dispints[1]);
                    if (b == -1)
                        b = 1;
                    break;
                case 19:
                    valid = ((int)info.GetTime() / 60) % 2 == 0 && (int)info.GetTime() % 2 == 0;
                    if (b == -1)
                        b = 0;
                    break;
                case 20:
                    valid = dispints[0] == 0;
                    if (b == -1)
                        b = 0;
                    break;
                case 21:
                    valid = new int[] { 3, 5, 6 }.Contains(dispints[0]) && new int[] { 1, 2, 4 }.Contains(dispints[1]);
                    if (b == -1)
                        b = 1;
                    break;
                case 22:
                    valid = ((int)info.GetTime() % 60) < 10;
                    if (b == -1)
                        b = 0;
                    break;
                case 23:
                    valid = dispints[0] == dispints[1];
                    if (b == -1)
                        b = 1;
                    break;
                case 24:
                    valid = dispints[0] == 0;
                    if (b == -1)
                        b = 1;
                    break;
                case 25:
                    valid = new int[] { 1, 2, 4 }.Contains(dispints[0]) && new int[] { 1, 2, 4 }.Contains(dispints[1]);
                    if (b == -1)
                        b = 0;
                    break;
                case 26:
                    valid = ((int)info.GetTime() / 60) % 2 == 0 && (int)info.GetTime() % 2 == 1;
                    if (b == -1)
                        b = 1;
                    break;
                case 27:
                    valid = dispints[0] > 3 && dispints[1] > 3;
                    if (b == -1)
                        b = 0;
                    break;
                case 28:
                    valid = new int[] { 3, 5, 6 }.Contains(dispints[0]) && new int[] { 3, 5, 6 }.Contains(dispints[1]);
                    if (b == -1)
                        b = 0;
                    break;
                case 29:
                    valid = Mathf.Abs(starttime - (int)info.GetTime()) % 7 == 0;
                    if (b == -1)
                        b = 1;
                    break;
                case 30:
                    valid = dispints[0] < 4 && dispints[1] < 4;
                    if (b == -1)
                        b = 0;
                    break;
                case 31:
                    valid = (dispints[0] / 2) % 2 == 0 && (dispints[1] / 2) % 2 == 0;
                    if (b == -1)
                        b = 0;
                    break;
                case 32:
                    valid = dispints[0] % 2 == 0 && dispints[1] == 0;
                    if (b == -1)
                        b = 1;
                    break;
                case 33:
                    valid = ((int)info.GetTime() / 60) % 2 == 1 && (int)info.GetTime() % 2 == 1;
                    if (b == -1)
                        b = 1;
                    break;
                case 34:
                    valid = dispints[0] == dispints[1];
                    if (b == -1)
                        b = 0;
                    break;
                case 35:
                    valid = dispints[0] == 7;
                    if (b == -1)
                        b = 1;
                    break;
                default:
                    if (b == -1)
                    {
                        if (startvals.Contains(finalvals[3]))
                            b = 0;
                        else if (info.GetSerialNumberNumbers().Sum() < finalvals[3])
                            b = 1;
                        else if ((starttime / 60) % 10 == finalvals[3])
                            b = 0;
                        else if (info.GetPortCount() > finalvals[3])
                            b = 1;
                        else if (info.GetModuleNames().Count() % 10 == finalvals[3])
                            b = 0;
                        else if (info.GetBatteryCount() > finalvals[3])
                            b = 1;
                        else if (info.GetSolvedModuleNames().Count() % 2 == finalvals[3] % 2)
                            b = 0;
                        else
                            b = 1;
                    }
                    valid = (int)info.GetTime() % 10 == finalvals[3];
                    break;
            }
            yield return true;
        }
        buttons[b].OnInteract();
    }
}
