# Unity 2D 经典输入系统实战：从方块操控到商业级手感（14 关完整指南）

> 本文档为基于 Unity 经典输入系统（`UnityEngine.Input`）的 2D 方块控制实践指南。由浅入深，兼顾基础输入生命周期、物理结合、空间感知与商业级手感优化，可作为个人学习路线、技术博客大纲或视频教程脚本。

---

## 零、 项目环境与工程规范

### 1. 基础环境搭建
* **主角方块（Player）**：
  * `SpriteRenderer`（纯色方形 Sprite）
  * `BoxCollider2D`
  * `Rigidbody2D`（`Collision Detection: Continuous`，锁定 Z 轴旋转 `Freeze Rotation Z`）
  * 挂载对应关卡的控制器脚本（如 `Task01_BasicMovement.cs`）
* **地面（Ground）**：
  * `BoxCollider2D`（静态物体，放置在主角下方）
* **相机（Main Camera）**：
  * 正交相机（Orthographic），Size 设置为 5，视野覆盖活动区域。

### 2. 多场景工程结构规范（隔离归档）
每个任务独立保存在一个场景中，避免参数与物理设置互相污染，随时可单独打开查看与演示：

```text
Assets/
├── Docs/
│   └── InputManager_Practice_Levels.md // 本规划文档
├── Scenes/
│   └── Tasks/
│       ├── Task01_HeartbeatSquare.unity
│       ├── Task02_DiagonalNormalization.unity
│       ├── Task03_AxisSmoothing.unity
│       ├── Task04_PhysicsJumpDecouple.unity
│       ├── Task05_VariableJumpGravity.unity
│       ├── Task06_DoubleJump.unity
│       ├── Task07_ChargeSlime.unity
│       ├── Task08_DoubleTapDash.unity
│       ├── Task09_MouseAimShoot.unity
│       ├── Task10_DragAndThrow.unity
│       ├── Task11_InteractTrigger.unity
│       ├── Task12_CelesteFeel.unity
│       ├── Task13_TimeStopPause.unity
│       └── Task14_KeyRebinding.unity
└── Scripts/
    └── Tasks/
        ├── Task01_HeartbeatSquare.cs
        ├── Task02_DiagonalNormalization.cs
        └── ...
```

---

## 第一幕：掌控身体（直觉与手感基础）

### 关卡 1：心跳方块（按键三态与即时反馈）
* **玩法表现**：
  * 按下空格（Down）瞬间变红，尺寸瞬间变大到 1.2 倍；
  * 持续按住（Hold）保持红色；
  * 松开空格（Up）瞬间变白，尺寸在 Update 中平滑回弹回 1.0 原尺寸。
* **核心 API**：
  * `Input.GetKeyDown(KeyCode.Space)`
  * `Input.GetKey(KeyCode.Space)`
  * `Input.GetKeyUp(KeyCode.Space)`
  * `Vector3.Lerp` 平滑插值
* **踩坑与核心认知**：
  * 明确区分按键采样的生命周期：单帧触发 vs 持续检测 vs 抬起事件。
  * 避免一开始就引入繁重的 Animator，仅用纯代码（修改 `color` 与 `localScale`）即可完成打击感（Juice）反馈。

---

### 关卡 2：拒绝在对角线起飞（速度归一化）
* **玩法表现**：
  * 用 `W/A/S/D` 控制方块在 2D 平面做四向移动；
  * 提供一个按键切换模式：
    * 模式 A（直觉写法）：斜向同时按 W 和 D 时，移动速度超速 $\sqrt{2} \approx 1.414$ 倍；
    * 模式 B（专业写法）：斜向移动经过向量归一化，严格保持恒定速度。
* **核心 API**：
  * `Vector2.Normalize()` 或 `vector.normalized`
  * `transform.Translate()` 与 `Time.deltaTime`
* **踩坑与核心认知**：
  * 独立游戏新手最常见的物理位移 Bug 之一：未归一化的输入向量会导致斜向跑速度激增。

---

### 关卡 3：冰面漂移 vs 街机响应（输入轴平滑）
* **玩法表现**：
  * 按数字键 1 体验“冰面滑行手感”；按数字键 2 体验“像素街机急停手感”。
* **核心 API**：
  * `Input.GetAxis("Horizontal")`（带平滑重力曲线，松开按键后值逐步归零）
  * `Input.GetAxisRaw("Horizontal")`（瞬时返回 -1, 0, 1）
  * Unity 编辑器：`Edit -> Project Settings -> Input Manager`（观察 `Sensitivity`、`Gravity`、`Dead` 参数）
