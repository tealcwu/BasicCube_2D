using UnityEngine;

public class KeyVisualizer : MonoBehaviour
{
    [Header("回显设置")]
    [Tooltip("是否开启按键回显（可在 Inspector 随时勾选/取消）")]
    [SerializeField] private bool isEnabled = true;

    [Tooltip("按键抬起后的淡出时间（秒）")]
    [SerializeField] private float fadeDuration = 0.6f;

    private string activeKeyText = "";     // 当前显示的按键名称
    private float displayTimer = 0f;       // 淡出计时器
    private bool isHolding = false;        // 是否处于长按状态

    void Update()
    {
        if (!isEnabled) return;

        // 捕获常见教学按键（可根据需要继续扩展）
        CheckKey(KeyCode.Space, "SPACE");
        CheckKey(KeyCode.Tab, "Tab");
        CheckKey(KeyCode.W, "W");
        CheckKey(KeyCode.A, "A");
        CheckKey(KeyCode.S, "S");
        CheckKey(KeyCode.D, "D");
        CheckKey(KeyCode.LeftShift, "Shift");
        CheckKey(KeyCode.E, "E");

        // 更新淡出时钟
        if (isHolding)
        {
            displayTimer = fadeDuration; // 按住期间常驻
        }
        else if (displayTimer > 0f)
        {
            displayTimer -= Time.deltaTime; // 抬起后平滑衰减
        }
    }

    private void CheckKey(KeyCode key, string displayName)
    {
        if (Input.GetKeyDown(key))
        {
            activeKeyText = displayName;
            isHolding = true;
            displayTimer = fadeDuration;
        }
        else if (Input.GetKeyUp(key) && activeKeyText == displayName)
        {
            isHolding = false;
        }
    }

    void OnGUI()
    {
        if (!isEnabled || displayTimer <= 0f || string.IsNullOrEmpty(activeKeyText)) return;

        // 1. 计算淡出透明度
        float alpha = Mathf.Clamp01(displayTimer / (fadeDuration * 0.5f));
        Color prevColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);

        // 2. 右下角自适应定位
        float boxWidth = 200f;
        float boxHeight = 80f;
        float posX = Screen.width - boxWidth - 25f;
        float posY = Screen.height - boxHeight - 25f;

        // 3. 绘制半透明黑色底板
        GUI.Box(new Rect(posX, posY, boxWidth, boxHeight), "");

        // 4. 居中绘制醒目的按键名称
        GUIStyle keyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        keyStyle.normal.textColor = Color.gold;
        GUI.Label(new Rect(posX, posY, boxWidth, boxHeight), activeKeyText, keyStyle);

        // 恢复全局颜色
        GUI.color = prevColor;
    }
}