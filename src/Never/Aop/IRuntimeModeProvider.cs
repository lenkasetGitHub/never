﻿namespace Never.Aop
{
    /// <summary>
    /// 运行方式提供者，不同环境应该有不同的运行方式，所有标准应该为：提供的值与当前相同（提供值为空则默认为相同）则工作，否则应视为不工作
    /// </summary>
    public interface IRuntimeModeProvider
    {
        /// <summary>
        /// 当前环境的运行方式
        /// </summary>
        IRuntimeMode[] RuntimeModeArray { get; }
    }
}