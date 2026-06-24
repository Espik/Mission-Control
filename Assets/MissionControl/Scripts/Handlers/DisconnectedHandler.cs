using System.Collections;
using System.Linq;
using UnityEngine;

/// Author: Espik
internal class DisconnectedHandler {
    private float currentSecond = 0.0f;
    private float strikeModifier = 20.0f;

    internal IEnumerator ProcessDisconnected(MissionControl comp) {
        while (comp.Bomb.GetSolvedModuleNames().Count() < 53)
            yield return null;

        while (comp.Bomb.GetSolvedModuleNames().Count() == 53) {
            if (Mathf.Floor(comp.Bomb.GetTime()) != currentSecond) {
                switch (comp.Bomb.GetStrikes()) {
                    case 0: strikeModifier = 20.0f; break;
                    case 1: strikeModifier = 19.0f; break;
                    case 2: strikeModifier = 18.0f; break;
                    case 3: strikeModifier = 17.0f; break;
                    default: strikeModifier = 16.0f; break;
                }

                currentSecond = Mathf.Floor(comp.Bomb.GetTime());

                if (comp.ZenModeActive)
                    comp.SetBombTime(comp.Bomb.GetTime() + strikeModifier / 24.0f);

                else
                    comp.SetBombTime(comp.Bomb.GetTime() - strikeModifier / 24.0f);
            }

            yield return null;
        } 
    }
}
