using UnityEngine;

public class Task01_HeartbeatSquare : MonoBehaviour, ITeachingTask
{
    [Header("教学对比开关")]
    [Tooltip("开启：使用带有 Lerp 弹性回弹与颜色瞬态的商业手感；关闭：新手直接缩放无过渡")]
    [SerializeField] private bool useCorrectApproach = false;

    [Header("手感参数设置")]
    [SerializeField] private float targetScaleMultiplier = 1.2f; // 按下时放大的倍率
    [SerializeField] private float returnSpeed = 8f; // 平滑回弹速度

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Vector3 targetScale;

    public string TaskTitle => "关卡1：心跳方块（按键三态即时反馈）";

    public bool UseCorrectApproach
    {
        get => useCorrectApproach;
        set => useCorrectApproach = value;
    }

    public string InCorrectModeDescription => "瞬时粗暴缩放，僵硬无弹性";

    public string CorrectModeDescription => "瞬发脉冲 + Lerp 平滑衰减回弹";   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        spriteRenderer.color = Color.white; // 初始颜色为白色
    }

    // Update is called once per frame
    void Update()
    {
        if(!useCorrectApproach)
        {
            // 新手直接缩放无过渡
            if (Input.GetKey(KeyCode.Space))
            {
                targetScale = originalScale * targetScaleMultiplier;
                transform.localScale = targetScale;
                spriteRenderer.color = Color.red; // 按下时变红
            }
            else
            {
                transform.localScale = originalScale;
                spriteRenderer.color = Color.white; // 松开时恢复白色
            }
        }
        else
        {
            // 商业手感：带有 Lerp 弹性回弹与颜色瞬态
            if (Input.GetKeyDown(KeyCode.Space))
            {
                targetScale = originalScale * targetScaleMultiplier;
                transform.localScale = targetScale;
                spriteRenderer.color = Color.red; // 按下时变红
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                targetScale = originalScale;
                spriteRenderer.color = Color.white; // 松开时恢复白色
            }

            // 平滑回弹到目标缩放
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * returnSpeed);
        }
    }
}
