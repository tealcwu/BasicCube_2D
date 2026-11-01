using UnityEngine;

public class TeachingHUD : MonoBehaviour
{
    private ITeachingTask teachingTask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in allMonoBehaviours)
        {
            if (obj is ITeachingTask task)
            {
                teachingTask = task;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(teachingTask != null)
            {
                teachingTask.UseCorrectApproach = !teachingTask.UseCorrectApproach;
                Debug.Log($"UseCorrectApproach toggled to: {teachingTask.UseCorrectApproach}");
            }
        }
    }

    void OnGUI()
    {
        if(teachingTask == null) return;

        // 1. 垫一个半透明黑色底板
        GUI.Box(new Rect(15, 15, 500, 200), "");
        
        // 2. 关卡标题（大字号）
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(25, 20, 500, 50), teachingTask.TaskTitle, titleStyle);
        
        // 3. 当前模式（红绿区分）
        GUIStyle modeStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
        modeStyle.normal.textColor = teachingTask.UseCorrectApproach ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);
        string modeText = teachingTask.UseCorrectApproach ? "🟢 当前模式：商业级弹性手感" : "🔴 当前模式：新手反面直觉写法";
        GUI.Label(new Rect(25, 75, 500, 40), modeText, modeStyle);
        
        // 4. 手感特征描述
        GUIStyle descStyle = new GUIStyle(GUI.skin.label) { fontSize = 22 };
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        string desc = teachingTask.UseCorrectApproach ? teachingTask.CorrectModeDescription : teachingTask.InCorrectModeDescription;
        GUI.Label(new Rect(25, 120, 500, 40), $"特性：{desc}", descStyle);
        
        // 5. 操作提示小字
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 22 };
        hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(25, 160, 500, 40), "提示：按 [Tab] 键可随时一键切换对比", hintStyle);
    }
}
