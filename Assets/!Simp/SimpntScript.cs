using System.Collections;
using System.Linq;
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
                        Debug.LogFormat("[Simpleton't #{0}] Button pressed on a zero.");
                        if (ans[0].Any(x => x))
                            module.HandleStrike();
                        else
                            module.HandlePass();
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
}
