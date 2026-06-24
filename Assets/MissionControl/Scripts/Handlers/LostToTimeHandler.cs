using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Author: VFlyer
internal class LostToTimeHandler : Handler {
    internal LostToTimeHandler(MissionControl comp) {
        this.comp = comp;
    }

    internal IEnumerator ProcessLostToTime() {
        while (!comp.activated)
            yield return null;

        if (comp.ZenModeActive || comp.TimeModeActive) {
            comp.ButtonText.text = "FATAL ERROR";
            comp.StartCoroutine(comp.FlickerTextRoutine());
            yield break;
        }
        ;
        /*
         * This entire section is denoted to be similar to how Time Mode handles scoring for each of the modules in this mission.
         * The major exception is due to fact that Time Mode has more precision when it comes to scoring.
         */
        var solvedModIDs = comp.Bomb.GetSolvedModuleIDs();
        var timeGainModTiersAll = new Dictionary<int, IEnumerable<string>>() {
            { 0, new[]
            {   "MissionControl", "AnagramsModule", "whoOF", "BigButton", "Numpath",
                "NotMemory", "LightBulbs", "modulo", "cruelDigitalRootModule" } },
            { 1, new[]
            {   "gemDivision", "fourOperands", "colorCycleButton", "Password", "diffusion",
                "EncryptedDice", "daylightDirections", "DoubleOhModule", "LabelPrioritiesModule", "ColourFlash",
                "nonverbalSimon", "VCRCS", "booleanVennModule", "theRule", "YellowButtonModule", "factoryCubes" } },
            { 2, new[]
            {   "ColoredSwitchesModule", "SetModule", "triamonds", "ChordQualities", "yellowArrowsModule",
                "artPricing", "MysticSquareModule", "YahtzeeModule", "binaryTango", "masyuModule" } },
            { 3, new[]
            {   "sqlCruel", "digisibility", "loopover", "spillingPaint",
                "klaxon", "shikaku", "simonSelectsModule", "squeeze",
                "TheHypercubeModule", "MahjongQuizHard"} },
            { 4, new[]
            {   "KudosudokuModule", "unfairsRevenge", "WalkingCubeModule", "TripleTraversalModule",
                "notX01", "violetCipher", "synesthesia", "buttonGrid", "coralCipher",
                "notreDameCipher", "memoryPoker", "AzureButtonModule", "SouvenirModule", "soulscream" } },

        };
        //Debug.LogFormat("<Mission Control #{0}> DEBUG: Tier Distributions:\n{1}", moduleId,timeGainModTiersAll.Select(a => string.Format("[{0}: {1}]", a.Key, a.Value.Join(", "))).Join("\n"));
        while (!comp.bombSolved && comp.isActiveAndEnabled) {
            var curSolves = comp.Bomb.GetSolvedModuleIDs();
            foreach (string modName in solvedModIDs)
                curSolves.Remove(modName);
            if (curSolves.Any()) {
                var totalTimeToGain = 0f;
                var timeGainsPerTier = new[] { 22.5f, 45f, 90f, 180f, 360f };
                var timeMultipliersPerStrike = new[] { 1f, 1f, 0.5f, 0.5f, 0.25f, 0f };
                //Debug.LogFormat("<Mission Control #{0}> DEBUG MODS SOLVED AT {2} REMAINING: {1}", moduleId, curSolves.Join(), Bomb.GetFormattedTime());
                foreach (string nextSolve in curSolves) {
                    var lowestTierObtained = timeGainModTiersAll.Keys.FirstOrDefault(a => timeGainModTiersAll[a].Contains(nextSolve));
                    //Debug.LogFormat("<Mission Control #{0}> DEBUG: {1} considered as tier {2}.", moduleId, nextSolve, lowestTierObtained + 1);
                    totalTimeToGain += timeGainsPerTier[lowestTierObtained];
                }
                var timePenalty = Mathf.Max(0f, comp.Bomb.GetStrikes() >= timeMultipliersPerStrike.Length ? timeMultipliersPerStrike.Last() : timeMultipliersPerStrike[comp.Bomb.GetStrikes()]);
                totalTimeToGain *= timePenalty;
                comp.SetBombTime(comp.Bomb.GetTime() + totalTimeToGain);
                solvedModIDs.AddRange(curSolves);
            }
            TimerRate.SetFromModule(comp.Module, Mathf.Max(1f, 1f + comp.Bomb.GetStrikes() - 4));
            yield return null;
            comp.bombSolved = comp.Bomb.GetSolvedModuleIDs().Count >= comp.Bomb.GetSolvableModuleIDs().Count;
        }
    }
}