using System.Collections;
using UnityEngine;

/// Author: Espik
internal class PreciseInstabilityHandler : Handler {
    private bool acceptingStrikes = false;
    private bool readyToChange = false;
    private int actualStrikes = 0;
    private int bombStrikes = 0;
    private int storedNumber = 1;
    private float storedTime = 0.0f;

    internal bool franticMode = false;
    internal int displayedSecond = 30;
    private int enteredSecond = 0;
    private bool enteredTimerNumber = false;

    private const int JAM_STRIKE_LIMIT = 4;
    private const float JAM_BOMB_TIME = 4800.0f;

    private readonly string[] JAM_MODULES = { "3dTunnels", "AdjacentLettersModule", "atlantis", "bafflingBox", "boolMaze", "CheapCheckoutModule", "colorfulHexabuttons",
        "ColourFlash", "CrazyTalk", "cruelModulo", "cucumberModule", "decimation", "DecolourFlashModule", "digitString", "DiscoloredSquaresModule", "GlitchedButtonModule",
        "GrayButtonModule", "gyromaze", "HitmanModule", "KritHoldUps", "HumanResourcesModule", "identificationCrisis", "Indentation", "SCP2719", "jewelVault", "latinHypercube",
        "Laundry", "Lean", "letteredHexabuttons", "mazematics", "meteor", "MssngvWls", "neptune", "NotBitmapsModule", "OnlyConnectModule", "PianoParadoxModule", "poetry", "rottenBeans",
        "PasswordV2", "qSchlagDenBomb", "SetModule", "shapesAndColors", "shapeshift", "simonSelectsModule", "SimonShiftsModule", "simonStumbles", "ButtonV2", "stabilityModule",
        "timeMachine", "undertunneling", "vigenereCipher", "widdershins", "X01", "YahtzeeModule" };

    private KMBombModule[] jamModule = new KMBombModule[54];
    private Vector3[] jamModuleScale = new Vector3[54];
    private Vector3 missionControlScale;

    internal PreciseInstabilityHandler(MissionControl comp) {
        this.comp = comp;
    }

    internal IEnumerator ProcessPreciseInstability() {
        comp.StartCoroutine(HideJamModules());
        storedNumber = UnityEngine.Random.Range(1, 21);

        comp.ButtonSelectable.OnInteract = delegate () {
            ButtonPressed();
            return false;
        };

        while (!comp.activated)
            yield return null;

        comp.StartCoroutine(InitJamBomb());

        while (!comp.bombSolved) {
            if (acceptingStrikes && readyToChange && bombStrikes != comp.Bomb.GetStrikes()) {
                actualStrikes++;

                // Bomb has 4+ strikes
                if (actualStrikes >= JAM_STRIKE_LIMIT && !comp.ZenModeActive && !comp.TimeModeActive) {
                    Debug.LogFormat("[Mission Control #{0}] Strike limit reached! Detonating bomb.", comp.moduleId);
                    comp.StartCoroutine(comp.DetonateBomb());
                }

                else {
                    Debug.LogFormat("[Mission Control #{0}] Strike detected on another module! Entering countdown mode.", comp.moduleId);
                    Debug.LogFormat("[Mission Control #{0}] To remove the effects of the strike, press the button when the timer displays {1}.", comp.moduleId, storedNumber);

                    comp.StartCoroutine(comp.FreezeTimer());
                    storedTime = comp.Bomb.GetTime();

                    acceptingStrikes = false;
                    readyToChange = false;
                    comp.flickerText = false;
                    franticMode = true;
                    comp.rotationSpeed = 0.25f;
                    enteredTimerNumber = false;
                    enteredSecond = 0;

                    for (int i = 0; i < jamModule.Length; i++) {
                        if (i == jamModule.Length - 1)
                            comp.StartCoroutine(HideJamModule(i, true, true));

                        else
                            comp.StartCoroutine(HideJamModule(i, true, false));
                    }

                    comp.StartCoroutine(ChangeBorderColor(false));
                    comp.StartCoroutine(StartTimer());
                }
            }

            yield return null;
        }
    }

