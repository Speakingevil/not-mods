using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;

public class NX01Script : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] buttons;
    public Renderer[] segs;
    public Material[] cols;
    public TextMesh[] labels;

    private readonly int[] primelist = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239 };
    private int stage = 1;
    private int[] nums = new int[10];
    private int[] seq = new int[12];
    private bool[][] answer = new bool[2][] { new bool[10], new bool[10]};
    private bool[] play = new bool[4];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        nums = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }.Shuffle().Where((x, i) => i < 10).ToArray();
        for (int i = 0; i < 10; i++)
            labels[i].text = nums[i].ToString();
        for (int i = 0; i < 12; i++)
            seq[i] = Random.Range(0, 10);
        Debug.LogFormat("[Not X01 #{0}] The values of the sectors in clockwise order are: {1}", moduleID, string.Join(", ", nums.Select(x => x.ToString()).ToArray()));
        foreach(KMSelectable button in buttons)
        {
            int b = Array.IndexOf(buttons, button) / 4;
            button.OnInteract = delegate () { Press(b); return false; };
        }
    }

    private void Ansgen()
    {
        List<string> logrules = new List<string> { };
        int[][] subseq = new int[2][];
        subseq[0] = seq.Where((x, i) => i < (stage + 1) * 3).ToArray();
        subseq[1] = subseq[0].Select(x => nums[x]).ToArray();
        Debug.LogFormat("[Not X01 #{0}] The flashing sequence for stage {1} is: {2}", moduleID, stage, string.Join(", ", subseq[1].Select(x => x.ToString()).ToArray()));
        int p = 0;
        int[] partialsums = subseq[1].Select(x => p += x).ToArray();
        Debug.LogFormat("[Not X01 #{0}] The partial sums of the sequence are: {1}", moduleID, string.Join(", ", partialsums.Select(x => x.ToString()).ToArray()));
        if(partialsums.Any(x => x % 25 == 0))
        {
            logrules.Add("1");
            int z = partialsums.TakeWhile(x => x % 25 > 0).Count();
            int[] f = subseq[0].Where((x, i) => i < z).ToArray();
            answer[0] = Enumerable.Range(0, 10).Select(x => f.Contains(x)).ToArray();
        }
        int[][] triplets = subseq[0].Where((x, i) => i < subseq[0].Count() - 2).Select((x, i) => new int[] { x, subseq[0][i + 1], subseq[0][i + 2]}).ToArray();
        if(triplets.Any(x => (x[1] - x[0] + 10) % 10 == 1 && (x[2] - x[1] + 10) % 10 == 1))
        {
            logrules.Add("2");
            bool[] odds = Enumerable.Range(0, 10).Select(x => nums[(x + 9) % 10] % 2 == 1).ToArray();
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ odds[x]).ToArray();
        }
        if (triplets.Any(x => (x[1] - x[0] + 10) % 10 == 9 && (x[2] - x[1] + 10) % 10 == 9))
        {
            logrules.Add("3");
            bool[] evens = Enumerable.Range(0, 10).Select(x => nums[(x + 1) % 10] % 2 == 0).ToArray();
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ evens[x]).ToArray();
        }
        if (subseq[0].Where((x, i) => i < subseq[0].Length - 1).Select((x, i) => Mathf.Abs(x - subseq[0][i + 1]) == 5).Any(x => x == true))
        {
            logrules.Add("4");
            bool[] primes = Enumerable.Range(0, 10).Select(x => primelist.Contains(nums[(x + 5) % 10])).ToArray();
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ primes[x]).ToArray();
        }
        if (triplets.Any(x => x.All(y => nums[y] % 2 == 0) && x.Distinct().Count() > 2))
        {
            logrules.Add("5");
            int[] top = new int[5] { 8, 9, 0, 1, 2 };
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ top.Contains(x)).ToArray();
        }
        if (subseq[1].Where((x, i) => i < subseq[1].Length - 1).Select((x, i) => Mathf.Abs(x - subseq[1][i + 1]) > 9).Count(x => x == true) > 1)
        {
            logrules.Add("6");
            bool[] teens = Enumerable.Range(0, 10).Select(x => nums[x] > 9).ToArray();
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ teens[x]).ToArray();
        }
        if (nums.Where(x => primelist.Contains(x)).All(x => subseq[1].Contains(x)))
        {
            logrules.Add("7");
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ (nums[x] % 2 == 0)).ToArray();
        }
        if (subseq[0].GroupBy(x => x).Any(x => x.Count() > 2))
        {
            logrules.Add("8");
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ (x % 2 == 1)).ToArray();
        }
        if (subseq[0].Where((x, i) => i < subseq[0].Length - 1).Select((x, i) => Mathf.Abs(x - subseq[0][i + 1]) % 10 > 1 && Mathf.Abs(x - subseq[0][i + 1]) % 10 < 8).All(x => x == true))
        {
            logrules.Add("9");
            int z = Array.IndexOf(nums, nums.Max());
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] ^ (Mathf.Abs(x - z) % 10 > 2  && Mathf.Abs(x - z) % 10 < 7)).ToArray();
        }
        if (logrules.Count() == 0)
            Debug.LogFormat("[Not X01 #{0}] No rules apply.", moduleID);
        else if (logrules.Count() == 1)
            Debug.LogFormat("[Not X01 #{0}] Rule {1} applies.", moduleID, logrules[0]);
        else
            Debug.LogFormat("[Not X01 #{0}] Rules {1} apply.", moduleID, string.Join(", ", logrules.ToArray()));
        int final = partialsums.Last();
        if(answer[0].Count(x => x == true) < 1)
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 1 applies.", moduleID);
            answer[0] = nums.Select(x => x != 1 && !primelist.Contains(x)).ToArray();
        }
        else if (partialsums.Where((x, i) => i > 0 && i < partialsums.Count() - 1).Any(x => final % x == 0))
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 2 applies.", moduleID);
            answer[0] = answer[0].Select(x => !x).ToArray();
        }
        else if (primelist.Contains(final))
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 3 applies.", moduleID);
            bool[] dflip = Enumerable.Range(0, 10).Select(x => answer[0][(x + 5) % 10]).ToArray();
            answer[0] = dflip;
        }
        else if (partialsums.Count(x => primelist.Contains(x)) > 2)
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 4 applies.", moduleID);
            bool[] clock = Enumerable.Range(0, 10).Select(x => answer[0][(x + 9) % 10]).ToArray();
            answer[0] = clock;
        }
        else if (partialsums.Count(x => primelist.Contains(x)) < 1)
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 5 applies.", moduleID);
            bool[] counter = Enumerable.Range(0, 10).Select(x => answer[0][(x + 1) % 10]).ToArray();
            answer[0] = counter;
        }
        else if (final % 7 == 0)
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 6 applies.", moduleID);
            bool[] vflip = Enumerable.Range(0, 10).Select(x => answer[0][(15 - x) % 10]).ToArray();
            answer[0] = vflip;
        }
        else if (partialsums.Count(x => x % 7 == 0) > 2)
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 7 applies.", moduleID);
            bool[] hflip = Enumerable.Range(0, 10).Select(x => answer[0][(10 - x) % 10]).ToArray();
            answer[0] = hflip;
        }
        else if (info.GetSerialNumberNumbers().Contains(((final - 1) % 9) + 1))
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 8 applies.", moduleID);
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] || info.GetSerialNumberNumbers().Contains(nums[x] % 10)).ToArray();
        }
        else if (answer[0].Count(x => x == true) > 5)
        {
            Debug.LogFormat("[Not X01 #{0}] Alteration rule 9 applies.", moduleID);
            answer[0] = Enumerable.Range(0, 10).Select(x => answer[0][x] && !info.GetSerialNumberNumbers().Contains(nums[x] % 10)).ToArray();
        }
        else
            Debug.LogFormat("[Not X01 #{0}] No alteration rules apply.", moduleID);
        if (answer[0].Count(x => x == true) < 1)
            Debug.LogFormat("[Not X01 #{0}] Do not submit any sectors.", moduleID);
        else
            Debug.LogFormat("[Not X01 #{0}] The sectors {1} must be submitted.", moduleID, string.Join(", ", Enumerable.Range(0, 10).Where(x => answer[0][x]).Select(x => nums[x].ToString()).ToArray()));
    }

    private void Press(int b)
    {
        if (!moduleSolved && !play[2])
        {
            if (play[0])
            {
                StopCoroutine("Sequence");
                if (play[1])
                {
                    play[1] = false;
                    for (int i = 0; i < 20; i++)
                        segs[i].material = cols[i % 2];
                }
                if (b > 9)
                {
                    play[2] = true;
                    StartCoroutine("Submit");
                    buttons[41].AddInteractionPunch();
                }
                else if(!answer[1][b])
                {
                    segs[b].material = cols[(b % 2) + 2];
                    segs[b + 10].material = cols[(b % 2) + 2];
                    Audio.PlaySoundAtTransform("Sound" + (12 - Mathf.Min(b, 10 - b)), buttons[b * 4].transform);
                    buttons[b * 4].AddInteractionPunch(0.6f);
                    answer[1][b] = true;
                }
            }
            else if (b > 9)
            {
                buttons[41].AddInteractionPunch();
                play[0] = true;
                if (!play[3])
                {
                    play[3] = true;
                    Ansgen();
                }
                play[1] = true;
                StartCoroutine("Sequence");
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                for (int i = 0; i < 10; i++)
                    labels[i].text = string.Empty;
            }
        }
    }

    private IEnumerator Sequence()
    {
        play[1] = true;
        int j = 0;
        while(!moduleSolved)
        {
            if (j == 0)
                yield return new WaitForSeconds(1);
            Audio.PlaySoundAtTransform("Sound" + (Mathf.Min(seq[j], 10 - seq[j]) + 1), buttons[seq[j] * 4].transform);
            segs[seq[j]].material = cols[(seq[j] % 2) + 2];
            segs[seq[j] + 10].material = cols[(seq[j] % 2) + 2];
            j = (j + 1) % ((stage + 1) * 3);
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < 20; i++)
                segs[i].material = cols[i % 2];
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Submit()
    {
        Audio.PlaySoundAtTransform("Submit", transform);
        for(int i = 0; i < (stage * 10) + 1; i++)
        {
            segs[(i + 9) % 10].material = cols[1 - (i % 2)];
            segs[((i + 9) % 10) + 10].material = cols[1 - (i % 2)];
            segs[i % 10].material = cols[(i % 2) + 2];
            segs[(i % 10) + 10].material = cols[(i % 2) + 2];
            yield return new WaitForSeconds(0.18f / stage);
        }
        segs[0].material = cols[0];
        segs[10].material = cols[0];
        if (answer[1].Count(x => x == true) < 1)
            Debug.LogFormat("[Not X01 #{0}] Blank submission sent.", moduleID);
        else
            Debug.LogFormat("[Not X01 #{0}] The sector{1} {2} w{3} submitted.", moduleID, answer[1].Count() < 2 ? "" : "s", string.Join(", ", Enumerable.Range(0, 10).Where(x => answer[1][x]).Select(x => nums[x].ToString()).ToArray()), answer[1].Count() < 2 ? "as" : "ere");
        if (answer[1].SequenceEqual(answer[0]))
        {
            stage++;
            if (stage > 3)
            {
                moduleSolved = true;
                module.HandlePass();
                StartCoroutine("Solve");
            }
            else
            {
                Audio.PlaySoundAtTransform("InputCorrect", transform);
                answer[0] = new bool[10];
                Ansgen();
                StartCoroutine("Sequence");
            }
        }
        else
        {
            module.HandleStrike();
            play[0] = false;
            for (int i = 0; i < 10; i++)
                labels[i].text = nums[i].ToString();
        }
        answer[1] = new bool[10];
        play[2] = false;
    }

    private IEnumerator Solve()
    {
        Audio.PlaySoundAtTransform("XSolve", transform);
        for(int i = 0; i < 7; i++)
        {
            for(int j = 0; j < 20; j++)
                segs[j].material = cols[j % 2 == 0 ? (1 - (i % 2)) * 2 : ((i % 2) * 2) + 1];
            yield return new WaitForSeconds(0.25f);
        }
        for (int i = 0; i < 20; i++)
            segs[i].material = cols[i % 2];
    }

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} bullseye [Presses the bullseye] | !{0} press <#> (#₂)... [Presses the specified sector(s), where sectors are 1-10 in clockwise order starting from north]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*bullseye\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (play[2])
            {
                yield return "sendtochaterror Cannot interact with the module while it is submitting!";
                yield break;
            }
            buttons[Random.Range(40, 42)].OnInteract();
            if (play[2] && answer[0].SequenceEqual(answer[1]))
                yield return "solve";
            else if (play[2] && !answer[0].SequenceEqual(answer[1]))
                yield return "strike";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 1)
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    int temp = -1;
                    if (!int.TryParse(parameters[i], out temp))
                    {
                        yield return "sendtochaterror!f The specified sector to press '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                    if (temp < 1 || temp > 10)
                    {
                        yield return "sendtochaterror The specified sector to press '" + parameters[i] + "' is out of range 1-10!";
                        yield break;
                    }
                }
                if (play[2])
                {
                    yield return "sendtochaterror Cannot interact with the module while it is submitting!";
                    yield break;
                }
                if (!play[0])
                {
                    yield return "sendtochaterror Cannot press any sectors until the module has been activated!";
                    yield break;
                }
                for (int i = 1; i < parameters.Length; i++)
                {
                    buttons[(int.Parse(parameters[i]) - 1) * 4 + Random.Range(0, 4)].OnInteract();
                    yield return new WaitForSeconds(0.4f);
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify at least one sector to press!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (play[0] && !play[1])
        {
            if (!play[2])
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!answer[0][i] && answer[1][i])
                    {
                        StopAllCoroutines();
                        moduleSolved = true;
                        module.HandlePass();
                        StartCoroutine("Solve");
                        yield break;
                    }
                }
            }
            else if (!answer[0].SequenceEqual(answer[1]))
            {
                StopAllCoroutines();
                moduleSolved = true;
                module.HandlePass();
                StartCoroutine("Solve");
                yield break;
            }
        }
        if (play[2] && stage == 3)
        {
            while (!moduleSolved)
                yield return true;
        }
        else
        {
            if (!play[0])
            {
                buttons[Random.Range(40, 42)].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            while (play[2])
                yield return true;
            int start = stage;
            for (int i = start; i < 4; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (answer[0][j] && !answer[1][j])
                    {
                        buttons[j * 4 + Random.Range(0, 4)].OnInteract();
                        yield return new WaitForSeconds(0.4f);
                    }
                }
                buttons[Random.Range(40, 42)].OnInteract();
                if (i != 3)
                {
                    while (play[2])
                        yield return true;
                }
            }
            while (!moduleSolved)
                yield return true;
        }
    }
}
