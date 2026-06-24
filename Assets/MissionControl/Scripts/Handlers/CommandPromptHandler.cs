using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

/// Author: eXish
public class CommandPromptHandler {
    public GameObject fakeCubeSel;
    public GameObject processingLED;
    public GameObject textBacking;
    public Image textBackingImg;
    public Text overlayText;
    public Color textBackingColor;
    private DictationRecognizer dictationRecognizer;
    private Dictionary<string, string> modIDToScript = new Dictionary<string, string>()
    {
        { "KeypadV2", "AdvancedKeypad" },
        { "spwiz3DMaze", "ThreeDMazeModule" },
        { "SimonScreamsModule", "SimonScreamsModule" },
        { "CheapCheckoutModule", "CheapCheckoutModule" },
        { "YahtzeeModule", "YahtzeeModule" },
        { "visual_impairment", "VisualImpairment" },
        { "wire", "wireScript" },
        { "TheDigitModule", "TheDigitScript" },
        { "krazyTalk", "krazyTalkScript" },
        { "calcModule", "calcModuleScript" },
        { "PrimeChecker", "PrimeCheckerScript" },
        { "bootTooBig", "bootTooBigScript" },
        { "KritHoldUps", "HoldUpsScript" },
        { "polygons", "polygons" },
        { "Negativity", "NegativityScript" },
        { "Jailbreak", "Jailbreak" },
        { "colorNumbers", "colorNumberCode" },
        { "GSPentabutton", "PentabuttonScript" },
        { "BaybayinWords", "BaybayinWords" },
        { "symbolicCoordinates", "symbolicCoordinatesScript" },
        { "doubleScreenModule", "DoubleScreenScript" },
        { "notCrazyTalk", "NCTScript" },
        { "Words", "Words" },
        { "insaIlo", "InsaIloScript" },
        { "quizbowl", "QuizbowlScript" },
        { "shogiIdentification", "ShogiIdentificationScript" },
        { "PurchasingProperties", "PurchasingPropertiesGameplay" },
        { "reverseMorse", "reverseMorseScript" },
        { "InnerConnectionsModule", "InnerConnectionsScript" },
        { "invisymbol", "InvisymbolScript" },
        { "MaroonButtonModule", "MaroonButtonScript" },
        { "RedButtonModule", "RedButtonScript" },
        { "GrayButtonModule", "GrayButtonScript" },
        { "presidentialElections", "presidentialElectionsScript" },
        { "USACycle", "USACycle" },
        { "handTurkey", "handTurkey" },
        { "xelWhackTheCops", "WhackTheCops" },
        { "doofenshmirtzEvilIncModule", "DoofenshmirtzEvilIncScript" },
        { "Patterns", "Patterns" },
        { "tripleTermModule", "TripleTermScript" },
        { "jobApplication", "JobApplicationScript" },
        { "HaikuModule", "HaikuScript" },
        { "surveySays", "SurveySays" },
        { "BattleshipModule", "BattleshipModule" },
        { "whiteout", "whiteoutScript" },
        { "HexiEvilFMN", "EvilMemory" }
    };
    private Dictionary<string, string> modIDToAssembly = new Dictionary<string, string>()
    {
        { "KeypadV2", "HexiAdvancedBaseModules" },
        { "spwiz3DMaze", "3DMaze" },
        { "SimonScreamsModule", "SimonScreams" },
        { "CheapCheckoutModule", "CheapCheckoutModule" },
        { "YahtzeeModule", "Yahtzee" },
        { "visual_impairment", "visual_impairment" },
        { "wire", "wire" },
        { "TheDigitModule", "TheDigitModule" },
        { "krazyTalk", "krazyTalk" },
        { "calcModule", "calcModule" },
        { "PrimeChecker", "PrimeChecker" },
        { "bootTooBig", "bootTooBig" },
        { "KritHoldUps", "KritHoldUps" },
        { "polygons", "polygons" },
        { "Negativity", "Negativity" },
        { "Jailbreak", "TriviaMurderPartyPack" },
        { "colorNumbers", "colorNumbers" },
        { "GSPentabutton", "GSPentabutton" },
        { "BaybayinWords", "BaybayinWords" },
        { "symbolicCoordinates", "symbolicCoordinates" },
        { "doubleScreenModule", "doubleScreenModule" },
        { "notCrazyTalk", "notMods" },
        { "Words", "TriviaMurderPartyPack" },
        { "insaIlo", "insaIlo" },
        { "quizbowl", "quizbowl" },
        { "shogiIdentification", "shogiIdentification" },
        { "PurchasingProperties", "PurchasingProperties" },
        { "reverseMorse", "reverseMorse" },
        { "InnerConnectionsModule", "InnerConnections" },
        { "invisymbol", "invisymbol" },
        { "MaroonButtonModule", "BunchOfButtonsPack" },
        { "RedButtonModule", "BunchOfButtonsPack" },
        { "GrayButtonModule", "BunchOfButtonsPack" },
        { "presidentialElections", "presidentialElections" },
        { "USACycle", "USACycle" },
        { "handTurkey", "handTurkey" },
        { "xelWhackTheCops", "xelWhackTheCops" },
        { "doofenshmirtzEvilIncModule", "doofenshmirtzEvilIncModule" },
        { "Patterns", "TriviaMurderPartyPack" },
        { "tripleTermModule", "familiarFacesModules" },
        { "jobApplication", "jobApplication" },
        { "HaikuModule", "HaikuModule" },
        { "surveySays", "surveySays" },
        { "BattleshipModule", "Battleship" },
        { "whiteout", "whiteout" },
        { "HexiEvilFMN", "HexiCruelFMN" }
    };
    private GameObject selectedModule = null;
    private Coroutine displayText = null;
    private string[] reservedWords = { " DASH ", " DOT ", " PLUS ", " MINUS ", " ZERO ", " ONE ", " TWO ", " THREE ", " FOUR ", " FIVE ", " SIX ", " SEVEN ", " EIGHT ", " NINE ", " NOTHING ", " SPACE " };
    private string[] reservedWordsReplacements = { " - ", " . ", " + ", " - ", " 0 ", " 1 ", " 2 ", " 3 ", " 4 ", " 5 ", " 6 ", " 7 ", " 8 ", " 9 ", "", " " };
    private bool processingCmd;

