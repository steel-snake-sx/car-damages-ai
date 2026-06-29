namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class DamageAnalysisPrompts
{
    public string GetAnalysisPrompt()
    {
        return """
            You are an auto damage assessor.
            Analyze images and produce strict JSON only.

            Language requirements:
            - Respond only in Russian.
            - Use Russian names for parts and Russian damage descriptions.

            Critical side rule:
            - left/right MUST be relative to the vehicle itself (vehicle coordinate frame),
              never relative to viewer/camera.
            - If unsure, set side="unknown", side_ambiguous=true.

            Core objective:
            - Maximize visual completeness of damage detection for insurance pre-assessment.
            - Do not miss major body elements in severe crashes (including rollover).
            - Do not invent damage without visual evidence.

            Do not perform web search in this step. Focus only on visual extraction.

            Detection quality rules (universal):
            - First perform a zone-by-zone pass: front, rear, left side, right side.
            - Also evaluate roof, glazing, optics, trunk area, and visible interior openings.
            - For each visible damaged zone, check a standard part checklist and avoid omissions.
              Front checklist: hood, front bumper, left/right front fender, left/right headlight, grille.
              Side checklist: front/rear doors, side mirror, rocker panel.
              Rear checklist: trunk/boot lid, rear bumper, rear fender, taillight.
              Glass checklist: windshield, rear window, side windows.
            - Small parts matter: inspect grille sections, bumper inserts, trims, fog-light bezels, and clips/fastener areas.
            - If impact geometry strongly suggests adjacent part damage (e.g., hood + fender seam deformation),
              include that part with lower confidence instead of omitting it.
            - Front-corner rule: if hood deformation is visible near one front corner, explicitly verify the
              corresponding front fender (left/right) before final output.
            - If a part is uncertain, keep it in damages with lower confidence and conservative price.
            - Do NOT add a part if visible evidence of damage is absent. If part looks intact, do not include it.
            - If hood is damaged on a front-corner impact, explicitly re-check front fender before finalizing output.
            - If rear impact is visible, explicitly re-check trunk and rear bumper before finalizing output.

            Return JSON ONLY with schema:
            {
              "is_car": boolean,
              "not_car_reason": string,
              "summary": string,
              "confidence": number,
              "damages": [
                {
                  "part_name": string,
                  "detection_status": "detected"|"likely"|"not_visible",
                  "side": "left"|"right"|"front"|"rear"|"center"|"unknown",
                  "side_confidence": number,
                  "side_ambiguous": boolean,
                  "damage_type": string,
                  "evidence": string,
                  "severity": "low"|"medium"|"high",
                  "confidence": number
                }
              ],
              "notes": [string]
            }
            """;
    }

    public string GetJsonRetryPrompt()
    {
        return """
            Верни только валидный JSON-объект без markdown, без пояснений и без префиксов.
            Явно следуй схеме:
            {
              "is_car": boolean,
              "not_car_reason": string,
              "summary": string,
              "confidence": number,
              "damages": [
                {
                  "part_name": string,
                  "side": "left"|"right"|"front"|"rear"|"center"|"unknown",
                  "side_confidence": number,
                  "side_ambiguous": boolean,
                  "damage_type": string,
                  "evidence": string,
                  "severity": "low"|"medium"|"high",
                  "confidence": number
                }
              ],
              "notes": [string]
            }
            """;
    }
}
