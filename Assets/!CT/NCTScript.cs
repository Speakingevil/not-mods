using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KModkit;
using Random = UnityEngine.Random;

public class NCTScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public GameObject flipswitch;
    public KMSelectable flip;
    public Text text;

    private readonly string[] disptexts = new string[81]{
        "1 3 2 4",
        "LEFT ARROW LEFT WORD RIGHT ARROW LEFT WORD RIGHT ARROW RIGHT WORD",
        "BLANK",
        "LITERALLY BLANK",
        "FOR THE LOVE OF ALL THAT IS GOOD AND HOLY PLEASE FULLSTOP FULLSTOP.",
        "AN ACTUAL LEFT ARROW LITERAL PHRASE",
        "FOR THE LOVE OF - THE DISPLAY JUST CHANGED, I DIDN'T KNOW THIS MOD COULD DO THAT. DOES IT MENTION THAT IN THE MANUAL?",
        "ALL WORDS ONE THREE TO FOR FOR AS IN THIS IS FOR YOU",
        "LITERALLY NOTHING",
        "NO, LITERALLY NOTHING",
        "THE WORD LEFT",
        "HOLD ON IT'S BLANK",
        "SEVEN WORDS FIVE WORDS THREE WORDS THE PUNCTUATION FULLSTOP",
        "THE PHRASE THE WORD STOP TWICE",
        "THE FOLLOWING SENTENCE THE WORD NOTHING",
        "ONE THREE TO FOR",
        "THREE WORDS THE WORD STOP",
        "DISREGARD WHAT I JUST SAID. FOUR WORDS, NO PUNCTUATION. ONE THREE 2 4.",
        "1 3 2 FOR",
        "DISREGARD WHAT I JUST SAID. TWO WORDS THEN TWO DIGITS. ONE THREE 2 4.",
        "WE JUST BLEW UP",
        "NO REALLY.",
        "← LEFT → LEFT → RIGHT",
        "ONE AND THEN 3 TO 4",
        "STOP TWICE",
        "LEFT",
        "PERIOD PERIOD",
        "THERE ARE THREE WORDS NO PUNCTUATION READY? STOP DOT PERIOD",
        "NOVEBMER OSCAR SPACE, LIMA INDIGO TANGO ECHO ROMEO ALPHA LIMA LIMA YANKEE SPACE NOVEMBER OSCAR TANGO HOTEL INDEGO NOVEMBER GOLF",
        "FIVE WORDS THREE WORDS THE PUNCTUATION FULLSTOP",
        "THE PHRASE: THE PUNCTUATION FULLSTOP",
        "EMPTY SPACE",
        "ONE THREE TWO FOUR",
        "IT'S SHOWING NOTHING",
        "LIMA ECHO FOXTROT TANGO SPACE ALPHA ROMEO ROMEO OSCAR RISKY SPACE SIERRA YANKEE MIKE BRAVO OSCAR LIMA",
        "ONE 3 2 4",
        "STOP.",
        ".PERIOD",
        "NO REALLY STOP",
        "1 3 TOO 4",
        "PERIOD TWICE",
        "1 3 TOO WITH 2 OHS FOUR",
        "1 3 TO 4",
        "STOP DOT PERIOD",
        "LEFT LEFT RIGHT LEFT RIGHT RIGHT",
        "IT LITERALLY SAYS THE WORD ONE AND THEN THE NUMBERS 2 3 4",
        "ONE IN LETTERS 3 2 4 IN NUMBERS",
        "WAIT FORGET EVERYTHING I JUST SAID, TWO WORDS THEN TWO SYMBOLS THEN TWO WORDS: ← ← RIGHT LEFT → →",
        "1 THREE TWO FOUR",
        "PERIOD",
        ".STOP",
        "NOVEBMER OSCAR SPACE, LIMA INDIA TANGO ECHO ROMEO ALPHA LIMA LIMA YANKEE SPACE NOVEMBER OSCAR TANGO HOTEL INDIA NOVEMBER GOLF",
        "LIMA ECHO FOXTROT TANGO SPACE ALPHA ROMEO ROMEO OSCAR WHISKEY SPACE SIERRA YANKEE MIKE BRAVO OSCAR LIMA",
        "NOTHING",
        "THERE'S NOTHING",
        "STOP STOP",
        "RIGHT ALL IN WORDS STARTING NOW ONE TWO THREE FOUR",
        "THE PHRASE THE WORD LEFT",
        "LEFT ARROW SYMBOL TWICE THEN THE WORDS RIGHT LEFT RIGHT THEN A RIGHT ARROW SYMBOL",
        "LEFT LEFT RIGHT ← RIGHT →",
        "NO COMMA LITERALLY NOTHING",
        "HOLD ON CRAZY TALK WHILE I DO THIS NEEDY",
        "THIS ONE IS ALL ARROW SYMBOLS NO WORDS",
        "THE WORD STOP TWICE",
        "← ← RIGHT LEFT → →",
        "THE PUNCTUATION FULLSTOP",
        "1 3 TOO WITH TWO OS 4",
        "THREE WORDS THE PUNCTUATION FULLSTOP",
        "OK WORD FOR WORD LEFT ARROW SYMBOL TWICE THEN THE WORDS RIGHT LEFT RIGHT THEN A RIGHT ARROW SYMBOL",
        "DOT DOT",
        "LEFT ARROW",
        "AFTER I SAY BEEP FIND THIS PHRASE WORD FOR WORD BEEP AN ACTUAL LEFT ARROW",
        "ONE THREE 2 WITH TWO OHS 4",
        "LEFT ARROW SYMBOL",
        "AN ACTUAL LEFT ARROW",
        "THAT'S WHAT IT'S SHOWING",
        "THE PHRASE THE WORD NOTHING",
        "THE WORD ONE AND THEN THE NUMBERS 3 2 4",
        "ONE 3 2 FOUR",
        "ONE WORD THEN PUNCTUATION. STOP STOP.",
        "THE WORD BLANK"};
    private readonly string[] morsecodes = new string[36] { "-----", ".----", "..---", "...--", "....-", ".....", "-....", "--...", "---..", "----.", ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
    private readonly string zerotoz = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private string[] alphnums = new string[8] { "0123456789", "", "", " ", "BCDFGHJKLMNPQRSTVWXYZ", ".'-←→,?:", "AEIOU", ""};
    private bool[] edgecheck = new bool[8];
    private int[] keyints = new int[5];
    private int[] timesub = new int[3];
    private int cyclenum;
    private string[] morsesub = new string[2];
    private string initialmessage;
    private bool initialphase;
    private bool morsephase;
    private bool off;
    
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
        edgecheck[0] = (info.GetBatteryHolderCount() * 2) - info.GetBatteryCount() >= 2;
        edgecheck[1] = info.GetOffIndicators().Count() >= 2;
        edgecheck[2] = info.GetOnIndicators().Count() >= 2;
        edgecheck[3] = info.GetPortPlates().Any(x => x.Count() == 0);
        edgecheck[4] = info.GetPortPlates().Any(x => x.Count() >= 3);
        edgecheck[5] = info.GetPorts().Distinct().Count() >= 6;
        edgecheck[6] = info.GetPorts().Count() >= 9;
        edgecheck[7] = edgecheck.All(x => x == false);
        alphnums[1] = string.Join("", info.GetOffIndicators().ToArray());
        alphnums[2] = string.Join("", info.GetOnIndicators().ToArray());
        alphnums[7] = info.GetSerialNumber();
        flip.OnInteract += delegate () { Flip(); return false; };
        flip.OnInteractEnded += delegate () { Stopflip(); };
        Reset();
    }

    private void Reset()
    {
        timesub[0] = 0;
        morsesub[1] = string.Empty;
        initialphase = true;
        morsephase = false;
        off = false;
        for (int i = 0; i < 5; i++)
        {
            keyints[i] = Random.Range(0, 81);
            while (keyints.Where((x, k) => k < i).Contains(keyints[i]))
                keyints[i] = Random.Range(0, 81);
        }
        initialmessage = disptexts[keyints[0]];
        text.text = initialmessage;
        for(int i = 0; i < initialmessage.Length; i++)
        {
            for(int j = 0; j < 8; j++)
                if(edgecheck[j] && alphnums[j].Contains(disptexts[keyints[0]][i]))
                {
                    timesub[0]++;
                    break;
                }
        }
        Debug.LogFormat("[Not Crazy Talk #{0}] Phase 1: The displayed message is \"{1}\".", moduleID, initialmessage);
        Debug.LogFormat("[Not Crazy Talk #{0}] {1} character{2} in the message {3} valid.", moduleID, timesub[0], timesub[0] == 1 ? "" : "s", timesub[0] == 1 ? "is" : "are");
        timesub[0] %= 10;
        Debug.LogFormat("[Not Crazy Talk #{0}] The switch should be flipped up when the last digit of the seconds timer is {1}.", moduleID, timesub[0]);
        initialmessage = Regex.Replace(initialmessage, "[^a-zA-Z0-9]", String.Empty);
        cyclenum = keyints.Where((x, k) => k > 0).Select(i => i / 9).Sum() + 4;
        morsesub[0] = morsecodes[zerotoz.IndexOf(initialmessage[(cyclenum - 1) % initialmessage.Length].ToString())];
    }

    private void Flip()
    {
        if (!moduleSolved && !off)
        {
            Audio.PlaySoundAtTransform("Click", flipswitch.transform);
            flipswitch.transform.localScale = new Vector3(0.011f, -flipswitch.transform.localScale.y, 0.0125f);
            if (initialphase)
            {
                timesub[1] = (int)info.GetTime() % 10;
                initialphase = false;
                Debug.LogFormat("[Not Crazy Talk #{0}] The switch was flipped up when the digit of the seconds timer was {1}.", moduleID, timesub[1]);
                Debug.LogFormat("[Not Crazy Talk #{0}] Phase 2: The displayed messages are \"{1}\".", moduleID, string.Join("\", \"", keyints.Where((x, k) => k > 0).Select(x => disptexts[x]).ToArray()));
                Debug.LogFormat("[Not Crazy Talk #{0}] The values of the displayed messages are {1}.", moduleID, string.Join(", ", keyints.Where((x, k) => k > 0).Select(x => ((x / 9) + 1).ToString()).ToArray()));
                Debug.LogFormat("[Not Crazy Talk #{0}] The {1}{2} character of \"{3}\" is {4}.", moduleID, cyclenum, cyclenum / 10 == 1 ? "th" : (cyclenum % 10 == 1? "st" : (cyclenum % 10 == 2 ? "nd" : (cyclenum % 10 == 3 ? "rd" : "th"))), initialmessage, initialmessage[(cyclenum - 1) % initialmessage.Length]);
                Debug.LogFormat("[Not Crazy Talk #{0}] {1} in Morse code is \"{2}\".", moduleID, initialmessage[(cyclenum - 1) % initialmessage.Length], morsesub[0]);
               StartCoroutine("DispCycle");
            }
            else
            {
                if (!morsephase)
                    morsephase = true;
                else
                    StopCoroutine("Submit");
                timesub[2] = (int)info.GetTime();
            }
        }
    }

    private void Stopflip()
    {
        if (morsephase && !off)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, flipswitch.transform);
            flipswitch.transform.localScale = new Vector3(0.011f, -flipswitch.transform.localScale.y, 0.0125f);
            StartCoroutine("Submit");
            if ((int)info.GetTime() == timesub[2])
                morsesub[1] += ".";
            else
                morsesub[1] += "-";
        }
    }

    private IEnumerator DispCycle()
    {
        for(int i = 1; i < 5; i++)
        {
            text.text = disptexts[keyints[i]];
            yield return new WaitForSeconds(1);
            if (i == 4)
                i = 0;
        }
    }

    private IEnumerator Submit()
    {
        yield return new WaitForSeconds(2);
        off = true;
        StopCoroutine("DispCycle");
        string checktext = timesub[1].ToString() + (morsecodes.Contains(morsesub[1]) ? zerotoz[Array.IndexOf(morsecodes, morsesub[1])].ToString() : "?");
        text.text = checktext;
        Debug.LogFormat("[Not Crazy Talk #{0}] Submitted \"{1}\"", moduleID, morsesub[1]);
        if (timesub[0] == timesub[1] && morsesub[0] == morsesub[1])
        {
            moduleSolved = true;
            module.HandlePass();
            Debug.LogFormat("[Not Crazy Talk #{0}] Correct.", moduleID);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        }
        else
            module.HandleStrike();
        yield return new WaitForSeconds(1);
        if (moduleSolved)
            text.text = string.Empty;
        else
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, flipswitch.transform);
            flipswitch.transform.localScale = new Vector3(0.011f, -flipswitch.transform.localScale.y, 0.0125f);
            Debug.LogFormat("[Not Crazy Talk #{0}] Resetting.", moduleID);
            Reset();
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} up at <#> [Flips switch up when the specified digit is the last seconds digit of the timer] | !{0} transmit <.-> [Transmits Morse code]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        while (off)
            yield return true;
        if (initialphase)
        {
            if(command.Length != 7 || !command.Contains("up at ") || !"0123456789".Contains(command.Last().ToString()))
            {
                yield return "sendtochaterror!f " + command + " is invalid syntax for flip up commands.";
                yield break;
            }
            while (((int)info.GetTime() % 10).ToString() != command.Last().ToString())
                yield return true;
            flip.OnInteract();
        }
        else
        {
            string[] commands = command.Split(' ');
            if(commands.Length != 2 || commands[0] != "transmit" || !Regex.Match(commands[1], "^[.-]+").Success)
            {
                yield return "sendtochaterror!f " + command + " is invalid syntax for transmission commands.";
                yield break;
            }
            yield return "solve";
            yield return "strike";
            int time = (int)info.GetTime();
            for (int i = 0; i < commands[1].Length; i++)
            {
                while ((int)info.GetTime() == time)
                    yield return true;
                yield return null;
                flip.OnInteract();
                yield return new WaitForSeconds(commands[1][i] == '.' ? 0.1f : 1.1f);
                flip.OnInteractEnded();
                time = (int)info.GetTime();
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        while (off)
            yield return true;
        if (initialphase)
        {
            while ((int)info.GetTime() % 10 != timesub[0])
                yield return true;
            flip.OnInteract();
        }
        else
            timesub[1] = timesub[0];
        morsesub[1] = "";
        int time = (int)info.GetTime();
        for(int i = 0; i < morsesub[0].Length; i++)
        {
            while ((int)info.GetTime() == time)
                yield return false;
            flip.OnInteract();
            yield return new WaitForSeconds(morsesub[0][i] == '.' ? 0.1f : 1.1f);
            flip.OnInteractEnded();
            time = (int)info.GetTime();
        }
        while (!moduleSolved)
            yield return true;
    }
}