    private MissionControl comp;

    internal IEnumerator ProcessCommandPrompt(MissionControl comp) {
        this.comp = comp;

        while (!comp.activated)
            yield return null;

        InitCmdPrompt();

        while (true) {
            if (processingCmd)
                processingLED.SetActive(true);
            else
                processingLED.SetActive(false);
            if (overlayText.text != "")
                textBacking.SetActive(true);
            else {
                textBacking.SetActive(false);
                textBackingImg.color = textBackingColor;
            }

            yield return null;
        }
    }

    // Sets up everything for Command Prompt
    private void InitCmdPrompt() {
        for (int i = 0; i < comp.transform.parent.childCount; i++) {
            Transform componentTransform = comp.transform.parent.GetChild(i);
            KMBombModule bombModule = componentTransform.GetComponent<KMBombModule>();
            if (bombModule != null) {
                GameObject cube = GameObject.Instantiate(fakeCubeSel, componentTransform);
                cube.transform.localPosition = componentTransform.localPosition;
                cube.transform.localEulerAngles = componentTransform.localEulerAngles;
                KMSelectable modSel = componentTransform.GetComponent<KMSelectable>();
                KMSelectable cubeSel = cube.GetComponent<KMSelectable>();
                var parentFace = componentTransform.GetComponent(ReflectionHelper.FindGameType("Selectable")).GetValue<object>("Parent");
                cubeSel.Parent = modSel;
                modSel.Children = new KMSelectable[] { cubeSel };
                modSel.ChildRowLength = 1;
                modSel.UpdateChildrenProperly();
                componentTransform.GetComponent(ReflectionHelper.FindGameType("Selectable")).SetValue("Parent", parentFace);
                if (bombModule.ModuleType == "Jailbreak") {
                    modSel.OnFocus = delegate () {
                        selectedModule = componentTransform.gameObject;
                    };
                    modSel.OnDefocus = delegate () {
                        selectedModule = null;
                    };
                    componentTransform.gameObject.GetComponent(ReflectionHelper.FindType(modIDToScript[bombModule.ModuleType], modIDToAssembly[bombModule.ModuleType])).SetValue("Focused", false);
                }
                else {
                    modSel.OnFocus += delegate () {
                        selectedModule = componentTransform.gameObject;
                    };
                    modSel.OnDefocus += delegate () {
                        selectedModule = null;
                    };
                }
            }
        }
        if (SystemInfo.operatingSystem.ToLower().Contains("windows"))
            StartDictationEngine();
        else {
            Debug.LogFormat("[Mission Control #{0}] DictationRecognizer error: GET_A_WINDOWS_COMPUTER_ERROR.", comp.moduleId);
            comp.flickerText = true;
            comp.ButtonText.text = "VOICE\nERROR";
            comp.StartCoroutine(comp.FlickerTextRoutine());
        }
    }

