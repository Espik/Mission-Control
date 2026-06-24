using KModkit;
using System.Collections;
using System.Linq;
using UnityEngine;

/// Author: Espik
internal class ForNoEyesOnlyHandler {
    private const int BLINDMODS = 47; // 47

    private MissionControl comp;

    internal IEnumerator ProcessForNoEyesOnly(MissionControl comp, bool blackScreen) {
        this.comp = comp;

        comp.ButtonSelectable.OnInteract = delegate () {
            ButtonPressed();
            return false;
        };

        if (blackScreen) {
            comp.SetBlackScreen();

            while (comp.Bomb.GetSolvedModuleNames().Count() < BLINDMODS)
                yield return null;

            comp.StartCoroutine(comp.FadeOutBlack(20.0f));
        }

        yield return null;
    }

    // Reading the serial number
    private IEnumerator ReadSerialNumber() {
        string serialNumber = comp.Bomb.GetSerialNumber();
        yield return new WaitForSeconds(4.0f);

        comp.ButtonBigText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        for (int i = 0; i < serialNumber.Length; i++) {
            comp.ButtonBigText.text = serialNumber[i].ToString();
            switch (serialNumber[i]) {
                case 'Z': comp.Audio.PlaySoundAtTransform("missionControl_-26", comp.transform); break;
                case 'Y': comp.Audio.PlaySoundAtTransform("missionControl_-25", comp.transform); break;
                case 'X': comp.Audio.PlaySoundAtTransform("missionControl_-24", comp.transform); break;
                case 'W': comp.Audio.PlaySoundAtTransform("missionControl_-23", comp.transform); break;
                case 'V': comp.Audio.PlaySoundAtTransform("missionControl_-22", comp.transform); break;
                case 'U': comp.Audio.PlaySoundAtTransform("missionControl_-21", comp.transform); break;
                case 'T': comp.Audio.PlaySoundAtTransform("missionControl_-20", comp.transform); break;
                case 'S': comp.Audio.PlaySoundAtTransform("missionControl_-19", comp.transform); break;
                case 'R': comp.Audio.PlaySoundAtTransform("missionControl_-18", comp.transform); break;
                case 'Q': comp.Audio.PlaySoundAtTransform("missionControl_-17", comp.transform); break;
                case 'P': comp.Audio.PlaySoundAtTransform("missionControl_-16", comp.transform); break;
                case 'O': comp.Audio.PlaySoundAtTransform("missionControl_-15", comp.transform); break;
                case 'N': comp.Audio.PlaySoundAtTransform("missionControl_-14", comp.transform); break;
                case 'M': comp.Audio.PlaySoundAtTransform("missionControl_-13", comp.transform); break;
                case 'L': comp.Audio.PlaySoundAtTransform("missionControl_-12", comp.transform); break;
                case 'K': comp.Audio.PlaySoundAtTransform("missionControl_-11", comp.transform); break;
                case 'J': comp.Audio.PlaySoundAtTransform("missionControl_-10", comp.transform); break;
                case 'I': comp.Audio.PlaySoundAtTransform("missionControl_-09", comp.transform); break;
                case 'H': comp.Audio.PlaySoundAtTransform("missionControl_-08", comp.transform); break;
                case 'G': comp.Audio.PlaySoundAtTransform("missionControl_-07", comp.transform); break;
                case 'F': comp.Audio.PlaySoundAtTransform("missionControl_-06", comp.transform); break;
                case 'E': comp.Audio.PlaySoundAtTransform("missionControl_-05", comp.transform); break;
                case 'D': comp.Audio.PlaySoundAtTransform("missionControl_-04", comp.transform); break;
                case 'C': comp.Audio.PlaySoundAtTransform("missionControl_-03", comp.transform); break;
                case 'B': comp.Audio.PlaySoundAtTransform("missionControl_-02", comp.transform); break;
                case 'A': comp.Audio.PlaySoundAtTransform("missionControl_-01", comp.transform); break;
                case '9': comp.Audio.PlaySoundAtTransform("missionControl_-36", comp.transform); break;
                case '8': comp.Audio.PlaySoundAtTransform("missionControl_-35", comp.transform); break;
                case '7': comp.Audio.PlaySoundAtTransform("missionControl_-34", comp.transform); break;
                case '6': comp.Audio.PlaySoundAtTransform("missionControl_-33", comp.transform); break;
                case '5': comp.Audio.PlaySoundAtTransform("missionControl_-32", comp.transform); break;
                case '4': comp.Audio.PlaySoundAtTransform("missionControl_-31", comp.transform); break;
                case '3': comp.Audio.PlaySoundAtTransform("missionControl_-30", comp.transform); break;
                case '2': comp.Audio.PlaySoundAtTransform("missionControl_-29", comp.transform); break;
                case '1': comp.Audio.PlaySoundAtTransform("missionControl_-28", comp.transform); break;
                default: comp.Audio.PlaySoundAtTransform("missionControl_-27", comp.transform); break;
            }

            yield return new WaitForSeconds(2.0f);
        }

        comp.ButtonBigText.text = "";
        yield return new WaitForSeconds(2.0f);
        comp.SolveModule();
        comp.canPressButton = true;
    }

    // Button is pressed
    private void ButtonPressed() {
        comp.ButtonSelectable.AddInteractionPunch(0.5f);

        if (comp.canPressButton) {
            comp.canPressButton = false;
            comp.Audio.PlaySoundAtTransform("missionControl_buttonPress", comp.transform);
            comp.Audio.PlaySoundAtTransform("missionControl_edgeworkRead", comp.transform);
            Debug.LogFormat("[Mission Control #{0}] You pressed the button. Reading the serial number now.", comp.moduleId);
            comp.StartCoroutine(ReadSerialNumber());
        }
    }
}
