# 【Unity 手感调优 #01】告别木讷！用按键三态与脉冲衰减打造心跳打击感

> **一句话导读**：彻底搞懂 `GetKeyDown / GetKey / GetKeyUp` 按键三态生命周期，无需繁重的 Animator，仅用纯代码打造具有商业级呼吸感与敲击快感的弹性方块。

---

## 1. 翻车现场（新手直觉写法）

* **现象描述**：
  很多初学者在制作方块交互或按钮反馈时，往往直接使用布尔判断：按住放大变红，松开恢复。结果画面表现极度生硬——松开按键的瞬间尺寸闪现缩回，没有丝毫缓冲与动量，看起来像是画面掉帧或抽搐。
* **新手代码（反面教材）**：
  ```csharp
  // 仅用 GetKey 进行粗暴硬切，缺乏过渡与动量
  if (Input.GetKey(KeyCode.Space))
  {
      transform.localScale = originalScale * 1.2f;
      spriteRenderer.color = Color.red;
  }
  else
  {
      transform.localScale = originalScale;
      spriteRenderer.color = Color.white;
  }
  ```
* **【动图占位】**：`![新手僵硬缩放表现](RawFootage/Task01/Task01_Incorrect.gif)`

---

## 2. 深度拆解：为什么会这样？（原理与生命周期）

### 2.1 按键采样的完整生命周期
在 Unity 经典输入系统中，一次按键交互由三个离散的阶段组成：
1. **Down（按下瞬态，仅持续 1 帧）**：`Input.GetKeyDown` 触发。这是玩家施加“力量”的起点；
2. **Hold（按住态，跨越多个渲染帧）**：`Input.GetKey` 持续返回 `true`；
3. **Up（抬起瞬态，仅持续 1 帧）**：`Input.GetKeyUp` 触发。这是外力撤销、弹性势能释放的起点。

### 2.2 动作手感的底层心法：瞬发冲击（Attack）vs 物理平滑衰减（Decay）
优秀的游戏打击感（Juice）遵循类似音频包络（ADSR）的曲线法则：
* **受击/按下（Attack）**：必须是 **0 延迟突变**。按键按下的瞬间，尺寸瞬间跳到峰值，给玩家即时的敲击正反馈；
* **恢复/松开（Decay/Release）**：必须是 **平滑插值衰减**。外力撤去后，依靠物理回弹平滑过渡回基准尺寸，赋予物体有机生命的弹性。

---

## 3. 工业级重构（商业手感解决方案）

### 3.1 核心重构代码
```csharp
using UnityEngine;

public class Task01_HeartbeatSquare : MonoBehaviour, ITeachingTask
{
    [Header("教学对比开关")]
    [Tooltip("开启：使用带有 Lerp 弹性回弹与颜色瞬态的商业手感；关闭：新手直接缩放无过渡")]
    [SerializeField] private bool useCorrectApproach = false;

    [Header("手感参数设置")]
    [SerializeField] private float targetScaleMultiplier = 1.2f; // 按下放大的峰值倍率
    [SerializeField] private float returnSpeed = 8f;             // 平滑回弹速度

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Vector3 targetScale;

    // 实现教学 HUD 接口
    public string TaskTitle => "关卡 1：心跳方块（按键三态即时反馈）";
    public bool UseCorrectApproach { get => useCorrectApproach; set => useCorrectApproach = value; }
    public string InCorrectModeDescription => "瞬时粗暴缩放 · 僵硬无弹性";
    public string CorrectModeDescription => "瞬发脉冲 + Lerp 平滑衰减回弹";

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        spriteRenderer.color = Color.white;
    }

    void Update()
    {
        if (!useCorrectApproach)
        {
            // 【反面模式】新手粗暴硬切
            if (Input.GetKey(KeyCode.Space))
            {
                targetScale = originalScale * targetScaleMultiplier;
                transform.localScale = targetScale;
                spriteRenderer.color = Color.red;
            }
            else
            {
                transform.localScale = originalScale;
                spriteRenderer.color = Color.white;
            }
        }
        else
        {
            // 【正面模式】商业级瞬发与弹性衰减
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 1. 按下瞬间爆发脉冲，零延迟直达峰值
                targetScale = originalScale * targetScaleMultiplier;
                transform.localScale = targetScale;
                spriteRenderer.color = Color.red;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                // 2. 松开时重置目标基准，颜色瞬间复位
                targetScale = originalScale;
                spriteRenderer.color = Color.white;
            }

            // 3. 在 Update 中使用 Lerp 平滑衰减
            transform.localScale = Vector3.Lerp(
                transform.localScale, 
                targetScale, 
                Time.deltaTime * returnSpeed
            );
        }
    }
}
```

### 3.2 效果对比
* 在场景中挂载 `TeachingHUD` 组件，按下键盘 **`Tab`** 键可实现运行中无缝切换对比：
  * **反面**：松手瞬间缩回，视觉抽搐；
  * **正面**：按下如同击鼓般爽快，松手呼吸般平滑回弹。
* **【动图占位】**：`![商业手感弹性回弹表现](RawFootage/Task01/Task01_Correct.gif)`

---

## 4. 商业项目进阶避坑指南（Juice & Tips）

1. **为什么不用 `Mathf.MoveTowards` 而用 `Vector3.Lerp`？**
   * `MoveTowards` 是匀速线性运动，在到达终点时会产生突兀的“急停”感；
   * `Lerp` 具有天然的指数衰减曲线（距离越近移动越慢），视觉上更贴近物理弹簧阻尼的减速过程。
2. **进阶微操：过冲回弹（Overshoot）**：
   * 追求极致打击感的游戏（如《蔚蓝》《挺进地牢》），在回弹时往往会故意跌落至 `0.95` 倍基准尺寸，再弹回 `1.0`，形成微小的“果冻果冻震颤”。

---

## 5. 源码与动手实践

* **项目开源仓库**：[GitHub / BasicCube_2D](https://github.com/)
* **关卡文件目录**：
  * 场景文件：`Assets/Scenes/Tasks/Task01_HeartbeatSquare.unity`
  * 脚本文件：`Assets/Scripts/Tasks/Task01_HeartbeatSquare.cs`

