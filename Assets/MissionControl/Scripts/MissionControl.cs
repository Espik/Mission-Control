using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Wawa.DDL;

public class MissionControl : MonoBehaviour {
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    public KMSelectable ButtonSelectable;
    public TextMesh ButtonText;
    public TextMesh ButtonBigText;
    public Transform ButtonTransform;

    public MeshRenderer ModuleBorder;
    public MeshRenderer ModuleButton;

    // From Mystery Module
    public GameObject[] Cover;
    public GameObject[] PivotRight;
    public GameObject[] PivotLeft;

    public Material PlanetMaterial;
    public Material BorderMaterial;
    public Material VignetteMaterial;

    public SpriteRenderer GoldenSlot;
    public Sprite[] GoldenSprites;

    public Material[] StageBorderMaterials;
    public Material[] StageButtonMaterials;

    // Module info
    private static int moduleIdCounter = 1;
    internal int moduleId;
    internal bool moduleSolved = false;
    internal bool activated = false;

    private string mission;
    private bool missionFound = false;
    private bool isUndefined = false;

    internal bool canPressButton = true;
    private static bool canPlayIntro = true;

    internal bool flickerText = false;
    internal float transparency = 0.0f;
    internal float rotationSpeed = 0.05f;

    internal readonly float BORDER_GREEN = 0.6640625f;
    internal readonly float BUTTON_GREEN = 0.75f;
    internal readonly float BUTTON_BLUE = 0.5f;

    internal bool bombSolved = false;
    internal bool freezeTimer = false;

    private CameraPostProcess postProcess = null;
    private Transform cameraPos = null;

    private int mode = 0;
    /* 1: Dead End (Big)
     * 2: Dead End (Small)
     * 3: Disconnected
     * 4: Wish
     * 5: Precise Instability
     * 6: For No Eyes Only
     * 7: Lost To Time
     * 8: Flyer's Manual Curse / Flyer's Alternative Manual Curse
     * 9: The Father of the Abyss
     * 10: The Mountain / The Mountain B-Side
     * 11: Command Prompt
     * 12: Defrost
     */

    // Module handlers
    internal DeadEndHandler DeadEnd;
    internal DisconnectedHandler Disconnected;
    internal WishHandler Wish;
    internal PreciseInstabilityHandler PreciseInstability;
    internal ForNoEyesOnlyHandler ForNoEyesOnly;
    internal LostToTimeHandler LostToTime;
    internal FlyersManualCurseHandler FlyersManualCurse;
    internal FatherOfTheAbyssHandler TheFatherOfTheAbyss;
    internal MountainHandler TheMountain;
    internal CommandPromptHandler CommandPrompt;
    internal DefrostHandler Defrost;

    // Mod settings
    private MissionControlSettings Settings;
    sealed class MissionControlSettings {
        public bool IntroSound = true;
    }


    // Ran as bomb loads
    private void Awake() {
        moduleId = moduleIdCounter++;

        // Module Settings
        var modConfig = new ModConfig<MissionControlSettings>("MissionControl");
        Settings = modConfig.Settings;
        modConfig.Settings = Settings;

        ButtonSelectable.OnInteract += delegate () { ButtonPressed(); return false; };
        Bomb.OnBombExploded += delegate () { OnDestroy(); };

        Module.OnActivate += OnActivate;

        cameraPos = Camera.main.transform;
    }

    // Gets information
    private void Start() {
        StartCoroutine(AnimateButton());
        mission = GetMission();
        Debug.LogFormat("<Mission Control #{0}> Mission: {1}", moduleId, mission);

        switch (mission) {
            case "undefined":
                isUndefined = true;
                break;

            case "mod_dead_end_deadend": // Dead End
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Dead End\".", moduleId);
                missionFound = true;
                mode = Bomb.GetSolvableModuleNames().Count() == 1 ? 2 : 1;

                if (mode == 1)
                    StartCoroutine(DeadEnd.ProcessDeadEndLarge(this));

                if (mode == 2)
                    StartCoroutine(DeadEnd.ProcessDeadEndSmall(this));

                break;

            case "mod_ktane_EspikHardMissions_disconnected": // Disconnected
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Disconnected\".", moduleId);
                missionFound = true;
                mode = 3;
                StartCoroutine(Disconnected.ProcessDisconnected(this));
                break;

