using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// Author: Espik
internal class DefrostHandler : Handler {
    private float currentSecond = 0.0f;
    private float[] timerPercents = { 0.2f, 0.1f, 0.1f, 0.1f, 0.05f, 0.05f, 0.1f, 0.1f, 0.1f, 0.05f };
    private int defrostStage = 0;
    private float maxTime = 0.0f;

    private float[,] timeLost = { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 11.0f },
                                  { 0.0f, 63.5f, 44.103f, 33.895f, 23.321f, 662.0f / 37.0f, 14.587f, 689.0f / 64.0f, 707.0f / 82.0f, 7.25f, 7.25f},
                                  { 0.0f, 16.0f, 11.345f, 8.895f, 89.0f / 14.0f, 4.947f, 13.0f / 3.0f, 107.0f / 32.0f, 116.0f / 41.0f, 2.5f, 2.5f},
                                  { 0.0f, 9.75f, 7.034f, 5.605f, 4.125f, 3.3026f, 53.0f / 18.0f, 303.0f / 128.0f, 47.0f / 22.0f, 1.875f, 1.875f},
                                  { 0.0f, 3.0f, 31.0f / 11.0f, 8.0f / 3.0f, 17.0f / 7.0f, 2.25f, 19.0f / 9.0f, 21.0f / 11.0f, 23.0f / 13.0f, 5.0f / 3.0f, 5.0f / 3.0f} };

    internal DefrostHandler(MissionControl comp) {
        this.comp = comp;
    }

    internal IEnumerator ProcessDefrost() {
        maxTime = comp.Bomb.GetTime();
        comp.ModuleButton.material = comp.StageButtonMaterials[0];
        comp.ModuleBorder.material = comp.StageBorderMaterials[0];

        while (!comp.activated)
            yield return null;

        comp.StartCoroutine(InitDefrost());

        while (comp.Bomb.GetSolvedModuleNames().Count() < comp.Bomb.GetModuleNames().Count()) {
            if (Mathf.Floor(comp.Bomb.GetTime()) != currentSecond) {
                currentSecond = Mathf.Floor(comp.Bomb.GetTime());

                int strikes = Math.Min(4, comp.Bomb.GetStrikes());
                if (timeLost[strikes, defrostStage] != 0.0f) {
                    if (comp.ZenModeActive)
                        comp.SetBombTime(comp.Bomb.GetTime() + 1 / timeLost[strikes, defrostStage]);

                    else
                        comp.SetBombTime(comp.Bomb.GetTime() - 1 / timeLost[strikes, defrostStage]);
                }
            }
        }

        yield return null;
    }

    // Runs the timer for Defrost
    private IEnumerator InitDefrost() {
        comp.canPressButton = false;
        comp.rotationSpeed = 0;

        for (int i = 0; i < timerPercents.Length; i++) {
            yield return new WaitForSeconds(timerPercents[i] * maxTime);

            defrostStage++;
            Debug.LogFormat("[Mission Control #{0}] The module has advanced to level {1}.", comp.moduleId, defrostStage);
            comp.Audio.PlaySoundAtTransform("missionControl_stageAdvance", comp.transform);

            comp.ButtonBigText.text = defrostStage.ToString();

            if (defrostStage == 1) {
                comp.canPressButton = true;
                comp.flickerText = true;
                comp.StartCoroutine(comp.FlickerTextRoutine());
            }

            comp.ModuleButton.material = comp.StageButtonMaterials[defrostStage];
            comp.ModuleBorder.material = comp.StageBorderMaterials[defrostStage];

            comp.rotationSpeed = 0.025f * defrostStage;
        }

        comp.StartCoroutine(InitRainbow());
    }

    // Recolors the material for Defrost stage 10
    private IEnumerator InitRainbow() {
        while (true) {
            for (float i = 0.0f; i < 20.0f; i++) {
                comp.StageButtonMaterials[10].color = new Color(1.0f, 0.6f, 0.6f + 0.02f * i);
                yield return new WaitForSeconds(0.025f);
            }

            for (float i = 0.0f; i < 20.0f; i++) {
                comp.StageButtonMaterials[10].color = new Color(1.0f - 0.02f * i, 0.6f + 0.01f * i, 1.0f);
                yield return new WaitForSeconds(0.025f);
            }

            for (float i = 0.0f; i < 20.0f; i++) {
                comp.StageButtonMaterials[10].color = new Color(0.6f, 0.8f + 0.01f * i, 1.0f - 0.02f * i);
                yield return new WaitForSeconds(0.025f);
            }

            for (float i = 0.0f; i < 20.0f; i++) {
                comp.StageButtonMaterials[10].color = new Color(0.6f + 0.02f * i, 1.0f, 0.6f);
                yield return new WaitForSeconds(0.025f);
            }

            for (float i = 0.0f; i < 20.0f; i++) {
                comp.StageButtonMaterials[10].color = new Color(1.0f, 1.0f - 0.02f * i, 0.6f);
                yield return new WaitForSeconds(0.025f);
            }
        }
    }
}
