using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class InputDronesScript : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField eclipsiaInput;
    public TMP_InputField mystaraInput;
    public TMP_InputField lyrionInput;
    public TMP_InputField kronusInput;
    public TMP_InputField fioraInput;

    [Header("UI Elements")]
    public TextMeshProUGUI errorText;
    public Button submitButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Score Calculation")]
    public ScoreCalc scoreCalculator;

    [Header("Drone Animation")]
    public Canvas parentCanvas;             // Canvas, де лежить UI
    public GameObject dronePrefab;          // префаб дрона (UI Image)
    public RectTransform submitButtonRect;  // RectTransform кнопки Submit
    public RectTransform[] planetTargets;   // Kronus, Lyrion, Mystara, Eclipsia, Fiora

    [Header("Animation Settings")]
    public float droneSpeed = 500f;         // швидкість (пікс/сек)
    public float spawnInterval = 0.1f;      // затримка між спавном

    private string submitUrl = "https://0464-213-109-233-107.ngrok-free.app/submit_move.php";
    private int gameId;
    private int playerId;

    // словник для підрахунку приземлених дронів на кожній планеті
    private Dictionary<int, int> landedCount = new Dictionary<int, int>();
    // зберігаємо останні значення для кожної планети
    private int[] lastValues;

    void Start()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(ValidateDistribution);

        if (errorText != null)
            errorText.text = "Enter the number of drones for each planet.\n" +
                             "The sum of drones must be 1000\n" +
                             "and must follow the rule:\n" +
                             "Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora";

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void ValidateDistribution()
    {
        // звук кліку
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // парсимо і перевіряємо
        if (!TryParseInputs(
                out int eclipsia, out int mystara,
                out int lyrion, out int kronus, out int fiora))
        {
            DisplayError("Please enter valid numbers between 0 and 1000.");
            return;
        }

        int sum = eclipsia + mystara + lyrion + kronus + fiora;
        if (sum != 1000)
        {
            DisplayError($"The total must be 1000. Current total: {sum}");
            return;
        }

        if (kronus < lyrion || lyrion < mystara || mystara < eclipsia || eclipsia < fiora)
        {
            DisplayError("Distribution must follow: Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora");
            return;
        }

        DisplaySuccess("Distribution successful!");

        // готуємо для сабміту
        gameId = SessionData.gameId;
        playerId = SessionData.playerId;

        // запускаємо сабміт та анімацію дронів через корутину
        int[] values = new int[] { kronus, lyrion, mystara, eclipsia, fiora };
        StartCoroutine(SubmitAnimateAndCheck(playerId, gameId, values));

        submitButton.interactable = false;
    }

    private IEnumerator SubmitAnimateAndCheck(int playerId, int gameId, int[] values)
    {
        // сабміт на сервер
        yield return StartCoroutine(SubmitMove(playerId, gameId,
            values[0], values[1], values[2], values[3], values[4]));

        // анімація дронів
        yield return StartCoroutine(SpawnAndFlyDrones(values));

        // починаємо чекати результати після анімації
        if (scoreCalculator != null && gameId != -1)
            scoreCalculator.BeginCheckingMoves();
        else
            Debug.Log("ScoreCalc script not assigned in Inspector!");
    }

    #region Parsing & UI-feedback

    private bool TryParseInputs(
        out int eclipsia, out int mystara,
        out int lyrion, out int kronus,
        out int fiora)
    {
        eclipsia = mystara = lyrion = kronus = fiora = 0;
        return TryParseField(eclipsiaInput, out eclipsia)
            && TryParseField(mystaraInput, out mystara)
            && TryParseField(lyrionInput, out lyrion)
            && TryParseField(kronusInput, out kronus)
            && TryParseField(fioraInput, out fiora);
    }

    private bool TryParseField(TMP_InputField fld, out int value)
    {
        value = 0;
        return fld != null && int.TryParse(fld.text, out value)
               && value >= 0 && value <= 1000;
    }

    private void DisplayError(string msg)
    {
        if (errorText != null)
        {
            errorText.text = msg;
            errorText.color = Color.red;
        }
        Debug.LogError(msg);
    }

    private void DisplaySuccess(string msg)
    {
        if (errorText != null)
        {
            errorText.text = msg;
            errorText.color = Color.green;
        }
        Debug.Log(msg);
    }

    #endregion

    #region Submit to Server

    private IEnumerator SubmitMove(
        int playerId, int gameId,
        int kronus, int lyrion, int mystara, int eclipsia, int fiora)
    {
        WWWForm form = new WWWForm();
        form.AddField("game_id", gameId);
        form.AddField("player_id", playerId);
        form.AddField("kronus", kronus);
        form.AddField("lyrion", lyrion);
        form.AddField("mystara", mystara);
        form.AddField("eclipsia", eclipsia);
        form.AddField("fiora", fiora);

        using (UnityWebRequest www = UnityWebRequest.Post(submitUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log("Server response: " + www.downloadHandler.text);
            else
                Debug.Log("SubmitMove error: " + www.error);
        }
    }

    #endregion

    #region Drone Animation

    private IEnumerator SpawnAndFlyDrones(int[] values)
    {
        // зберігаємо значення для обчислення розміщення
        lastValues = values;
        // очищуємо лічильник приземлених дронів
        landedCount.Clear();

        List<IEnumerator> flyCoroutines = new List<IEnumerator>();

        for (int i = 0; i < values.Length; i++)
        {
            landedCount[i] = 0;  // ініціалізуємо лічильник для планети i
            // округляємо до найближчого цілого: 149->1, 150->2
            int count = Mathf.RoundToInt(values[i] / 100f);
            for (int j = 0; j < count; j++)
            {
                // 1) Інстансим префаб
                GameObject go = Instantiate(dronePrefab, parentCanvas.transform);
                RectTransform dr = go.GetComponent<RectTransform>();
                // 2) Ставимо його на позицію кнопки
                dr.position = submitButtonRect.position;
                // 3) Політ та посадка біля планети
                Vector3 target = planetTargets[i].position;
                var moveRoutine = MoveDroneSettle(dr, i, target);
                flyCoroutines.Add(moveRoutine);
                StartCoroutine(moveRoutine);
                // 4) Трохи зачекаємо перед наступним
                yield return new WaitForSecondsRealtime(spawnInterval);
            }
        }

        // чекаємо завершення всіх рухів
        foreach (var routine in flyCoroutines)
            yield return StartCoroutine(routine);
    }

    private IEnumerator MoveDroneSettle(RectTransform dr, int planetIndex, Vector3 target)
    {
        Vector3 start = dr.position;
        float dist = Vector3.Distance(start, target);
        float t = 0f;
        float duration = dist / droneSpeed;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;  // без урахування timeScale
            dr.position = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }

        // обчислюємо порядковий номер приземлення
        int landed = landedCount[planetIndex];
        landedCount[planetIndex] = landed + 1;
        // загальна кількість дронів для планети
        int totalForPlanet = Mathf.RoundToInt(lastValues[planetIndex] / 100f);
        // кут для розміщення по колу
        float angleDeg = 360f * landed / totalForPlanet;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float settleRadius = 100f;  // відстань від центру планети

        Vector2 offset = new Vector2(
            Mathf.Cos(angleRad),
            Mathf.Sin(angleRad)
        ) * settleRadius;

        // фінальна позиція над планетою
        dr.position = target + (Vector3)offset;
    }

    #endregion
}