* **踩坑与核心认知**：
  * 平台跳跃或动作游戏一般推荐 `GetAxisRaw`，因为玩家要求输入绝对跟手、无多余滑步。

---

## 第二幕：重力与动量（平台跳跃的秘密）

### 关卡 4：拯救吃键（物理跳跃与生命周期解耦）
* **玩法表现**：
  * 方块站在地面上，按空格键触发起跳；在空中不能连跳；
  * 做对比演示：展示在 `FixedUpdate` 采样输入导致的“按了空格却跳不起来”的偶发吞键 Bug。
* **核心 API**：
  * `Rigidbody2D.linearVelocity`（或老版本 `velocity`）/ `AddForce()`
  * `Physics2D.OverlapCircle` 或 `Physics2D.BoxCast` 地面检测
* **踩坑与核心认知（经典面试题）**：
  * **输入与物理分工范式**：必须在 `Update`（每渲染帧调用，不漏按键）中捕获按键标志位 `jumpRequested = true`；在 `FixedUpdate`（物理帧）中消费标志位并施加冲量，消费后立即重置为 `false`。

---

### 关卡 5：像马里奥一样起跳（变高跳跃与下落重力倍率）
* **玩法表现**：
  * 短按空格轻灵小跳，长按空格冲天大跳；
  * 起跳上升阶段重力适中，下落阶段（或提前松开按键）重力增大 2~3 倍，下坠极快且干脆，消除在月球上漫步般的漂浮感（Floaty Jump）。
* **核心 API**：
  * `Rigidbody2D.gravityScale` 动态调节
  * 判断刚体垂直速度 `rb.linearVelocity.y < 0`
* **踩坑与核心认知**：
  * 现代动作手感的核心秘密之一：上升和下落不采用相同重力。

---

### 关卡 6：绝境升天（二段跳与空中翻滚）
* **玩法表现**：
  * 允许在跳起到半空中时再次按下空格触发二段跳，并伴随一次方块 360 度翻转；落地后重置可用跳跃次数。
* **核心 API**：
  * 空中跳跃计数器 `extraJumpsRemaining`
  * 落地判定（Grounded 回调重置计数）
* **踩坑与核心认知**：
  * 二段跳的起跳点必须先将当前 Y 轴垂直速度清零（`rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0)`），否则残余下落速度会抵消二段跳冲量导致跳不高。

---

### 关卡 7：蓄力史莱姆（长按压缩与弹射爆发）
* **玩法表现**：
  * 按住 S 键或 Shift 键蓄力，方块随时间被压缩（X 轴拉宽、Y 轴变扁，最大蓄力 1.5 秒）；
  * 松开按键时，按照蓄力百分比向当前移动朝向或上方产生爆发式弹射冲量。
* **核心 API**：
  * `Mathf.Clamp` 限制蓄力上限
  * 蓄力计时器累加 `Time.deltaTime`
* **踩坑与核心认知**：
  * 练习输入时长到游戏物理量（力、形变）的线性/非线性映射。

---

### 关卡 8：街机式闪现（双击 A/D 触发 Dash）
* **玩法表现**：
  * 在 0.25 秒内快速连续按下两次 A 键或 D 键，触发一次短距横向无敌突进（Dash）。
* **核心 API**：
  * 时间戳比对：`Time.time - lastPressTime < doubleTapThreshold`
  * 瞬时速度提升与协程（Coroutine）持续计时
* **踩坑与核心认知**：
  * 掌握输入时序（Input Timing）检测算法，为后续格斗搓招和复杂连击打基础。

---

## 第三幕：感知与世界（鼠标与空间互动）

### 关卡 9：枪斗术（朝向鼠标、射击与 UI 防穿透）
* **玩法表现**：
  * 方块正面始终跟随鼠标光标旋转；
  * 鼠标左键发射子弹；
  * 屏幕右上角放置一个“暂停”UI 按钮，点击 UI 按钮时**绝对不会误发射子弹**。
* **核心 API**：
  * `Input.mousePosition`
  * `Camera.main.ScreenToWorldPoint()`
  * `Mathf.Atan2(dy, dx) * Mathf.Rad2Deg`
  * `EventSystem.current.IsPointerOverGameObject()`
* **踩坑与核心认知（必踩死坑）**：
  * `ScreenToWorldPoint` 必须传入正确的相机 Z 轴深度。
  * 商业项目必须在处理鼠标/触控射击前加入 UI 穿透检测（`IsPointerOverGameObject`）。

---

