using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DisCFScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] buttons;
    public TextMesh disp;
    public GameObject matStore;
    public GameObject[] hidden;
    public Font[] fonts;
    public Material[] fontmats;
    public Material[] symbols;
    public Material[] patterns;

    private readonly int[] fontsizes = new int[9] { 29, 34, 24, 31, 20, 33, 33, 48, 22 };
    private readonly Vector3[] sympos = new Vector3[9] { new Vector3(-0.016f, 0.0125f, -0.016f), new Vector3(0, 0.0125f, -0.016f), new Vector3(0.016f, 0.0125f, -0.016f), new Vector3(-0.016f, 0.0125f, 0), new Vector3(0, 0.0125f, 0), new Vector3(0.016f, 0.0125f, 0), new Vector3(-0.016f, 0.0125f, 0.016f), new Vector3(0, 0.0125f, 0.016f), new Vector3(0.016f, 0.0125f, 0.016f) };
    private readonly int[][] triplets = new int[21][]
    {
        new int[3]{ 0, 0, 0}, new int[3]{ 1, 1, 1}, new int[3]{ 2, 2, 2}, new int[3]{ 3, 3, 3}, new int[3]{ 4, 4, 4}, new int[3]{ 5, 5, 5}, new int[3]{ 6, 6, 6}, new int[3]{ 7, 7, 7}, new int[3]{ 8, 8, 8},
        new int[3]{0, 1, 2}, new int[3]{ 3, 4, 5}, new int[3]{ 6, 7, 8}, new int[3]{ 0, 3, 6}, new int[3]{ 1, 4, 7}, new int[3]{ 2, 5, 8},
        new int[3]{0, 4, 8}, new int[3]{ 0, 5, 7}, new int[3]{ 1, 3, 8}, new int[3]{ 1, 5, 6}, new int[3]{2, 3, 7}, new int[3]{ 2, 4, 6}
    };
    private readonly string[][] loginfo = new string[10][]
    {
         new string[9] { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth"},
         new string[9] { "Black", "Red", "Green", "Blue", "Grey", "Cyan", "Magenta", "Yellow", "White"},
         new string[9] { "Cyan", "Magenta", "White", "Yellow", "Grey", "Red", "Black", "Green", "Blue"},
         new string[9] { "English", "French", "Esperanto", "German", "Danish", "Polish", "Finnish", "Dutch", "Italian"},
         new string[9] { "Stripe", "Polka Dot", "Wave", "Concentric Circle", "Chessboard", "Honeycomb", "Lattice", "Triangle", "Zig-zag"},
         new string[9] { "Special Elite", "Gochi Hand", "Day Poster Black", "Anonymous Pro", "Rock Salt", "Chewy", "Lobster", "Ostrich Sans", "Clarendon"},
         new string[9] { "Black", "Grey", "White", "Orange", "Lime", "Jade", "Azure", "Violet", "Rose"},
         new string[9] { "Shield", "Star", "Sun", "Globe", "Heart", "Chain", "Cube", "Cloud", "Command Key"},
         new string[9] { "Top-left", "Top-middle", "Top-right", "Middle-left", "Middle", "Middle-right", "Bottom-left", "Bottom-middle", "Bottom-right"},
         new string[9] { "Round Brackets", "Quotes", "Square Brackets", "Asterisks", "Tildes", "Colons", "Curly Brackets", "Whitespace", "Angle Brackets"}
    };
    private readonly string[,] langdisps = new string[9, 9]
    {
        {"BLACK", "RED", "GREEN", "BLUE", "GREY", "CYAN", "MAGENTA", "YELLOW", "WHITE" },
        {"NOIR", "ROUGE", "VERT", "BLEU", "GRIS", "TURQUOISE", "ROSE", "JAUNE", "BLANC" },
        {"NIGRA", "RUG"+"\u0302"+"A", "VERDA", "BLUA", "GRIZA", "TURKISA", "MAGENTO", "FLAVA", "BLANKA"},
        {"SCHWARZ", "ROT", "GRU"+"\0308"+"N", "BLAU", "GRAU", "TU"+"\u0308"+"RKIS", "ROSAROT", "GELB", "WEIß"},
        {"SORT", "RØD", "GRØN", "BLÅ", "GRÅ", "TURKUISE", "LYSERØD", "GUL", "HVID"},
        {"CZARNY", "CZERWONY", "ZIELONY", "NIEBIESKY", "SZARY", "TURKUS", "RO"+"\u0301"+"Z"+"\u0300"+"OWY", "Z"+"\u0300"+"O"+"\u0301"+"L"+"\u0338"+"TY", "BIAL"+"\u0338"+"Y"},
        {"MUSTA", "PUNAINEN", "VIHREA"+"\u0308", "SININEN", "HARMAA", "TURKOOSI", "PINKKI", "KELTAINEN", "VALKOINEN"},
        {"ZWART", "ROOD", "GROEN", "BLAUW", "GRIJS", "TURKOOIS", "ROZE", "GEEL", "WIT"},
        {"NERO", "ROSSO", "VERDE", "BLU", "GRIGIO", "TURCHESE", "ROSA", "GIALLO", "BIANCO"}
    };
    private readonly Color[][] cols = new Color[2][]
    {       
        new Color[9]{ new Color(0, 1, 1), new Color(1, 0, 1), new Color(1, 1, 1), new Color(1, 1, 0), new Color(0.65f, 0.65f, 0.65f), new Color(1, 0, 0), new Color(0.25f, 0.25f, 0.25f), new Color(0, 1, 0), new Color(0, 0, 1)},
        new Color[9]{ new Color(0.25f, 0.25f, 0.25f), new Color(0.65f, 0.65f, 0.65f), new Color(1, 1, 1), new Color(1, 0.5f, 0), new Color(0.5f, 1, 0), new Color(0, 1, 0.5f), new Color(0, 0.5f, 1), new Color(0.5f, 0, 1), new Color(1, 0, 0.5f)}
    };
    private string[] disptext = new string[9];
    private int[,] dispinfo = new int[9, 9];
    private int[] order;
    private int screen;
    private int[] selection = new int[3] { -1, -1, -1 };

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        order = Enumerable.Range(0, 9).ToArray().Shuffle();
        for (int i = 0; i < 9; i++)
        {
            int r = Random.Range(0, 21);
            if (r < 9)
                r = Random.Range(0, 21);
            for (int j = 0; j < 3; j++)
                dispinfo[j, i] = triplets[r][j];
        }
        for (int i = 0; i < 6; i++)
        {
        repeat:;
            for (int j = 0; j < 9; j++)
                dispinfo[i + 3, j] = Random.Range(0, 9);
            for (int p = 0; p < i + 2; p++)
            {
                for (int q = p + 1; q < i + 3; q++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        int[] t = new int[3] { dispinfo[i + 3, j], dispinfo[p, j], dispinfo[q, j] }.OrderBy(x => x).ToArray();
                        if (!triplets.Any(x => x.SequenceEqual(t)))
                            break;
                        if (j > 7)
                            goto repeat;
                    }
                }
            }
        }
        string[] f = new string[9];
        for (int i = 0; i < 9; i++)
        {
            disptext[i] = "('[*~:{ <"[dispinfo[i, 8]] + langdisps[dispinfo[i, 2], dispinfo[i, 0]] + ")']*~:} >"[dispinfo[i, 8]];
            string[] t = Enumerable.Range(0, 9).Select(x => loginfo[x + 1][dispinfo[i, x]]).ToArray();
            f[i] = string.Format("{2} in {3}, written in {4}, coloured {5}, and contained in {6} with a {7} {8} pattern and a {9} in the {10} position.", moduleID, loginfo[0][i], t[0], t[2], t[4], t[1], t[8], t[5], t[3], t[6], t[7]);               
        }
        for (int i = 0; i < 9; i++)
            Debug.LogFormat("[Discolour Flash #{0}] The {1} display is: {2}", moduleID, loginfo[0][i], f[order[i]]);
        int[] s = Enumerable.Range(0, 3).Select(x => Array.IndexOf(order, x) + 1).OrderBy(x => x).ToArray();
        Debug.LogFormat("[Discolour Flash #{0}] Displays {1}, {2}, and {3} should be submitted.", moduleID, s[0], s[1], s[2]);
        foreach (KMSelectable button in buttons)
        {
            bool yes = button == buttons[0];
            button.OnHighlight = delegate () { if (!moduleSolved) { hidden[yes ? 3 : 1].SetActive(true); hidden[yes ? 2 : 0].SetActive(false); } };
            button.OnHighlightEnded = delegate () { hidden[yes ? 3 : 1].SetActive(false); hidden[yes ? 2 : 0].SetActive(true); };
            button.OnInteract = delegate () { Press(yes); return false; };
        }
        StartCoroutine("Seq");
        matStore.SetActive(false);
        hidden[1].SetActive(false);
        hidden[3].SetActive(false);
    }

    private void Press(bool y)
    {
        if (!moduleSolved)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Stamp, buttons[y ? 0 : 1].transform);
            buttons[y ? 0 : 1].AddInteractionPunch(0.66f);
            if (y)
            {
                disp.fontStyle = FontStyle.Italic;
                if (!selection.Contains(order[screen])) {
                    for (int i = 0; i < 3; i++)
                        if (selection[i] < 0)
                        {
                            selection[i] = order[screen];
                            break;
                        }
                    if (selection.All(x => x >= 0))
                    {
                        int[] s = selection.Select(x => Array.IndexOf(order, x) + 1).OrderBy(x => x).ToArray();
                        Debug.LogFormat("[Discolour Flash {0}] Submitted displays {1}, {2}, and {3}.", moduleID, s[0], s[1], s[2]);
                        if (selection.OrderBy(x => x).SequenceEqual(new int[] { 0, 1, 2 }))
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            Audio.PlaySoundAtTransform("BlipSolve", transform);
                            StopCoroutine("Seq");
                            disp.text = langdisps[0, Array.IndexOf(order, screen)];
                            disp.fontSize = 48;
                            disp.font = fonts[7];
                            disp.GetComponent<Renderer>().material = fontmats[7];
                            disp.fontStyle = FontStyle.Normal;
                            hidden[2].SetActive(true);
                            hidden[3].SetActive(false);
                        }
                        else
                        {
                            disp.fontStyle = FontStyle.Normal;
                            selection = new int[3] { -1, -1, -1 };
                            module.HandleStrike();
                        }
                    }
                }
            }
            else
            {
                disp.fontStyle = FontStyle.Normal;
                selection = new int[3] { -1, -1, -1 };
            }
        }
    }

    private IEnumerator Seq()
    {
        while (!moduleSolved)
        {
            int a = order[screen];
            int[] f = Enumerable.Range(0, 9).Select(x => dispinfo[a, x]).ToArray();
            disp.text = disptext[a];
            disp.color = cols[0][f[1]];
            disp.font = fonts[f[4]];
            disp.GetComponent<Renderer>().material = fontmats[f[4]];
            disp.fontSize = fontsizes[f[4]];
            hidden[1].GetComponent<Renderer>().material = patterns[f[3]];
            hidden[1].GetComponent<Renderer>().material.color = cols[1][f[5]];
            hidden[3].GetComponent<Renderer>().material = symbols[f[6]];
            hidden[3].transform.localPosition = sympos[f[7]];
            if (selection.Contains(a))
                disp.fontStyle = FontStyle.Italic;
            else
                disp.fontStyle = FontStyle.Normal;
            yield return new WaitForSeconds(1.1f);
            screen++;
            screen %= 9;
        }
    }
}
