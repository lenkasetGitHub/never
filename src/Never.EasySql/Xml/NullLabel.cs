﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Never.EasySql.Xml
{
    /// <summary>
    /// null label
    /// </summary>
    /// <seealso cref="Never.EasySql.Xml.BaseLabel" />
    public class NullLabel : BaseLabel
    {
        /// <summary>
        /// 验证参数
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// 成功验证参数后会拼接这个字符
        /// </summary>
        public string Then { get; set; }

        /// <summary>
        /// 子标签
        /// </summary>
        public List<ILabel> Labels { get; set; }

        /// <summary>
        /// 标签类型
        /// </summary>
        /// <returns></returns>
        public override LabelType GetLabelType() => LabelType.Null;

        /// <summary>
        /// 是否找到参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public override bool ContainParameter<T>(EasySqlParameter<T> parameter, IReadOnlyList<KeyValueTuple<string, object>> convert)
        {
            var item = convert.FirstOrDefault(o => o.Key == this.Parameter);
            return this.Match(item);
        }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool Match(KeyValueTuple<string, object> item)
        {
            if (item != null)
            {
                if (item.Value == null)
                    return true;

                if (item.Value is INullableParameter)
                {
                    var @null = (INullableParameter)item.Value;
                    if (@null.HasValue)
                        return false;

                    return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// 格式化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="format"></param>
        /// <param name="parameter"></param>
        /// <param name="convert"></param>
        public override void Format<T>(SqlTagFormat format, EasySqlParameter<T> parameter, IReadOnlyList<KeyValueTuple<string, object>> convert)
        {
            var item = convert.FirstOrDefault(o => o.Key == this.Parameter);
            if (this.Match(item))
            {
                if (this.Labels.Any())
                {
                    if (format.IfContainer)
                    {
                        if (format.IfThenCount > 0 && this.Then != null)
                            format.Write(this.Then);
                        else
                            format.IfThenCount++;
                    }
                    else
                    {
                        format.Write(this.Then);
                    }

                    foreach (var line in this.Labels)
                    {
                        line.Format(format, parameter, convert);
                    }
                }
            }
        }
    }
}