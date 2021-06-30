using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class NEMScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public KMBombInfo info;
    public TextMesh display;

    private readonly string[] letters = new string[26] { ":)", "=(", "(:", ")=", ":(", "=)", "):", "(=", ":|", "|:", "=|", "|=", ":-)", "=-(", "(-:", ")-=", ":-(", "=-)", ")-:", "(-=", ":-|", "|-:", "=-|", "|-=", "(:<", ">:)" };
    private readonly string[] words = new string[50] { "APEX", "ATOM", "BITS", "BURN", "CHEW", "COZY", "DASH", "DRUM", "ECHO", "EPIC", "FIVE", "FLUX", "GNAW", "GYRI", "HAJI", "HELM", "IAMB", "ITCH", "JAWS", "JUMP", "KEYS", "KNOT", "LAZY", "LIME", "MUON", "MYTH", "NEXT", "NOVA", "OGRE", "ONYX", "PICK", "POEM", "QUAY", "QUIZ", "ROSE", "RUBY", "SNOW", "SURF", "TOMB", "TWIN", "UNDO", "USER", "VERB", "VOID", "WAVY", "WRAP", "YEAR", "YUCK", "ZERO", "ZINC" };
    private readonly int[] values = new int[50] { 789, 118, 256, 451, 385, 263, 830, 19, 754, 625, 551, 121, 790, 986, 214, 403, 25, 709, 187, 129, 866, 439, 56, 711, 592, 39, 678, 237, 470, 95, 286, 359, 961, 110, 271, 399, 612, 89, 834, 252, 26, 717, 410, 1, 339, 285, 21, 144, 999, 654};
    private int index;
    private string d;
    private int[] order = new int[4];
    private int[] ans = new int[2];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        index = Random.Range(0, 50);
        order = new int[] { 0, 1, 2, 3 }.Shuffle();
        d = words[index];
        display.text = string.Join("", Enumerable.Range(0, 4).Select(x => letters["ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(d[order[x]].ToString())]).ToArray());
        Debug.LogFormat("[Not Emoji Math #{0}] The displayed text is \"{1}\". Decoding the text yields \"{2}\". Unscrambling yields \"{3}\".", moduleID, display.text, string.Join("", Enumerable.Range(0, 4).Select(x => d[order[x]].ToString()).ToArray()), d);
        d = display.text;
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate ()
            {
                if (!moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    button.AddInteractionPunch(0.6f);
                    switch (b)
                    {
                        case 11:
                            if (display.text != d)
                            {
                                Debug.LogFormat("[Not Emoji Math #{0}] Submitted {1}.", moduleID, ans[1]);
                                if (ans[1] == ans[0])
                                {
                                    module.HandlePass();
                                    moduleSolved = true;
                                }
                                else
                                {
                                    module.HandleStrike();
                                    ans[1] = 0;
                                    display.text = d;
                                }
                            }
                            break;
                        case 10:
                            ans[1] = 0;
                            display.text = d;
                            break;
                        default:
                            if (ans[1] < 1000)
                            {
                                ans[1] *= 10;
                                ans[1] += b;
                                display.text = ans[1].ToString();
                            }
                            break;
                    }
                }
                return false;
            };
        }
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        Debug.LogFormat("[Not Emoji Math #{0}] Applying functions {1} to the value {2}.", moduleID, string.Join(", ", info.GetSerialNumberLetters().Select(x => x.ToString()).ToArray()), values[index]);
        int[] funcs = info.GetSerialNumberLetters().Select(x => "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x.ToString())).ToArray();
        int k = funcs.Length;
        int n = values[index];
        List<int> iters = new List<int> { n };
        for (int i = 0; i < k; i++)
        {
            switch (funcs[i])
            {
                case 0: n += display.text.Length; break;
                case 1: n *= 2; break;
                case 2: n += funcs.Sum() + k; break;
                case 3: n += info.GetSerialNumberNumbers().Sum(); break;
                case 4: n *= 6 - k; break;
                case 5: n += info.GetIndicators().Select(x => "#ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x[0].ToString())).Sum(); break;
                case 6: n += info.GetSerialNumberNumbers().ToArray()[0] * (int)Mathf.Pow(10, 3 - i); break;
                case 7: n *= k - i; break;
                case 8: n = Mathf.Abs(n - (values[index] * 10)); break;
                case 9: n += string.Join("", info.GetIndicators().ToArray()).Distinct().Count(); break;
                case 10: n += n.ToString().Select(x => x - '0').Sum(); break;
                case 11: n += info.GetIndicators().Select(x => "#ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x[2].ToString())).Sum(); break;
                case 12: n += 1000; break;
                case 13: n = Mathf.Abs(5000 - n); break;
                case 14: int y = (int)Mathf.Pow(10, (int)Mathf.Log10(n)); n = ((n % 10) * y) + (n / 10); break;
                case 15: int z = (int)Mathf.Pow(10, (int)Mathf.Log10(n)); n = ((n % z) * 10) + (n / z); break;
                case 16: n += (int)Mathf.Pow(info.GetSerialNumberNumbers().Sum(), 2); break;
                case 17: n *= ((n - 1) % 9) + 1; break;
                case 18: n = n.ToString().Reverse().Aggregate(0, (b, x) => 10 * b + x - '0'); break;
                case 19: n = Mathf.Abs(10000 - n); break;
                case 20: n = n.ToString().OrderBy(x => x).Aggregate(0, (b, x) => 10 * b + x - '0'); break;
                case 21: n = n.ToString().OrderBy(x => x).Reverse().Aggregate(0, (b, x) => 10 * b + x - '0'); break;
                case 22: n = (n * 2).ToString().OrderBy(x => x).Aggregate(0, (b, x) => 10 * b + x - '0'); break;
                case 23: n = values[index] + i; break;
                case 24: n += values[index]; break;
                default: n = n % 2 == 0 ? n / 2 : (3 * n) + 1; break;
            }
            iters.Add(n);
        }
        Debug.LogFormat("[Not Emoji Math #{0}] {1}", moduleID, string.Join(" \u2192 ", iters.Select(x => x.ToString()).ToArray()));
        n %= 10000;
        Debug.LogFormat("[Not Emoji Math #{0}] The output is {1}.", moduleID, n);
        for (int i = 0; i < 4; i++)
            ans[0] += ((n / (int)Mathf.Pow(10, 3 - i)) % 10) * (int)Mathf.Pow(10, 3 - order[i]);
        Debug.LogFormat("[Not Emoji Math #{0}] The final code is {1}.", moduleID, ans[0]);
    }
}
