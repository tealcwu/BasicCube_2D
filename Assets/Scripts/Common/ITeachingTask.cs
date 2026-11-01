using System;


public interface ITeachingTask
{
    string TaskTitle { get; }       // 关卡标题
    bool UseCorrectApproach { get; set;} // 当前对比形状状态（支持读写）
    string InCorrectModeDescription { get; } // 反面模式描述
    string CorrectModeDescription { get; } // 正面模式描述
}