    // Creates the voice recognition system for Command Prompt
    private void StartDictationEngine() {
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += DictationRecognizer_OnDictationResult;
        dictationRecognizer.DictationComplete += DictationRecognizer_OnDictationComplete;
        dictationRecognizer.DictationError += DictationRecognizer_OnDictationError;
        dictationRecognizer.Start();
    }

    // Destroys the voice recognition system for Command Prompt
    internal void CloseDictationEngine() {
        if (dictationRecognizer != null) {
            dictationRecognizer.DictationComplete -= DictationRecognizer_OnDictationComplete;
            dictationRecognizer.DictationResult -= DictationRecognizer_OnDictationResult;
            dictationRecognizer.DictationError -= DictationRecognizer_OnDictationError;
            if (dictationRecognizer.Status == SpeechSystemStatus.Running) {
                dictationRecognizer.Stop();
            }
            dictationRecognizer.Dispose();
        }
    }

    // Determines if the voice recognition system for Command Prompt needs a restart or throws a fatal error
    private void DictationRecognizer_OnDictationComplete(DictationCompletionCause completionCause) {
        switch (completionCause) {
        case DictationCompletionCause.TimeoutExceeded:
        case DictationCompletionCause.PauseLimitExceeded:
        case DictationCompletionCause.Canceled:
        case DictationCompletionCause.Complete:
        // Restart required
        CloseDictationEngine();
        StartDictationEngine();
        break;
        case DictationCompletionCause.UnknownError:
        case DictationCompletionCause.AudioQualityFailure:
        case DictationCompletionCause.MicrophoneUnavailable:
        case DictationCompletionCause.NetworkFailure:
        // Fatal error
        CloseDictationEngine();
        Debug.LogFormat("[Mission Control #{0}] DictationRecognizer encountered a fatal error.", comp.moduleId);
        comp.flickerText = true;
        comp.ButtonText.text = "VOICE\nERROR";
        comp.StartCoroutine(comp.FlickerTextRoutine());
        break;
        }
    }

    // Runs whenever a command is successfully heard on Command Prompt
    private void DictationRecognizer_OnDictationResult(string text, ConfidenceLevel confidence) {
        if (selectedModule != null && !processingCmd) {
            string modID = selectedModule.GetComponent<KMBombModule>().ModuleType;
            text = " " + text.ToUpper() + " ";
            text = text.Replace(":00", "").Replace("MR.", "MR").Replace("COL.", "COL");
            if (modID == "HexiEvilFMN")
                text = text.Replace("-", "");
            for (int i = 0; i < reservedWords.Length; i++)
                text = ReplaceAllInstances(text, reservedWords[i], reservedWordsReplacements[i]);
            text = text.Trim();
            Debug.LogFormat("<Mission Control #{0}> Received command \"{1}\" for \"{2}\".", comp.moduleId, text, modID);
            if (modID == "MissionControl") {
                if (text == "HELP")
                    text = comp.TwitchHelpMessage.ToUpper().Replace("!{0} ", "").Replace(" | COUNTDOWN <1-20> [PRESSES THE BUTTON WHEN THE COUNTDOWN TIMER IS THE SPECIFIED NUMBER ON PRECISE INSTABILITY]", "");
                else
                    comp.StartCoroutine(HandleCommand(null, text, modID));
            }
            else {
                object component = selectedModule.GetComponent(ReflectionHelper.FindType(modIDToScript[modID], modIDToAssembly[modID]));
                if (text == "HELP") {
                    if (modID == "YahtzeeModule")
                        text = component.GetValue<string>("TwitchHelpMessage").ToUpper().Replace("!{0} ", "").Replace(" | DONE [SOLVE]", "");
                    else
                        text = component.GetValue<string>("TwitchHelpMessage").ToUpper().Replace("!{0} ", "");
                }
                else
                    comp.StartCoroutine(HandleCommand(component, text, modID));
            }
            if (displayText != null) {
                comp.StopCoroutine(displayText);
                overlayText.color = Color.green;
                textBackingImg.color = textBackingColor;
            }
            displayText = comp.StartCoroutine(DisplayCmdPromptText(text));
        }
    }

