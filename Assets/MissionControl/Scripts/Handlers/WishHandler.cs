using System.Collections;
using System.Linq;
using UnityEngine;

/// Author: Espik
internal class WishHandler {
    internal readonly int[] WISH_THRESHOLDS = { 13, 26, 33, 41, 49, 59, 65, 71, 78, 83, 88, 93 };
    private readonly string[] WISH_MODULES = { "notX01", "deceptiveRainbowArrowsModule", "cube", "ChaoticCountdownModule", "whiteCipher",
        "blackCipher", "bamboozlingButton", "TripleTraversalModule", "rgbMaze", "perceptron" };
    private readonly string[] WISH_HARD_MODULES = { "EncryptionLingoModule", "WalkingCubeModule" };

    internal int buttonPresses = 0;
    private readonly float TIME_LOSS = 0.1f; // 10%

    private KMBombModule[] mystifiedModule = new KMBombModule[12];
    private Vector3[] mystifyScale = new Vector3[12];

    private MissionControl comp;

    internal IEnumerator ProcessWish(MissionControl comp) {
        this.comp = comp;
        comp.StartCoroutine(HideWishModules());

        comp.ButtonSelectable.OnInteract = delegate () {
            ButtonPressed();
            return false;
        };

        yield return null;
    }

    // Finds and covers the modules for Wish
    private IEnumerator HideWishModules() {
        int modulesLeft = 10;
        int hardModulesLeft = 2;

        int[] indecies = new int[10];
        for (int i = 0; i < indecies.Length; i++)
            indecies[i] = i;

        int[] hardIndecies = { 10, 11 };

        // Code from Mystery Module
        for (int i = 0; i < comp.transform.parent.childCount; i++) {
            var module = comp.transform.parent.GetChild(i).gameObject.GetComponent<KMBombModule>();
            if (module == null)
                continue;

            // First set of Wish modules
            if (modulesLeft > 0) {
                foreach (string name in WISH_MODULES) {
                    if (module.ModuleType == name) {
                        int rand = UnityEngine.Random.Range(0, modulesLeft);
                        mystifiedModule[indecies[rand]] = module;
                        comp.StartCoroutine(CoverModule(indecies[rand]));

                        modulesLeft--;
                        for (int j = rand; j < modulesLeft; j++) {
                            indecies[j] = indecies[j + 1];
                        }
                        break;
                    }
                }
            }

            // Second set of Wish modules
            if (hardModulesLeft > 0) {
                foreach (string name in WISH_HARD_MODULES) {
                    if (module.ModuleType == name) {
                        int rand = UnityEngine.Random.Range(0, hardModulesLeft);
                        mystifiedModule[hardIndecies[rand]] = module;
                        comp.StartCoroutine(CoverModule(hardIndecies[rand]));

                        hardModulesLeft--;
                        for (int j = rand; j < hardModulesLeft; j++) {
                            hardIndecies[j] = hardIndecies[j + 1];
                        }
                        break;
                    }
                }
            }
        }

        yield return null;
    }

    // Covers a module for Wish
    private IEnumerator CoverModule(int num) {
        // Code from Mystery Module
        Debug.LogFormat("[Mission Control #{0}] Hiding module: {1}", comp.moduleId, mystifiedModule[num].ModuleDisplayName);

        comp.Cover[num].SetActive(true);
        var mysPos = mystifiedModule[num].transform.localPosition;
        comp.Cover[num].transform.parent = mystifiedModule[num].transform.parent;

        var scale = new Vector3(.95f, .95f, .95f);
        comp.Cover[num].transform.localScale = scale;
        comp.Cover[num].transform.rotation = mystifiedModule[num].transform.rotation;
        if (comp.Cover[num].transform.rotation == new Quaternion(0f, 0f, 1f, 0f))
            comp.Cover[num].transform.localPosition = new Vector3(mysPos.x, mysPos.y - 0.02f, mysPos.z);
        else
            comp.Cover[num].transform.localPosition = new Vector3(mysPos.x, mysPos.y + 0.02f, mysPos.z);
        Debug.LogFormat("<Mission Control #{0}> Rotation: {1}", comp.moduleId, comp.Cover[num].transform.rotation);
        yield return null;
        comp.Cover[num].transform.parent = comp.transform.parent;

        /*MethodInfo mth;
        foreach (var component in mystifiedModule[num].gameObject.GetComponents<MonoBehaviour>())
            if ((mth = component.GetType().GetMethod("MysteryModuleHiding", BindingFlags.Public | BindingFlags.Instance)) != null) {
                if (mth.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(KMBombModule[]) }))
                    mth.Invoke(component, new object[] { keyModules.ToArray() });
                else if (mth.GetParameters().Length == 0)
                    mth.Invoke(component, null);
            }*/

        mystifyScale[num] = mystifiedModule[num].transform.localScale;
        mystifiedModule[num].transform.localScale = new Vector3(0, 0, 0);
        yield return null;
    }

    // Reveals a module for Wish
    private IEnumerator RevealWishModule(int num) {
        // Code from Mystery Module
        Debug.LogFormat("[Mission Control #{0}] Revealing module: {1}", comp.moduleId, mystifiedModule[num].ModuleDisplayName);

        /*MethodInfo mth;
        foreach (var component in mystifiedModule[num].gameObject.GetComponents<MonoBehaviour>())
            if ((mth = component.GetType().GetMethod("MysteryModuleRevealing", BindingFlags.Public | BindingFlags.Instance)) != null && mth.GetParameters().Length == 0)
                mth.Invoke(component, null);*/

        var duration = 2.0f;
        var elapsed = 0.0f;
        while (elapsed < duration) {
            yield return null;
            elapsed += Time.deltaTime;
            mystifiedModule[num].transform.localScale = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), mystifyScale[num], elapsed / duration);
            comp.PivotRight[num].transform.localEulerAngles = new Vector3(0.0f, 0.0f, -90.0f * elapsed / duration);
            comp.PivotLeft[num].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 90.0f * elapsed / duration);
        }
        mystifiedModule[num].transform.localScale = mystifyScale[num];
        GameObject.Destroy(comp.Cover[num]);
        yield return null;
    }

    // Button is pressed
    private void ButtonPressed() {
        comp.ButtonSelectable.AddInteractionPunch(0.5f);

        if (comp.canPressButton && !comp.moduleSolved) {
            comp.canPressButton = false;
            comp.Audio.PlaySoundAtTransform("missionControl_buttonPress", comp.transform);
            Debug.LogFormat("[Mission Control #{0}] You pressed the button. Total presses: {1}", comp.moduleId, buttonPresses + 1);

            comp.StartCoroutine(RevealWishModule(buttonPresses));
            if (WISH_THRESHOLDS[buttonPresses] > comp.Bomb.GetSolvedModuleNames().Count()) {
                comp.Audio.PlaySoundAtTransform("missionControl_badChime", comp.transform);
                comp.SetBombTime(comp.Bomb.GetTime() * (1.0f - TIME_LOSS));
            }

            else
                comp.Audio.PlaySoundAtTransform("missionControl_goodChime", comp.transform);

            buttonPresses++;
            if (buttonPresses >= WISH_THRESHOLDS.Length)
                comp.SolveModule();

            comp.canPressButton = true;
        }
    }
}
