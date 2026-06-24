using System.Collections;
using System.Linq;
using UnityEngine;

/// Author: Espik
internal class DeadEndHandler {
    private static bool deadEndSolve = false;
    private const float DEADENDSTART = 12000.0f; // 12000
    private static float finishingTime = 55.0f;
    private float iteration = 0.0f;

    internal IEnumerator ProcessDeadEndLarge(MissionControl comp) {
        while (!comp.bombSolved) {
            // Increases the timer speed
            if (comp.ZenModeActive) {
                if (Mathf.Floor(comp.Bomb.GetTime()) != iteration) {
                    iteration = Mathf.Floor(comp.Bomb.GetTime());

                    // Prevents the timer from going too fast
                    if (iteration >= 12000.0f)
                        comp.SetBombTime(comp.Bomb.GetTime() + 0.75f);

                    else
                        comp.SetBombTime(comp.Bomb.GetTime() + (Mathf.Floor(iteration / 160.0f) / 100.0f));
                }

                else {
                    if (Mathf.Floor(comp.Bomb.GetTime()) != DEADENDSTART - iteration) {
                        iteration = DEADENDSTART - Mathf.Floor(comp.Bomb.GetTime());
                        comp.SetBombTime(comp.Bomb.GetTime() - (Mathf.Floor(iteration / 160.0f) / 100.0f));
                    }
                }
            }

            // Bomb solves
            if (comp.Bomb.GetSolvedModuleNames().Count() == comp.Bomb.GetSolvableModuleNames().Count()) {
                comp.bombSolved = true;
                finishingTime = comp.Bomb.GetTime();
                deadEndSolve = true;
            }

            yield return null;
        }
    }

    internal IEnumerator ProcessDeadEndSmall(MissionControl comp) {
        comp.canPressButton = false;

        while (!comp.bombSolved) {
            // Keeps the time between 55-56 seconds
            if (comp.ZenModeActive && comp.Bomb.GetTime() > 55.95f)
                comp.SetBombTime(55.01f);

            else if (!comp.ZenModeActive && comp.Bomb.GetTime() < 55.05f)
                comp.SetBombTime(55.99f);

            // Larger bomb is solved
            if (deadEndSolve) {
                comp.bombSolved = true;
                deadEndSolve = false;
                comp.SetBombTime(finishingTime);
                comp.SolveModule();
            }

            yield return null;
        }
    }
}