    // Runs whenever the voice recognition system on Command Prompt throws an error
    private void DictationRecognizer_OnDictationError(string error, int hresult) {
        Debug.LogFormat("[Mission Control #{0}] DictationRecognizer error: \"{1}\".", comp.moduleId, error);
        comp.flickerText = true;
        comp.ButtonText.text = "VOICE\nERROR";
        comp.StartCoroutine(comp.FlickerTextRoutine());
    }

    // Replaces all instances of a word for commands in Command Prompt
    private string ReplaceAllInstances(string text, string replace1, string replace2) {
        while (text.Contains(replace1))
            text = text.Replace(replace1, replace2);
        return text;
    }

    // Handles each ran command on Command Prompt, some of this code may not be necessary for this bomb but better safe than sorry
    private IEnumerator HandleCommand(object component, string text, string modID) {
        processingCmd = true;
        int strikeCt = comp.Bomb.GetStrikes();
        int solves = comp.Bomb.GetSolvedModuleNames().Count();
        IEnumerator routine = null;
        bool simple = false;
        try {
            routine = component == null ? comp.ProcessTwitchCommand(text.ToLower()) : component.CallMethod<IEnumerator>("ProcessTwitchCommand", text.ToLower());
        }
        catch (InvalidCastException) { simple = true; }
        if (simple) {
            IEnumerable<KMSelectable> btns = component.CallMethod<IEnumerable<KMSelectable>>("ProcessTwitchCommand", text.ToLower());
            foreach (KMSelectable btn in btns) {
                btn.OnInteract();
                yield return new WaitForSeconds(.1f);
                if (btn.OnInteractEnded != null)
                    btn.OnInteractEnded();

                if (strikeCt != comp.Bomb.GetStrikes() || solves != comp.Bomb.GetSolvedModuleNames().Count())
                    break;
            }
        }
        else {
            if (routine == null) {
                processingCmd = false;
                yield break;
            }
            while (true) {
                bool? moved = routine.MoveNext();
                if (moved.HasValue && !moved.Value)
                    break;

                object currentObj = routine.Current;
                if (currentObj is IEnumerable<KMSelectable>) {
                    foreach (var selectable in (IEnumerable<KMSelectable>) currentObj) {
                        selectable.OnInteract();
                        yield return new WaitForSeconds(.1f);
                        if (selectable.OnInteractEnded != null)
                            selectable.OnInteractEnded();

                        if (strikeCt != comp.Bomb.GetStrikes() || solves != comp.Bomb.GetSolvedModuleNames().Count())
                            break;
                    }
                }
                else if (currentObj is string) {
                    Match match;
                    float waitTime;
                    string currentString = (string) currentObj;
                    if (currentString.RegexMatch(@"^(sendtochaterror!h) +(\S(?:\S|\s)*)$"))
                        break;
                    else if (currentString.RegexMatch(@"^trycancel((?: (?:.|\\n)+)?)$")) {
                        yield return null;
                        continue;
                    }
                    else if (currentString.RegexMatch(out match, "^trywaitcancel ([0-9]+(?:\\.[0-9])?)((?: (?:.|\\n)+)?)$") && float.TryParse(match.Groups[1].Value, out waitTime))
                        yield return new WaitForSeconds(waitTime);
                }
                else
                    yield return currentObj;

                if (strikeCt != comp.Bomb.GetStrikes() || solves != comp.Bomb.GetSolvedModuleNames().Count())
                    break;
            }
        }
        if (strikeCt != comp.Bomb.GetStrikes() || solves != comp.Bomb.GetSolvedModuleNames().Count()) {
            if (modID == "krazyTalk" && component.GetValue<bool>("_isHolding"))
                component.SetValue("_isHolding", false);
        }
        processingCmd = false;
    }

    // Displays command text on the user's screen temporarily
    private IEnumerator DisplayCmdPromptText(string text) {
        overlayText.text = text;
        yield return new WaitForSeconds(5f);
        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime;
            overlayText.color = Color.Lerp(Color.green, Color.clear, t);
            textBackingImg.color = Color.Lerp(textBackingColor, Color.clear, t);
            yield return null;
        }
        overlayText.text = "";
        overlayText.color = Color.green;
        displayText = null;
    }
}
