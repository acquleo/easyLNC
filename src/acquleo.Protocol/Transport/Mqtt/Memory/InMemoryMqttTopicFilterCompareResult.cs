// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace acquleo.Protocol.Transport.Mqtt.Memory
{
    /// <summary>
    /// 
    /// </summary>
    public enum InMemoryMqttTopicFilterCompareResult
    {
        /// <summary>
        /// 
        /// </summary>
        NoMatch,
        /// <summary>
        /// 
        /// </summary>
        IsMatch,
        /// <summary>
        /// 
        /// </summary>
        FilterInvalid,
        /// <summary>
        /// 
        /// </summary>
        TopicInvalid
    }
}