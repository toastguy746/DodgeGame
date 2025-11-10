using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public Text pressAnyKeyText; // 깜빡일 텍스트
    private bool isVisible = true;

    void Start()
    {
        if (pressAnyKeyText == null)
            pressAnyKeyText = GetComponent<Text>();

        // 1초마다 텍스트 투명도 전환
        InvokeRepeating(nameof(ToggleAlpha), 0f, 0.25f);
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    void ToggleAlpha()
    {
        isVisible = !isVisible;
        Color c = pressAnyKeyText.color;
        c.a = isVisible ? 1f : 0f;
        pressAnyKeyText.color = c;
    }
}