### 关卡 10：重力抓取手（拖拽拾取与惯性投掷）
* **玩法表现**：
  * 鼠标移动到方块上方时高亮；按住左键可直接拖拽方块飞行；
  * 快速甩动鼠标并松开，方块根据松开瞬间的鼠标移动速度被“甩飞”出去并自然下落。
* **核心 API**：
  * `Collider2D.OverlapPoint()`
  * 拖拽状态锁（Drag Lock，防止鼠标滑出碰撞体丢焦）
  * 鼠标历史世界坐标差值计算即时释放速度：`(currentPos - prevPos) / Time.deltaTime`
* **踩坑与核心认知**：
  * 理解物理仿真与外部强制坐标干预之间的转换平滑度。

---

### 关卡 11：靠近我，按下 E（范围触发与互动气泡）
* **玩法表现**：
  * 场景中放置一个宝箱或交互物；
  * 当方块走入宝箱周围的检测区域时，宝箱头顶冒出动态“按 E 交互”提示；离开后消失；
  * 在范围内按下 E 键，宝箱开启并爆出金币。
* **核心 API**：
  * `Collider2D.isTrigger` 触发器
  * `OnTriggerEnter2D` / `OnTriggerExit2D`
  * 在 Trigger 范围内开启输入监听状态开关 `canInteract`
* **踩坑与核心认知**：
  * 避免无论走到哪里都能触发全局互动的低级错误，严格实现状态机与物理触发范围绑定。

---

## 第四幕：神作手感与系统架构（工业级实战）

### 关卡 12：蔚蓝（Celeste）的手感魔法（土狼时间 + 输入预缓冲）
* **玩法表现**：
  1. **输入预缓冲（Jump Buffering）**：在距离地面还有 2 个身位时提前按下跳跃，落地的一瞬间自动起跳，不会吞操作；
  2. **土狼时间（Coyote Time）**：走出平台悬空后的 0.15 秒之内按下跳跃，依然能成功起跳。
* **核心 API**：
  * `jumpBufferCounter`（按下空格时重置为 0.15 秒，每帧递减）
  * `coyoteTimeCounter`（只要在地面就持续重置为 0.15 秒，离开地面后每帧递减）
* **踩坑与核心认知**：
  * 真正区分“业余 Demo”与“商业级手感”的王牌机制。手感差的游戏往往因为过于较真物理判定，而好游戏会通过缓冲宽容玩家的反应时间。

---

### 关卡 13：砸瓦鲁多！时间暂停与输入阻断
* **玩法表现**：
  * 按 `ESC` 切换暂停状态；游戏物理定格，屏幕置灰，玩家所有的移动和跳跃按键失效；再按一次恢复。
* **核心 API**：
  * `Time.timeScale = 0f`
  * 状态拦截：`if (isPaused) return;`
  * `Input.GetKeyDown` 依然不受 `timeScale` 影响（它是根据真实时钟采样的）
* **踩坑与核心认知**：
  * 暂停菜单自身如果需要动画，必须使用 `Time.unscaledDeltaTime`。

---

### 关卡 14：黑客改键（运行时自定义按键 Rebinding）
* **玩法表现**：
  * 点击“跳跃键绑定”按钮，提示“请按下任意键”；
  * 玩家按下键盘上的任意按键（如从 Space 改成 J），立即完成改键绑定并持久化保存；下次启动自动读取。
* **核心 API**：
  * `Input.anyKeyDown`
  * 枚举遍历或 `Event.current.keyCode` 捕获
  * `PlayerPrefs.SetString()` / `PlayerPrefs.GetString()`
* **踩坑与核心认知**：
  * 旧版 Input Manager 的 Axis 无法在运行时直接修改配置文件，商业成熟方案是在业务层抽象一套 `Dictionary<Action, KeyCode>` 或使用 `PlayerPrefs` 做软重定向。

---

## 附录：内容输出与转化建议

1. **写技术文章（知乎 / 掘金 / Indienova / Dev.to）**：
   * 采用“翻车现场（反面教材） vs 优雅重构（正面教材）”排版；
   * 每个机制配上 3~5 秒高帧率清晰动图（如：斜向超速对比、土狼时间开启对比）。
2. **做技术视频（Bilibili / YouTube / 抖音短视频）**：
   * 必须锁定 60 FPS 录制（OBS）；
   * 使用 **Keyviz** 工具在画面角落实时显示按键回显；
   * 在 Scene 视图中用 `OnDrawGizmos` 把脚底检测圆形、土狼时间倒计时可视化展示。
3. **可交互体验（终极杀手锏）**：
   * 使用 Unity 的 WebGL 导出功能，将各关卡做成网页版挂在 GitHub Pages 或 Itch.io 上，让读者在浏览器里直接按键体验手感差异。

