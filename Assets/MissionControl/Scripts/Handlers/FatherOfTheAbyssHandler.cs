using System.Collections;
using System.Linq;

/// Author: Espik, Commissioner: Danielstigman
internal class FatherOfTheAbyssHandler {
    private float abyssTime = 12000.0f;

    internal IEnumerator ProcessTheFatherOfTheAbyss(MissionControl comp) {
        abyssTime = comp.Bomb.GetTime();

        while (!comp.activated)
            yield return null;

        comp.StartCoroutine(comp.FadeInBlack(10.0f, true, 1.0f / abyssTime));

        while (comp.Bomb.GetSolvedModuleNames().Count() < comp.Bomb.GetModuleNames().Count()) {
            yield return null;
        }

        comp.StartCoroutine(comp.FadeOutBlack(10.0f));
    }
}
