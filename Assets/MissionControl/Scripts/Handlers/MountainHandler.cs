using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// Author: Blananas2
internal class MountainHandler : Handler {
    private bool goldenPresent = false;
    private bool goldenActive = false;
    private int solveCount;

    internal MountainHandler(MissionControl comp) {
        this.comp = comp;
    }

    internal IEnumerator ProcessTheMountain() {
        comp.ButtonTransform.localEulerAngles = new Vector3(0f, 0f, 270f);
        goldenPresent = true;
        comp.StartCoroutine(AnimateGolden());

        comp.ButtonSelectable.OnInteract = delegate () {
            ButtonPressed();
            return false;
        };

        while (true) {
            solveCount = comp.Bomb.GetSolvedModuleNames().Count;

            if (solveCount == 1 && !goldenActive) {
                Debug.Log(comp.Bomb.GetSolvedModuleIDs()[0]);
                if (comp.Bomb.GetSolvedModuleIDs()[0] == "MissionControl") {
                    goldenActive = true;
                    comp.SetBombTime(comp.Bomb.GetTime() + 3600f);
                }
                else {
                    goldenPresent = false;
                    comp.GoldenSlot.sprite = null;
                    comp.ButtonTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
                }
            }
            if (solveCount == comp.Bomb.GetSolvableModuleIDs().Count() && goldenActive && goldenPresent) {
                goldenPresent = false;
                comp.StartCoroutine(GoldenCollect());
            }
            if (comp.Bomb.GetStrikes() > 0 && goldenActive) {
                Debug.LogFormat("[Mission Control #{0}] Struck with the golden strawberry. Detonating bomb.", comp.moduleId);
                comp.StartCoroutine(comp.DetonateBomb());
            }
            if (comp.Bomb.GetTime() < 3600f && goldenActive) {
                Debug.LogFormat("[Mission Control #{0}] Ran out of time with the golden strawberry. Detonating bomb.", comp.moduleId);
                comp.StartCoroutine(comp.DetonateBomb());
            }

            yield return null;
        }
    }

    //Animates the golden strawberry for The Mountain
    private IEnumerator AnimateGolden() {
        int goldenFrame = 0;
        while (goldenPresent) {
            yield return new WaitForSeconds(0.066f);
            goldenFrame = (goldenFrame + 1) % 6;
            comp.GoldenSlot.sprite = comp.GoldenSprites[goldenFrame];
            comp.GoldenSlot.transform.localPosition = new Vector3(0f, 0.03f, (float) Math.Sin(comp.Bomb.GetTime() % 6.2831853f) * 0.005f - 0.0025f);
            yield return null;
        }
    }

    private IEnumerator GoldenCollect() {
        for (int g = 6; g < 20; g++) {
            yield return new WaitForSeconds(g != 15 ? 0.09f : 0.6f);
            comp.GoldenSlot.sprite = comp.GoldenSprites[g];
            if (g == 13) {
                comp.StartCoroutine(GoldenFlash());
            }
            yield return null;
        }
        comp.GoldenSlot.sprite = null;
    }

    private IEnumerator GoldenFlash() {
        bool flashBool = false;
        while (true) {
            flashBool = !flashBool;
            comp.GoldenSlot.color = new Color(1f, flashBool ? 1f : 0.69f, 0.69f);
            yield return new WaitForSeconds(0.06f);
        }
    }

    // Button is pressed
    private void ButtonPressed() {
        comp.ButtonSelectable.AddInteractionPunch(0.5f);
        comp.SolveModule();
    }
}