    // Finds and hides modules for Precise Instability
    private IEnumerator HideJamModules() {
        comp.canPressButton = false;
        int moduleCounter = 0;
        for (int i = 0; i < comp.transform.parent.childCount; i++) {
            var module = comp.transform.parent.GetChild(i).gameObject.GetComponent<KMBombModule>();
            if (module == null)
                continue;

            if (moduleCounter < jamModule.Length) {
                foreach (string name in JAM_MODULES) {
                    if (module.ModuleType == name) {
                        jamModule[moduleCounter] = module;
                        comp.StartCoroutine(HideJamModule(moduleCounter, false, false));
                        moduleCounter++;
                        break;
                    }
                }
            }
        }

        Debug.LogFormat("<Mission Control #{0}> Hiding module: Mission Control", comp.moduleId);
        missionControlScale = comp.Module.transform.localScale;
        comp.Module.transform.localScale = new Vector3(0, 0, 0);
        readyToChange = true;
        yield return null;
    }

    // Hides a module for Precise Instability
    private IEnumerator HideJamModule(int num, bool delay, bool validate) {
        yield return new WaitForSeconds(0.02f);
        Debug.LogFormat("<Mission Control #{0}> Hiding module: {1}", comp.moduleId, jamModule[num].ModuleDisplayName);

        if (!delay) {
            jamModuleScale[num] = jamModule[num].transform.localScale;
            jamModule[num].transform.localScale = new Vector3(0, 0, 0);
        }

        else {
            var duration = 0.5f;
            var elapsed = 0.0f;
            while (elapsed < duration) {
                yield return null;
                elapsed += Time.deltaTime;
                jamModule[num].transform.localScale = Vector3.Lerp(jamModuleScale[num], new Vector3(0.0f, 0.0f, 0.0f), elapsed / duration);
            }

            jamModule[num].transform.localScale = new Vector3(0, 0, 0);
        }

        if (validate)
            readyToChange = true;

        yield return null;
    }

