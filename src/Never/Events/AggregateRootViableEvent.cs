﻿using System;

namespace Never.Events
{
    /// <summary>
    /// 聚合根启用事件
    /// </summary>
    /// <typeparam name="TAggregateRootId">聚合根唯一标识对象的类型</typeparam>
    [Serializable, Never.EventStreams.IgnoreStoreEventAttribute, Never.Attributes.IgnoreAnalyse]
    public class AggregateRootViableEvent<TAggregateRootId> : IAggregateRootEvent<TAggregateRootId>
    {
        #region property

        /// <summary>
        /// 唯一标识
        /// </summary>
        public TAggregateRootId AggregateId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 编辑时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 编辑者
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public int Version { get; set; }

        #endregion property

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootViableEvent{TAggregateRootId}"/> class.
        /// </summary>
        public AggregateRootViableEvent()
            : this(default(TAggregateRootId), "sys")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootViableEvent{TAggregateRootId}"/> class.
        /// </summary>
        /// <param name="uniqueId">The unique identifier.</param>
        /// <param name="creator">The creator.</param>
        protected AggregateRootViableEvent(TAggregateRootId uniqueId, string creator)
        {
            this.AggregateId = uniqueId;
            this.Version = 0;
            this.CreateDate = DateTime.Now;
            this.Creator = string.IsNullOrEmpty(creator) ? "sys" : creator;
        }

        #endregion ctor
    }
}