            case "mod_ktane_EspikHardMissions_wish": // Wish
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Wish\".", moduleId);
                missionFound = true;
                mode = 4;
                StartCoroutine(Wish.ProcessWish(this));
                break;

            case "mod_jamMissions_Espik": // Precise Instability
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Precise Instability\".", moduleId);
                missionFound = true;
                mode = 5;
                StartCoroutine(PreciseInstability.ProcessPreciseInstability(this));
                break;

            case "mod_blindfoldMissions_blindBomb": // For No Eyes Only
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"For No Eyes Only\".", moduleId);
                missionFound = true;
                mode = 6;
                StartCoroutine(ForNoEyesOnly.ProcessForNoEyesOnly(this, true));
                break;

            case "mod_blindfoldMissions_blindBombTest": // For No Eyes Only [Practice]
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"For No Eyes Only [Practice]\".", moduleId);
                missionFound = true;
                mode = 6;
                StartCoroutine(ForNoEyesOnly.ProcessForNoEyesOnly(this, false));
                break;

            case "mod_missionpack_VFlyer_missionTimeConstraint": // Lost To Time
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Lost To Time\".", moduleId);
                missionFound = true;
                mode = 7;
                StartCoroutine(LostToTime.ProcessLostToTime(this));
                break;

            case "mod_missionpack_VFlyer_missionModuleCorruption": // Flyer's Manual Curse
            case "mod_missionpack_VFlyer_missionModuleCorruptionALT": // Flyer's Alterative Manual Curse
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Flyer's Manual Curse\". Mission ran can be an ALT version.", moduleId);
                missionFound = true;
                mode = 8;
                StartCoroutine(FlyersManualCurse.ProcessFlyersManualCurse(this));
                break;

            case "mod_DansPissionMack_redacted": // The Father of the Abyss
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"The Father of the Abyss\".", moduleId);
                missionFound = true;
                mode = 9;
                StartCoroutine(TheFatherOfTheAbyss.ProcessTheFatherOfTheAbyss(this));
                break;

            case "mod_theBombsBlanMade_mountain": //The Mountain
            case "mod_theBombsBlanMade_mountainBside": //The Mountain B-Side
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"The Mountain{1}\".", moduleId, mission.Contains("Bside") ? " B-Side" : "");
                missionFound = true;
                mode = 10;
                StartCoroutine(TheMountain.ProcessTheMountain(this));
                break;

            case "mod_eXishMissions_cmdprompt": // Command Prompt
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Command Prompt\".", moduleId);
                missionFound = true;
                mode = 11;
                StartCoroutine(CommandPrompt.ProcessCommandPrompt(this));
                break;

            case "mod_espikStellarMissions_defrost": // Defrost
                Debug.LogFormat("[Mission Control #{0}] Found mission: \"Defrost\".", moduleId);
                missionFound = true;
                mode = 12;
                StartCoroutine(Defrost.ProcessDefrost(this));
                break;
        }
    }

    // Lights turn on
    private void OnActivate() {
        activated = true;

        if (missionFound) {
            if (canPlayIntro && Settings.IntroSound) {
                canPlayIntro = false;
                Audio.PlaySoundAtTransform("missionControl_inEffect", transform);
            }
        }

        else {
            flickerText = true;

            if (isUndefined) {
                Debug.LogFormat("[Mission Control #{0}] Unable to detect missions.", moduleId);
                ButtonText.text = "FATAL\nERROR";
            }

            else {
                Debug.LogFormat("[Mission Control #{0}] No mission found.", moduleId);
                ButtonText.text = "ERROR";
            }

            StartCoroutine(FlickerTextRoutine());
        }
    }

    // Removes gimmick effects when the the bomb isn't active
    private void OnDestroy() {
        canPlayIntro = true;
        BorderMaterial.color = new Color(1.0f, BORDER_GREEN, 0.0f);
        PlanetMaterial.color = new Color(1.0f, BUTTON_GREEN, BUTTON_BLUE);

        if (postProcess != null) {
            postProcess.Vignette = 0.0f;
            postProcess.Grayscale = 0.0f;
            DestroyImmediate(postProcess);
            postProcess = null;
        }

        CommandPrompt.CloseDictationEngine();
    }

    // Gets the mission - code by S.
    private string GetMission() {
        try {
            Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
            Type type = gameplayState.GetType();
            FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
            return fieldMission.GetValue(gameplayState).ToString();
        }

        catch (NullReferenceException) {
            return "undefined";
        }
    }

    // Rotates the button so the texture animates
    private IEnumerator AnimateButton() {
        while (true) {
            ButtonTransform.Rotate(new Vector3(rotationSpeed, 0.0f, 0.0f));
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Button is pressed
    private void ButtonPressed() {
        ButtonSelectable.AddInteractionPunch(0.5f);

        // Unmodified rules
        if (canPressButton && !moduleSolved) {
            Audio.PlaySoundAtTransform("missionControl_buttonPress", transform);
            Debug.LogFormat("[Mission Control #{0}] Button pressed at {1}.", moduleId, Bomb.GetFormattedTime());

            if (Bomb.GetSerialNumberNumbers().Sum() == Math.Floor(Bomb.GetTime()) % 60)
                SolveModule();

            else
                StrikeModule();
        }
    }

    /// <summary>
    /// Helper functions for handlers
    /// </summary>

    // Detonates the bomb
    internal IEnumerator DetonateBomb() {
        if (ZenModeActive)
            Module.HandleStrike();

        while (!ZenModeActive) {
            Module.HandleStrike();
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Fades in the vignette - code from Art Appreciation
    internal IEnumerator FadeIn(float speed = 0.67f) {
        if (postProcess != null) {
            DestroyImmediate(postProcess);
        }

        postProcess = cameraPos.gameObject.AddComponent<CameraPostProcess>();
        postProcess.PostProcessMaterial = new Material(VignetteMaterial);

        for (float progress = 0.0f; progress < 1.0f; progress += Time.deltaTime * speed) {
            postProcess.Vignette = progress * 1.6f;
            postProcess.Grayscale = progress * 0.35f;

            yield return null;
        }

        postProcess.Vignette = 1.6f;
        postProcess.Grayscale = 0.35f;
    }

    // Fades in a black screen
    internal IEnumerator FadeInBlack(float amplifier, bool exponential, float speed = 1.0f) {
        if (postProcess != null) {
            DestroyImmediate(postProcess);
        }

        postProcess = cameraPos.gameObject.AddComponent<CameraPostProcess>();
        postProcess.PostProcessMaterial = new Material(VignetteMaterial);

        if (exponential) {
            for (float progress = 0.0f; progress < 1.0f; progress += Time.deltaTime * speed) {
                postProcess.Vignette = (float) (Math.Pow(progress, 2) * amplifier);

                yield return null;
            }
        }

        else {
            for (float progress = 0.0f; progress < 1.0f; progress += Time.deltaTime * speed) {
                postProcess.Vignette = progress * amplifier;

                yield return null;
            }
        }

        postProcess.Vignette = amplifier;
    }

    // Fades out the vignette - code from Art Appreciation
    internal IEnumerator FadeOut(float speed = 0.67f) {
        for (float progress = 1.0f - Time.deltaTime * speed; progress >= 0.0f; progress -= Time.deltaTime * speed) {
            postProcess.Vignette = progress * 1.6f;
            postProcess.Grayscale = progress * 0.35f;

            yield return null;
        }

        if (postProcess != null) {
            DestroyImmediate(postProcess);
            postProcess = null;
        }
    }

    // Fades out a black screen
    internal IEnumerator FadeOutBlack(float amplifier, float speed = 1.0f) {
        for (float progress = 1.0f - Time.deltaTime * speed; progress >= 0.0f; progress -= Time.deltaTime * speed) {
            postProcess.Vignette = progress * amplifier;

            yield return null;
        }

        if (postProcess != null) {
            DestroyImmediate(postProcess);
            postProcess = null;
        }
    }

    // Makes the text flicker on the button
    internal IEnumerator FlickerTextRoutine() {
        while (flickerText) {
            transparency += 0.01f;
            transparency %= 2.0f;

            if (transparency > 1.0f) { // Down
                ButtonText.color = new Color(1.0f, 1.0f, 1.0f, 2.0f - transparency);
                ButtonBigText.color = new Color(1.0f, 1.0f, 1.0f, 2.0f - transparency);
            }

            else { // Up
                ButtonText.color = new Color(1.0f, 1.0f, 1.0f, transparency);
                ButtonBigText.color = new Color(1.0f, 1.0f, 1.0f, transparency);
            }

            yield return new WaitForSeconds(0.02f);
        }

        ButtonText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        ButtonBigText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    // Freezes the bomb's timer
    internal IEnumerator FreezeTimer() {
        freezeTimer = true;
        float frozenTime = Bomb.GetTime();

        while (freezeTimer) {
            TimeRemaining.FromModule(Module, frozenTime);
            yield return new WaitForSeconds(0.02f);
        }

        yield return null;
    }

    // Removes a strike from the bomb - code by Emik (currently bugged)
    internal void RemoveStrike() {
        var bomb = GetComponentInParent<KMBomb>();
        int strikes = bomb.GetStrikes();
        strikes -= 1;
        bomb.SetStrikes(strikes);
    }

    // Makes the screen completely black
    internal void SetBlackScreen() {
        if (postProcess != null) {
            DestroyImmediate(postProcess);
        }

        postProcess = cameraPos.gameObject.AddComponent<CameraPostProcess>();
        postProcess.PostProcessMaterial = new Material(VignetteMaterial);

        postProcess.Vignette = 10000.0f;
    }

    // Sets the bomb's timer to a specified time
    internal void SetBombTime(float time) {
        TimeRemaining.FromModule(Module, time);
    }

    // Solves the module
    internal void SolveModule() {
        if (!moduleSolved) {
            Debug.LogFormat("[Mission Control #{0}] Module solved!", moduleId);
            moduleSolved = true;
            Module.HandlePass();
        }
    }

    // Adds a strike
    private void StrikeModule() {
        Debug.LogFormat("[Mission Control #{0}] Strike!", moduleId);
        Module.HandleStrike();
    }


    // Variables set by Tweaks for Zen/Time mode detection
    #pragma warning disable 414
    internal bool ZenModeActive;
    internal bool TimeModeActive;
    private bool TwitchPlaysActive;
    #pragma warning restore 414

    // Twitch Plays command handler - by eXish
    #pragma warning disable 414
    internal readonly string TwitchHelpMessage = @"!{0} press (##) [Presses the button (optionally when the seconds digits of the bomb's timer are '##')] | !{0} countdown <1-20> [Presses the button when the countdown timer is the specified number on Precise Instability]";
    #pragma warning restore 414
    internal IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*countdown\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 1)
                yield return "sendtochaterror Please specify when to press the button!";
            else if (parameters.Length > 2)
                yield return "sendtochaterror Too many parameters!";
            else
            {
                int time = -1;
                if (!int.TryParse(parameters[1], out time))
                {
                    yield return "sendtochaterror!f The specified number '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (time < 1 || time > 20)
                {
                    yield return "sendtochaterror The specified number '" + parameters[1] + "' is out of range 1-20!";
                    yield break;
                }
                if (!PreciseInstability.franticMode)
                {
                    yield return "sendtochaterror The countdown timer is not currently active!";
                    yield break;
                }
                yield return null;
                while (time != PreciseInstability.displayedSecond) yield return "trycancel Halted waiting to press the button due to a cancel request.";
                ButtonSelectable.OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 1)
            {
                if (!canPressButton)
                {
                    yield return "sendtochaterror The button cannot be pressed right now!";
                    yield break;
                }
                yield return null;
                ButtonSelectable.OnInteract();
            }
            else if (parameters.Length > 2)
                yield return "sendtochaterror Too many parameters!";
            else
            {
                int time = -1;
                if (!int.TryParse(parameters[1], out time))
                {
                    yield return "sendtochaterror!f The specified seconds digits '" + parameters[1] + "' are invalid!";
                    yield break;
                }
                if (time < 0 || time > 59)
                {
                    yield return "sendtochaterror The specified seconds digits '" + parameters[1] + "' are invalid!";
                    yield break;
                }
                if (parameters[1].Length < 2)
                {
                    yield return "sendtochaterror The specified seconds digits '" + parameters[1] + "' are invalid!";
                    yield break;
                }
                if (!canPressButton)
                {
                    yield return "sendtochaterror The button cannot be pressed right now!";
                    yield break;
                }
                yield return null;
                while (time != Math.Floor(Bomb.GetTime()) % 60) yield return "trycancel Halted waiting to press the button due to a cancel request.";
                ButtonSelectable.OnInteract();
            }
        }
    }

    // Twitch Plays autosolver - by eXish
    IEnumerator TwitchHandleForcedSolve()
    {
        switch (mode)
        {
            case 0:
            case 1:
            case 3:
            case 7:
            case 8:
            case 9:
            case 11:
                while (Bomb.GetSerialNumberNumbers().Sum() != Math.Floor(Bomb.GetTime()) % 60) yield return true;
                ButtonSelectable.OnInteract();
                break;
            case 2:
                while (!moduleSolved) yield return true;
                break;
            case 4:
                while (!moduleSolved)
                {
                    if (Wish.WISH_THRESHOLDS[Wish.buttonPresses] > Bomb.GetSolvedModuleNames().Count())
                        yield return true;
                    else
                    {
                        ButtonSelectable.OnInteract();
                        yield return new WaitForSeconds(.1f);
                    }
                }
                break;
            case 5:
                while (PreciseInstability.franticMode || Bomb.GetSerialNumberNumbers().Sum() != Math.Floor(Bomb.GetTime()) % 60) yield return true;
                ButtonSelectable.OnInteract();
                break;
            case 6:
                if (canPressButton)
                    ButtonSelectable.OnInteract();
                while (!moduleSolved) yield return true;
                break;
            case 10:
                ButtonSelectable.OnInteract();
                yield return new WaitForSeconds(.1f);
                break;
        }
    }
}