    // Reveals a module for Precise Instability
    private IEnumerator RevealJamModule(int num, bool validate) {
        Debug.LogFormat("<Mission Control #{0}> Revealing module: {1}", comp.moduleId, jamModule[num].ModuleDisplayName);

        var duration = 2.0f;
        var elapsed = 0.0f;
        while (elapsed < duration) {
            yield return null;
            elapsed += Time.deltaTime;
            jamModule[num].transform.localScale = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), jamModuleScale[num], elapsed / duration);
        }

        jamModule[num].transform.localScale = jamModuleScale[num];

        if (validate)
            readyToChange = true;

        yield return null;
    }

    // Initiates the bomb for Precise Instability
    private IEnumerator InitJamBomb() {
        while (!readyToChange) { }
        readyToChange = false;
        comp.StartCoroutine(AnimateTimer());

        for (int i = 0; i < jamModule.Length; i++) {
            comp.StartCoroutine(RevealJamModule(i, false));
            yield return new WaitForSeconds(0.07f);
        }

        Debug.LogFormat(@"<Mission Control #{0}> Revealing module: Mission Control", comp.moduleId);
        var duration = 2.0f;
        var elapsed = 0.0f;
        while (elapsed < duration) {
            yield return null;
            elapsed += Time.deltaTime;
            comp.Module.transform.localScale = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), missionControlScale, elapsed / duration);
        }

        comp.Module.transform.localScale = missionControlScale;
        readyToChange = true;
        comp.canPressButton = true;
        yield return null;
    }

    // Animates the timer for Precise Instability
    private IEnumerator AnimateTimer() {
        float delayTime = 1.0f / 60.0f;
        float startTime = 120.0f;
        float endTime = JAM_BOMB_TIME + storedNumber;

        if (comp.ZenModeActive) {
            startTime = 0.0f;
            endTime = storedNumber;
        }

        else if (comp.TimeModeActive) {
            startTime = comp.Bomb.GetTime();
            endTime = comp.Bomb.GetTime() + storedNumber;
        }

        yield return new WaitForSeconds(2.0f);

        for (float i = 0.0f; i < 105.0f; i++) {
            comp.SetBombTime((endTime - startTime) * (i / 105.0f) + startTime);
            yield return new WaitForSeconds(delayTime);
        }

        comp.SetBombTime(endTime + 0.04f);

        comp.StartCoroutine(comp.FreezeTimer());

        yield return new WaitForSeconds(2.0f);
        comp.freezeTimer = false;
        acceptingStrikes = true;
        yield return null;
    }

    // Starts the countdown timer
    private IEnumerator StartTimer() {
        comp.Audio.PlaySoundAtTransform("missionControl_MechanismsClock", comp.transform);
        comp.ButtonBigText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        for (displayedSecond = 30; displayedSecond > 0; displayedSecond--) {
            comp.ButtonBigText.text = displayedSecond.ToString();
            yield return new WaitForSeconds(1.0f);
        }

        comp.ButtonBigText.text = "0";
        if (enteredSecond == storedNumber) {
            comp.Audio.PlaySoundAtTransform("missionControl_goodChime", comp.transform);
            Debug.LogFormat("[Mission Control #{0}] You pressed at the correct time! One strike removed!", comp.moduleId);

            comp.RemoveStrike();
        }

        else {
            comp.Audio.PlaySoundAtTransform("missionControl_badChime", comp.transform);
            Debug.LogFormat("[Mission Control #{0}] You pressed the button at the wrong time.", comp.moduleId);
            bombStrikes++;
        }

        storedNumber = ((int) storedTime) % 20;
        storedNumber = storedNumber == 0 ? 20 : storedNumber;
        Debug.LogFormat("[Mission Control #{0}] The number to press for the next countdown is {1}.", comp.moduleId, storedNumber);

        yield return new WaitForSeconds(2.0f);
        comp.freezeTimer = false;
        comp.ButtonBigText.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        comp.ButtonBigText.text = actualStrikes.ToString();
        Debug.LogFormat("[Mission Control #{0}] The bomb currently has {1} internal strikes.", comp.moduleId, actualStrikes);

        comp.flickerText = true;
        comp.transparency = 0.0f;
        comp.StartCoroutine(comp.FlickerTextRoutine());

        franticMode = false;
        comp.rotationSpeed = 0.05f;
        comp.StartCoroutine(ChangeBorderColor(true));

        while (!readyToChange) {
        }

        for (int i = 0; i < jamModule.Length; i++) {
            if (i == jamModule.Length - 1)
                comp.StartCoroutine(RevealJamModule(i, true));

            else
                comp.StartCoroutine(RevealJamModule(i, false));
        }

        yield return null;
    }

    // Changes the color of the button and border
    private IEnumerator ChangeBorderColor(bool direction) {
        if (!direction) {
            comp.StartCoroutine(comp.FadeIn());

            for (float i = 89.0f; i >= 0.0f; i--) {
                comp.BorderMaterial.color = new Color(1.0f, comp.BORDER_GREEN * (i / 90.0f), 0.0f);
                comp.PlanetMaterial.color = new Color(1.0f, comp.BUTTON_GREEN * (i / 90.0f), comp.BUTTON_BLUE * (i / 90.0f));
                yield return new WaitForSeconds(0.02f);
            }

            comp.BorderMaterial.color = new Color(1.0f, 0.0f, 0.0f);
            comp.PlanetMaterial.color = new Color(1.0f, 0.0f, 0.0f);
        }

        else {
            comp.StartCoroutine(comp.FadeOut());

            for (float i = 0.0f; i < 90.0f; i++) {
                comp.BorderMaterial.color = new Color(1.0f, comp.BORDER_GREEN * (i / 90.0f), 0.0f);
                comp.PlanetMaterial.color = new Color(1.0f, comp.BUTTON_GREEN * (i / 90.0f), comp.BUTTON_BLUE * (i / 90.0f));
                yield return new WaitForSeconds(0.02f);
            }

            comp.BorderMaterial.color = new Color(1.0f, comp.BORDER_GREEN, 0.0f);
            comp.PlanetMaterial.color = new Color(1.0f, comp.BUTTON_GREEN, comp.BUTTON_BLUE);
            acceptingStrikes = true;
        }

        yield return null;
    }

    // Button is pressed
    private void ButtonPressed() {
        comp.ButtonSelectable.AddInteractionPunch(0.5f);

        if (franticMode && !enteredTimerNumber) {
            enteredSecond = displayedSecond;
            enteredTimerNumber = true;
            comp.Audio.PlaySoundAtTransform("missionControl_buttonPress", comp.transform);
            Debug.LogFormat("[Mission Control #{0}] You pressed the button when the countdown displayed {1}.", comp.moduleId, enteredSecond);
        }

        else if (franticMode) {
            Debug.LogFormat("[Mission Control #{0}] You pressed the button again during the countdown. Only the first press is registered.", comp.moduleId);
        }
    }
}
