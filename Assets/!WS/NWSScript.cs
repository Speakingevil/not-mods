using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NWSScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public GameObject[] hlrings;
    public Renderer[] rings;
    public Material[] ringmats;
    public TextMesh[] letters;

    private readonly string con = "BCDFGHJKLMNPQRSTVWXYZ";
    private readonly List<int> pairs = new List<int> { 12, 16, 2, 3, 32, 30, 26, 20, 7, 4, 1, 34, 9, 11, 23, 35, 24, 18, 17, 22, 6, 33, 8, 0, 5, 28, 27, 10, 21, 31, 25, 19, 15, 29, 13, 14 };
    private readonly int[,,] moves = new int[9, 21, 2]
    {
        {{4, 5}, {4, 0}, {4, 1}, {5, 4}, {5, 5}, {5, 0}, {5, 1}, {5, 2}, {0, 4}, {0, 5}, {0, 1}, {0, 2}, {0, 0}, {1, 4}, {1, 5}, {1, 0}, {1, 1}, {1, 2}, {2, 5}, {2, 0}, {2, 1} },
        {{0, 1}, {1, 1}, {1, 0}, {1, 5}, {0, 5}, {5, 5}, {5, 0}, {5, 1}, {5, 2}, {0, 2}, {1, 2}, {2, 1}, {2, 0}, {0, 0}, {2, 5}, {1, 4}, {0, 4}, {5, 4}, {4, 5}, {4, 0}, {4, 1} },
        {{0, 4}, {0, 5}, {4, 0}, {5, 0}, {0, 2}, {0, 1}, {2, 0}, {1, 0}, {5, 4}, {5, 5}, {4, 5}, {4, 1}, {5, 1}, {5, 2}, {0, 0}, {1, 2}, {1, 1}, {2, 1}, {2, 5}, {1, 5}, {1, 4} },
        {{2, 0}, {1, 5}, {0, 4}, {5, 5}, {4, 0}, {5, 1}, {0, 2}, {1, 1}, {5, 0}, {4, 1}, {5, 2}, {0, 1}, {1, 2}, {2, 1}, {1, 0}, {0, 0}, {2, 5}, {4, 1}, {1, 2}, {5, 4}, {4, 5} },
        {{5, 4}, {0, 4}, {1, 4}, {2, 1}, {1, 1}, {0, 1}, {5, 1}, {4, 1}, {4, 5}, {5, 5}, {0, 5}, {1, 5}, {2, 5}, {1, 2}, {0, 2}, {5, 2}, {0, 0}, {4, 0}, {5, 0}, {1, 0}, {2, 0} },
        {{4, 1}, {5, 2}, {1, 2}, {2, 1}, {2, 5}, {1, 4}, {5, 4}, {4, 5}, {4, 0}, {5, 1}, {0, 2}, {1, 1}, {2, 0}, {1, 5}, {0, 4}, {5, 5}, {5, 0}, {0, 0}, {0, 1}, {1, 0}, {0, 5} },
        {{4, 4}, {4, 5}, {5, 4}, {5, 5}, {0, 4}, {0, 5}, {1, 4}, {1, 5}, {2, 4}, {2, 5}, {4, 1}, {4, 2}, {5, 1}, {5, 2}, {0, 1}, {0, 2}, {1, 1}, {1, 2}, {0, 0}, {2, 1}, {2, 2} },
        {{2, 4}, {2, 5}, {2, 0}, {2, 1}, {2, 2}, {1, 4}, {1, 5}, {1, 0}, {1, 1}, {1, 2}, {5, 2}, {5, 1}, {5, 0}, {5, 5}, {5, 4}, {4, 2}, {4, 1}, {4, 0}, {4, 5}, {0, 0}, {4, 4} },
        {{2, 2}, {1, 2}, {1, 1}, {2, 1}, {3, 1}, {4, 2}, {4, 1}, {5, 1}, {5, 2}, {5, 3}, {4, 4}, {5, 4}, {5, 5}, {4, 5}, {3, 5}, {2, 4}, {2, 5}, {1, 5}, {1, 4}, {1, 3}, {0, 0} },
    };
    private int[] del = new int[3];
    private string grid;
    private string[] missing = new string[3];
    private string[] ans = new string[12];
    private bool[,] assigned = new bool[6, 6];
    private bool[] selected = new bool[36];
    private int anscount;
    private bool hidden;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        foreach (TextMesh l in letters)
            l.text = string.Empty;       
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        del = new int[3] { Random.Range(0, 6), Random.Range(0, 6), Random.Range(0, 9) };
        List<string> gri = con.Select(x => x.ToString()).ToList();
        for (int i = 0; i < 3; i++)
        {
            int d = (5 * i) + del[i];
            missing[i] = gri[d];
            gri.RemoveAt(d);
        }
        gri = gri.Concat(gri).ToList().Shuffle().ToList();
        grid = string.Join("", gri.ToArray());
        Debug.LogFormat("[Not Word Search #{0}] The grid:\n[Not Word Search #{0}] {1}", moduleID, string.Join("\n[Not Word Search #" + moduleID + "] ", Enumerable.Range(0, 6).Select(x => string.Join(" ", gri.Where((y, k) => k >= (6 * x) && k < (6 * x) + 6).ToArray())).ToArray()));
        Debug.LogFormat("[Not Word Search #{0}] The missing consonants are: {1}", moduleID, string.Join(", ", missing.ToArray()));
        for (int i = 0; i < 36; i++)
            letters[i].text = gri[i];
        int pos = pairs.IndexOf((del[0] * 6) + del[1]);
        int[] cpos = new int[2] { pos / 6, pos % 6 };
        for (int i = 0; i < 6; i++)
        {
            string t = grid[pos].ToString();
            int[] s = Enumerable.Range(0, 36).Where(x => grid[x].ToString() == t).ToArray();
            for (int j = 0; j < 2; j++)
            {
                assigned[s[j] / 6, s[j] % 6] = true;
                ans[(2 * i) + j] = t;
            }
            if(i < 5)
            {
                int[] move = new int[2];
                int c = con.IndexOf(t);
                for(int j = 0; j < 2; j++)
                   move[j] = moves[del[2], c, j];
            redo:;
                if (move[0] != 0 || move[1] != 0)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            cpos[k] += move[k];
                            cpos[k] %= 6;
                        }
                        if (!assigned[cpos[0], cpos[1]])
                            goto next;
                    }
                }
                c = c++ % 21;
                for (int j = 0; j < 2; j++)
                    move[j] = moves[del[2], c, j];
                goto redo;
            next:;
                pos = (cpos[0] * 6) + cpos[1];
            }
        }
        Debug.LogFormat("[Not Word Search #{0}] Press the pairs of letters in the order: {1}", moduleID, string.Join(", ", ans.Where((x, i) => i % 2 == 0).ToArray()));
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate () { Press(b); return false; };
        }
    }

    private void Press(int b)
    {
        if (!moduleSolved && !selected[b])
        {
            buttons[b].AddInteractionPunch(0.2f);
            if(grid[b].ToString() == ans[anscount])
            {
                Audio.PlaySoundAtTransform("Good", buttons[b].transform);
                if (!hidden)
                {
                    hidden = true;
                    foreach (TextMesh l in letters)
                        l.text = string.Empty;
                }
                selected[b] = true;
                hlrings[b].transform.Rotate(180, 0, 0);
                rings[b].material = ringmats[0];
                if (anscount < 11)
                    anscount++;
                else
                {
                    moduleSolved = true;
                    module.HandlePass();
                    foreach (Renderer r in rings)
                        r.material = ringmats[0];
                    Audio.PlaySoundAtTransform("WordSolve", transform);
                    StartCoroutine(Solve());
                }
            }
            else
            {
                Audio.PlaySoundAtTransform("Bad", buttons[b].transform);
                if (hidden)
                {
                    hidden = false;
                    for (int i = 0; i < 36; i++)
                        letters[i].text = grid[i].ToString();
                }
                module.HandleStrike();
                StartCoroutine(Bad(b));
            }
        }
    }

    private IEnumerator Bad(int b)
    {
        rings[b].material = ringmats[1];
        hlrings[b].transform.Rotate(180, 0, 0);
        yield return new WaitForSeconds(1);
        hlrings[b].transform.Rotate(180, 0, 0);
    }

    private IEnumerator Solve()
    {
        yield return new WaitForSeconds(1.7f);
        for(int i = 0; i < 4; i++)
        {
            foreach (GameObject g in hlrings)
                g.transform.Rotate(180, 0, 0);
            yield return new WaitForSeconds(0.4f);
        }
    }
}
