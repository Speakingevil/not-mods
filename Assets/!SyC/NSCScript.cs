using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class NSCScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public TextMesh[] displays;
    public Renderer[] leds;
    public Material[] cols;

    private readonly string opkey = "fibcgmhdlnfbdnhciklcemafdfjjglkcaamhjlmfdigkaceinackneehgbad";
    private readonly int[] primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97};
    private readonly string symbtext = "ABCDEFGHLMNOPQXZghlp";
    private int[] binorder = new int[4] { 0, 1, 2, 3 };
    private bool[] neg = new bool[3];
    private List<int>[] functionlists = new List<int>[2] { new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19 }, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19 } };
    private List<int>[] functions = new List<int>[] { new List<int> { }, new List<int> { } };
    private int[] symbols = new int[3];
    private int[] symnums = new int[7];
    private bool[,] outputs = new bool[6, 2];
    private int[] ans = new int[2];
    private int[] selected = new int[2];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        functionlists[0].Shuffle();
        for (int i = 0; i < 3; i++)
        {
            symbols[i] = functionlists[0][i];
            leds[i].material = cols[0];
            neg[i] = Random.Range(0, 2) == 0;
            leds[i + 3].material = cols[neg[i] ? 1 : 0];
        }
        displays[0].text = symbols.Select(x => symbtext[x].ToString()).Join();
        for(int i = 0; i < 2; i++)
        {
            functionlists[i].Shuffle();
            functions[i].Add(functionlists[i][0]);
            functionlists[i].RemoveAt(0);
            ans[i] = functions[i][0];
            displays[i + 1].text = "";
        }
        module.OnActivate = Activate;
    }

    private int N(int x, int i)
    {
        switch (x)
        {
            case 0:
                switch (i)
                {
                    case 0: return symbols[1] + 1;
                    case 1: return DateTime.Now.Hour * 2;
                    default: return info.GetPortPlateCount() * 6;                
                }
            case 1:
                switch (i)
                {
                    case 0: return info.GetSerialNumberLetters().Select(k => k - 'A' + 1).Min();
                    case 1: return string.Join("", info.GetIndicators().ToArray()).Count(k => "AEIOU".Contains(k)) * 5;
                    default: return primes.Count(k => k < info.GetSerialNumberNumbers().Sum()) * 6;
                }
            case 2:
                switch (i)
                {
                    case 0: return info.GetSerialNumberNumbers().Max() - info.GetSerialNumberNumbers().Min();
                    case 1: return info.GetOffIndicators().Count() * info.GetOnIndicators().Count();
                    default: return (int)DateTime.Today.DayOfWeek * 4;
                }
            case 3:
                switch (i)
                {
                    case 0: return info.GetIndicators().Count();
                    case 1: return info.GetModuleNames().Count().ToString().Select(k => k - '0').Sum();
                    default: return info.CountUniquePorts() * 5;
                }
            case 4:
                switch (i)
                {
                    case 0: return info.GetBatteryCount();
                    case 1: return string.Join("", info.GetIndicators().ToArray()).Count(k => k - 'A' > 12);
                    default: return info.GetPortCount() + info.GetSerialNumberNumbers().Last();
                }
            case 5:
                switch (i)
                {
                    case 0: return info.GetOnIndicators().Count() * 7;
                    case 1: return info.GetSerialNumberNumbers().Min() + info.GetPortPlateCount();
                    default: return (info.GetBatteryHolderCount() * 2 - info.GetBatteryCount()) * 6;
                }
            case 6:
                switch (i)
                {
                    case 0: return DateTime.Now.Date.Day;
                    case 1: return info.GetModuleNames().Select(k => k[0] - 'A' + 1).Max();
                    default: return info.GetSerialNumberLetters().ToArray()[1] - 'A' + 1;
                }
            case 7:
                switch (i)
                {
                    case 0: return info.GetSerialNumberNumbers().Min() + 10;
                    case 1: return info.GetSerialNumberLetters().Select(k => k - 'A' + 1).Min() * 2;
                    default: return string.Join("", info.GetIndicators().ToArray()).GroupBy(k => k).Count();
                }
            case 8:
                switch (i)
                {
                    case 0: return info.GetPortCount();
                    case 1: return string.Join("", info.GetIndicators().ToArray()).Count(k => k - 'A' < 13);
                    default: return info.GetOffIndicators().Count() + info.GetBatteryHolderCount();
                }
            case 9:
                switch (i)
                {
                    case 0: return info.GetOffIndicators().Count() * 9;
                    case 1: return info.GetSerialNumberLetters().Select(k => k - 'A' + 1).Sum() / info.GetSerialNumberLetters().Count();
                    default: return 15;
                }
            case 10:
                switch (i)
                {
                    case 0: return info.GetSerialNumberLetters().Count(k => !"AEIOU".Contains(k.ToString())) * 7;
                    case 1: return symbols[0] + symbols[2] + 2;
                    default: return primes.Where(k => k <= info.GetSerialNumberNumbers().Max()).Sum();
                }
            case 11:
                switch (i)
                {
                    case 0: return info.GetBatteryHolderCount() * 3;
                    case 1: return info.GetModuleNames().Count() / 5;
                    default: return DateTime.Now.Hour + DateTime.Today.Month;
                }
            case 12:
                switch (i)
                {
                    case 0: return info.GetSerialNumberNumbers().Sum();
                    case 1: return info.GetSerialNumberLetters().First() - 'A' + 1;
                    default: return info.GetIndicators().Count() == 0 ? 0 : string.Join("", info.GetIndicators().ToArray()).Where(k => !" AEIOU".Contains(k)).Select(k => k - 'A' + 1).Min();
                }
            case 13:
                switch (i)
                {
                    case 0: return info.GetSerialNumberLetters().Select(k => k - 'A' + 1).Max();
                    case 1: return info.GetSerialNumberNumbers().Where(k => k != 0).Aggregate(1, (a, b) => a * b).ToString().Select(k => k - '0').Sum();
                    default: return info.GetOnIndicators().Count() + info.GetPortPlateCount();
                }
            case 14:
                switch (i)
                {
                    case 0: return info.GetModuleNames().Count() % 25;
                    case 1: return info.GetIndicators().Count() == 0 ? 0 : string.Join("", info.GetIndicators().ToArray()).Select(k => k - 'A' + 1).Max();
                    default: return (info.GetBatteryCount() - info.GetBatteryHolderCount()) * 5;
                }
            case 15:
                switch (i)
                {
                    case 0: return info.GetSerialNumberLetters().Count(k => "AEIOU".Contains(k.ToString())) * 9;
                    case 1: return DateTime.Today.Month * 3;
                    default: return 21;
                }
            case 16:
                switch (i)
                {
                    case 0: return info.GetBatteryCount() + info.GetIndicators().Count(k => k.All(q => !"AEIOU".Contains(q.ToString())));
                    case 1: return info.GetPortCount() * 3;
                    default: return info.GetSerialNumberNumbers().Last();
                }
            case 17:
                switch (i)
                {
                    case 0: return info.GetOffIndicators().Count() + info.GetPortPlateCount();
                    case 1: return info.GetSerialNumberNumbers().Sum() / info.GetSerialNumberNumbers().Count();
                    default: return string.Join("", info.GetIndicators().ToArray()).Count(k => !"AEIOU".Contains(k));
                }
            case 18:
                switch (i)
                {
                    case 0: return info.GetSerialNumberNumbers().Max();
                    case 1: return info.GetOnIndicators().Count() == 0 ? 0 :string.Join("", info.GetOnIndicators().ToArray()).Select(k => k - 'A' + 1).Max();
                    default: return info.GetPortPlateCount() + info.GetBatteryHolderCount();
                }
            default:
                switch (i)
                {
                    case 0: return info.GetOnIndicators().Count() + info.GetBatteryHolderCount();
                    case 1: return info.GetSerialNumberNumbers().First();
                    default: return 20 - symbols[1];
                }
        }
    }

    private bool T(int x, int i)
    {
        switch (x)
        {
            case 0:
                switch (i)
                {
                    case 0: return symnums[6] % 2 == 0;
                    case 1: return symnums[0] > symnums[2];
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k]).Distinct().Count() < 3;
                }
            case 1:
                switch (i)
                {
                    case 0: return symnums[6] % 7 == 0 || symnums[6] % 13 == 0;
                    case 1: return string.Join("", Enumerable.Range(3, 3).Select(k => symnums[k].ToString()).ToArray()).All(k => k != '8' && k != '9');
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k]).All(k => k % 2 == 0) || Enumerable.Range(0, 3).Select(k => symnums[k]).All(k => k % 2 == 1);
                }
            case 2:
                switch (i)
                {
                    case 0: return Enumerable.Range(3, 3).Select(k => symnums[k]).Any(k => primes.Contains(k));
                    case 1: return symnums[6] % 3 == 1;
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k]).Count(k => k % 2 == 0) == 1;
                }
            case 3:
                switch (i)
                {
                    case 0: return Enumerable.Range(0, 3).Select(k => symnums[k]).Count(k => k >= 20) > 1;
                    case 1: return new char[] {'1', '2'}.All(q => string.Join("", Enumerable.Range(3, 3).Select(k => symnums[k].ToString()).ToArray()).Contains(q));
                    default: return primes.Contains(symnums[6]);
                }
            case 4:
                switch (i)
                {
                    case 0: return Enumerable.Range(3, 3).Select(k => symnums[k]).Any(k => k <= 10);
                    case 1: return Enumerable.Range(0, 3).Select(k => symnums[k]).Any(k => k < 2);
                    default: return symnums[6] % 2 == 1 && (symnums[6] / 10) % 2 == 1;
                }
            case 5:
                switch (i)
                {
                    case 0: return new int[] { 2, 4 }.Contains(symnums[6] % 5);
                    case 1: return Enumerable.Range(0, 3).Select(k => symnums[k] % 3).Distinct().Count() > 2;
                    default: return symnums[6] >= 50;
                }
            case 6:
                switch (i)
                {
                    case 0: return Enumerable.Range(9, 14).All(k => k != (symnums[6] / 10) + (symnums[6] % 10));
                    case 1: return Enumerable.Range(3, 3).Select(k => symnums[k]).Any(k => k % 9 == 0);
                    default: return symnums[5] < (symnums[0] * 2) % 100;
                }
            case 7:
                switch (i)
                {
                    case 0: return symnums[1] > symnums[0];
                    case 1: return Mathf.Abs(symnums[6] / 10 - symnums[6] % 10) < 3;
                    default: return Enumerable.Range(3, 3).Select(k => symnums[k]).Any(k => primes.Contains(k));
                }
            case 8:
                switch (i)
                {
                    case 0: return new int[] { 1, 3, 6 }.Contains(symnums[6] % 7);
                    case 1: int[] p = Enumerable.Range(0, 3).Select(k => symnums[k]).ToArray(); return p.Max() - p.Min() < 10;
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k]).All(k => k > 1 && !primes.Contains(k));
                }
            case 9:
                switch (i)
                {
                    case 0: return Enumerable.Range(0, 3).Select(k => symnums[k]).Count(k => (((k - 1) % 9) + 1) % 2 == 1) > 1;
                    case 1: return Enumerable.Range(0, 3).Select(k => symnums[k]).All(k => k % 3 > 0);
                    default: return new int[] { 3, 6 }.Any(k => k == symnums[6] / 10 || k == symnums[6] % 10);
                }
            case 10:
                switch (i)
                {
                    case 0: return (((symnums[6] - 1) % 9) - 1) % 2 == 1;
                    case 1: return Enumerable.Range(0, 3).Select(k => symnums[k]).Any(k => k >= 30);
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k]).Where(k => k > 0).Any(k => symnums[6] % k == 0);
                }
            case 11:
                switch (i)
                {
                    case 0: return symnums[6] % 4 == 1;
                    case 1: return Enumerable.Range(3, 3).Select(k => symnums[k]).All(k => k % 4 != 0);
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k] % 7).Distinct().Count() > 2;
                }
            case 12:
                switch (i)
                {
                    case 0: return Enumerable.Range(0, 3).Select(k => symnums[k]).Count(k => k % 2 == 1) == 1;
                    case 1: return symnums[4] < (2 * symnums[1]) % 100;
                    default: return symnums[6] % 2 != (symnums[6] / 10) % 2;
                }
            case 13:
                switch (i)
                {
                    case 0: return symnums[2] > symnums[1];
                    case 1: return symnums[6] % 2 == 1;
                    default: return string.Join("", Enumerable.Range(3, 3).Select(k => symnums[k].ToString()).ToArray()).All(k => k != '1');
                }
            case 14:
                switch (i)
                {
                    case 0: return primes.Count(k => k >= symnums.Min() && k <= symnums.Where((p, q) => q < 3).Max()) == 4;
                    case 1: return Enumerable.Range(7, 11).All(k => k != (symnums[6] / 10) + (symnums[6] % 10));
                    default: return Enumerable.Range(0, 3).Select(k => symnums[k]).Count(k => k % 2 == 0) > 1;
                }
            case 15:
                switch (i)
                {
                    case 0: return string.Join("", Enumerable.Range(0, 3).Select(k => symnums[k].ToString()).ToArray()).GroupBy(k => k).All(k => k.Count() == 1);
                    case 1: return Enumerable.Range(3, 3).Select(k => symnums[k]).Any(k => k.ToString().Select(q => q - '0').Sum() % 5 == 0);
                    default: return symnums[6] % 6 == 0;
                }
            case 16:
                switch (i)
                {
                    case 0: return Enumerable.Range(0, 3).Select(k => symnums[k]).All(k => k <= 20);
                    case 1: return symnums[6] > 1 && !primes.Contains(symnums[6]);
                    default: return symnums[0] > symnums[1];
                }
            case 17:
                switch (i)
                {
                    case 0: int[] q = Enumerable.Range(3, 3).Select(k => symnums[k]).ToArray(); return primes.Count(k => k >= q.Min() && k <= q.Max()) < 2;
                    case 1: return Enumerable.Range(0, 3).Select(k => symnums[k]).Count(k => k % 3 == 0) == 1;
                    default: return symnums[6] % 2 == (symnums[6] / 10) % 2;
                }
            case 18:
                switch (i)
                {
                    case 0: return symnums[3] > (symnums[2] * 2) % 100;
                    case 1: int[] p = Enumerable.Range(0, 3).Select(k => symnums[k]).ToArray(); return p.Max() - p.Min() >= 25;
                    default: return new int[] { 2, 9 }.Any(k => k == symnums[6] / 10 || k == symnums[6] % 10);
                }
            default:
                switch (i)
                {
                    case 0: int[] p = Enumerable.Range(0, 3).Select(k => symnums[k]).ToArray(); return p.Max() - p.Min() <= 5;
                    case 1: return Mathf.Abs(symnums[6] / 10 - symnums[6]) > 4;
                    default: return string.Join("", Enumerable.Range(0, 3).Select(k => symnums[k].ToString()).ToArray()).GroupBy(k => k).Any(k => k.Count() > 2);
                }
        }
    }

    private bool L(bool a, bool b, char c)
    {
        switch (c)
        {
            case 'a': return a && b;
            case 'b': return a || b;
            case 'c': return a ^ b;
            case 'd': return !(a && b);
            case 'e': return !(a || b);
            case 'f': return a == b;
            case 'g': return !a || b;
            case 'h': return a || !b;
            case 'i': return a && !b;
            case 'j': return !a && b;
            case 'k': return a;
            case 'l': return b;
            case 'm': return !a;
            default: return !b;
        }
    }

    private void Activate()
    {
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] The displayed symbols are the {1}, {2}, and {3} symbols in the manual, from the top.", moduleID, (symbols[0] + 1).ToString() + (symbols[0] > 2 ? "th" : new string[] { "st", "nd", "rd"}[symbols[0]]), (symbols[1] + 1).ToString() + (symbols[1] > 2 ? "th" : new string[] { "st", "nd", "rd" }[symbols[1]]), (symbols[2] + 1).ToString() + (symbols[2] > 2 ? "th" : new string[] { "st", "nd", "rd" }[symbols[2]]));
        for (int i = 0; i < 3; i++)
        {
            symnums[i] = N(symbols[i], i) % 100;
            if (symnums[i] < 0)
                symnums[i] = 0;
        }
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] The numeric values of the symbols are: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 3).Select(k => symnums[k].ToString()).ToArray()));
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] {1} of the truth values are negated.", moduleID, neg[1] ? (neg[0] ? "Both" : "The second") : (neg[0] ? "The first" : "Neither"));
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
                if (i != 2 - j)
                {
                    symnums[i + 3] += symnums[j];
                    symnums[i + 3] %= 100;
                }
            symnums[6] += symnums[i];
        }
        symnums[6] %= 100;
        for (int i = 0; i < 3; i++)
        {
            int k = 0;
            for(int j = 0; j < 3; j++)
                if(i != j)
                {
                    outputs[i, k] = T(symbols[i], j) ^ neg[k];
                    k = 1;
                }
        }
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] The truth values of the symbols, after negations are applied, are: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 3).Select(k => (outputs[k, 0] ? "T" : "F") + "|" + (outputs[k, 1] ? "T" : "F")).ToArray()));
        string[] logops = new string[6];
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                outputs[3 + (i * 2), j] = L(outputs[i, j], outputs[i + 1, j], opkey[(ans[i] * 3) + i + j]);
                logops[(4 * i) + j] = string.Format("[Not Symbolic Coordinates #{0}] {1} {4}{5} {2} = {3}", moduleID, outputs[i, j] ? "T" : "F", outputs[i + 1, j] ? "T" : "F", outputs[3 + (i * 2), j] ? "T" : "F", new string[] { "\u03b1", "\u03b2", "\u03b3"}[i + j], "LR"[i]);
            }
        if (info.IsIndicatorPresent(Indicator.CLR))
        {
            if (neg[2])
            {               
                outputs[4, 0] = L(outputs[0, 0], outputs[2, 0], opkey[(ans[0] * 3) + 2]);
                outputs[4, 1] = L(outputs[0, 1], outputs[2, 1], opkey[ans[1] * 3]);
            }
            else
            {
                outputs[4, 0] = L(outputs[0, 1], outputs[2, 1], opkey[ans[1] * 3]);
                outputs[4, 1] = L(outputs[0, 0], outputs[2, 0], opkey[(ans[0] * 3) + 2]);
            }
        }
        else
        {
            if (neg[2])
            {
                outputs[4, 0] = L(outputs[0, 1], outputs[2, 1], opkey[(ans[0] * 3) + 2]);
                outputs[4, 1] = L(outputs[0, 0], outputs[2, 0], opkey[ans[1] * 3]);
            }
            else
            {
                outputs[4, 0] = L(outputs[0, 0], outputs[2, 0], opkey[ans[1] * 3]);
                outputs[4, 1] = L(outputs[0, 1], outputs[2, 1], opkey[(ans[0] * 3) + 2]);
            }
        }
        logops[neg[2] ? 2 : 3] = string.Format("[Not Symbolic Coordinates #{0}] {1} {3} {2} = {4}", moduleID, (neg[2] ^ info.IsIndicatorPresent(Indicator.CLR) ? outputs[0, 1] : outputs[0, 0]) ? "T" : "F", (neg[2] ^ info.IsIndicatorPresent(Indicator.CLR) ? outputs[2, 1] : outputs[2, 0]) ? "T" : "F", neg[2] ? "\u03b3" + "L" : "\u03b1" + "R", outputs[4, 0] ? "T" : "F");
        logops[neg[2] ? 3 : 2] = string.Format("[Not Symbolic Coordinates #{0}] {1} {3} {2} = {4}", moduleID, (neg[2] ^ info.IsIndicatorPresent(Indicator.CLR) ? outputs[0, 0] : outputs[0, 1]) ? "T" : "F", (neg[2] ^ info.IsIndicatorPresent(Indicator.CLR) ? outputs[2, 0] : outputs[2, 1]) ? "T" : "F", neg[2] ? "\u03b1" + "R" : "\u03b3" + "L", outputs[4, 1] ? "T" : "F");
        int batt = info.GetBatteryCount();
        for(int i = 0; i < 4; i++)
        {
            binorder[i] += batt * 3;
            binorder[i] %= 4;
        }
        if (info.IsIndicatorPresent(Indicator.TRN))
            binorder = binorder.Reverse().ToArray();
        int[] ledcols = new int[3];
        for (int i = 0; i < 3; i++)
        {
            ledcols[i] = outputs[i + 3, 0] ? (outputs[i + 3, 1] ? binorder[3] : binorder[2]) : (outputs[i + 3, 1] ? binorder[1] : binorder[0]);
            leds[i].material = cols[ledcols[i] + 2];
        }
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] The colours of the LEDs are: {1}", moduleID, string.Join(", ", ledcols.Select(k => new string[] { "Green", "Yellow", "Aqua", "Purple"}[k]).ToArray()));
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] The target outputs of the operator sets are: {1}", moduleID, string.Join(", ", Enumerable.Range(3, 3).Select(k => (outputs[k, 0] ? "T" : "F") + "|" + (outputs[k, 1] ? "T" : "F")).ToArray()));
        Debug.LogFormat(string.Join("\n", logops));
        for (int i = 0; i < 2; i++)
        {
            int query = 0;
            bool[] check = Enumerable.Range(3, 3).Select(k => outputs[k, i]).ToArray();
            while(query < functionlists[i].Count())
            {
                bool[] pass = new bool[2] { false, false };
                for (int k = 0; k < 2; k++)
                {
                    for (int j = 0; j < 2; j++)
                        if (check[k * 2] != L(outputs[k, j], outputs[k + 1, j], opkey[(functionlists[i][query] * 3) + k + j]))
                        {
                            if (pass[0])
                            {
                                if (pass[1])
                                {
                                    query++;
                                    goto next;
                                }
                                else
                                    pass[1] = true;
                            }
                            else
                                pass[0] = true;
                        }
                }
                if (info.IsIndicatorPresent(Indicator.CLR))
                {
                    if (neg[2])
                    {
                        if(pass[1] && check[1] != L(outputs[0, 0], outputs[2, 0], opkey[(functionlists[i][query] * 3) + 2]) && check[1] != L(outputs[0, 1], outputs[2, 1], opkey[functionlists[i][query] * 3]))
                        {
                            query++;
                            goto next;
                        }
                    }
                    else
                    {
                        if(pass[1] && check[1] != L(outputs[0, 1], outputs[2, 1], opkey[functionlists[i][query] * 3]) && check[1] != L(outputs[0, 0], outputs[2, 0], opkey[(functionlists[i][query] * 3) + 2]))
                        {
                            query++;
                            goto next;
                        }
                    }
                }
                else
                {
                    if (neg[2])
                    {
                        if (pass[1] && check[1] != L(outputs[0, 1], outputs[2, 1], opkey[(functionlists[i][query] * 3) + 2]) && check[1] != L(outputs[0, 0], outputs[2, 0], opkey[functionlists[i][query] * 3]))
                        {
                            query++;
                            goto next;
                        }
                    }
                    else
                    {
                        if (pass[1] && check[1] != L(outputs[0, 0], outputs[2, 0], opkey[functionlists[i][query] * 3]) && check[1] != L(outputs[0, 1], outputs[2, 1], opkey[(functionlists[i][query] * 3) + 2]))
                        {
                            query++;
                            goto next;
                        }
                    }
                }
                functionlists[i].RemoveAt(query);
                next:;
            }
            for (int j = 0; j < Mathf.Min(4, functionlists[i].Count()); j++)
                functions[i].Add(functionlists[i][j]);
            functions[i].Shuffle();
            Debug.LogFormat("[Not Symbolic Coordinates #{0}] The possible {1} sets of functions are: {2}", moduleID, i == 0 ? "left" : "right", string.Join(", ", functions[i].Select(k => "0123456789ABCDEFGHIJ"[k].ToString()).ToArray()));
        }
        Debug.LogFormat("[Not Symbolic Coordinates #{0}] The sets that yield the target outputs are {1} and {2}.", moduleID, "0123456789ABCDEFGHIJ"[ans[0]], "0123456789ABCDEFGHIJ"[ans[1]]);
        for(int i = 0; i < 2; i++)
            displays[i + 1].text = "0123456789ABCDEFGHIJ"[functions[i][0]].ToString();
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate () { Press(b); return false; };
        }
    }

    private void Press(int b)
    {
        if (!moduleSolved)
        {
            if (b == 4)
            {
                buttons[4].AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[4].transform);
                if (functions[0][selected[0]] == ans[0] && functions[1][selected[1]] == ans[1])
                {
                    Audio.PlaySoundAtTransform("BeepSolve", transform);
                    displays[0].text = "BRAVO";
                    for (int i = 1; i < 3; i++)
                        displays[i].text = "*";
                    foreach (Renderer r in leds)
                        r.material = cols[0];
                    moduleSolved = true;
                    module.HandlePass();
                }
                else
                    module.HandleStrike();
            }
            else
            {
                buttons[b].AddInteractionPunch(0.5f);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, buttons[b].transform);
                selected[b / 2] += b % 2 == 0 ? 1 : functions[b / 2].Count() - 1;
                selected[b / 2] %= functions[b / 2].Count();
                displays[(b / 2) + 1].text = "0123456789ABCDEFGHIJ"[functions[b / 2][selected[b / 2]]].ToString();
            }
        }
    }
}
