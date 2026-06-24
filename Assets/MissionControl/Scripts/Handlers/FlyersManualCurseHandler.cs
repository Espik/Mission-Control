using System.Collections;
using UnityEngine;

/// Author: VFlyer
internal class FlyersManualCurseHandler {
    private int solveCount;
    private int toleratedStrikeLimit;

    internal IEnumerator ProcessFlyersManualCurse(MissionControl comp) {
        while (!comp.activated)
            yield return null;

        comp.flickerText = true;
        comp.StartCoroutine(comp.FlickerTextRoutine());

        while (true) {
            solveCount = comp.Bomb.GetSolvedModuleNames().Count;
            toleratedStrikeLimit = solveCount / 5 + 1;

            if (toleratedStrikeLimit < 8)
                comp.ButtonText.text = string.Format("{0} / {1}\n{2}", solveCount, toleratedStrikeLimit * 5, toleratedStrikeLimit.ToString("0x"));
            else
                comp.ButtonText.text = "MAX REACHED\n8x";
            if (comp.Bomb.GetStrikes() >= toleratedStrikeLimit && !(comp.ZenModeActive || comp.TimeModeActive)) {
                Debug.LogFormat("[Mission Control #{0}] Say goodbye to that attempt. At {1}, you solved {2} module(s) and struck {3} time(s). This mission cannot tolerate that many strikes in this state.", comp.moduleId, comp.Bomb.GetFormattedTime(), comp.Bomb.GetSolvedModuleNames().Count, comp.Bomb.GetStrikes());
                comp.DetonateBomb();
            }

            yield return null;
        }
    }
}
