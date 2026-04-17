using System;
using System.Collections.Generic;

namespace UIFramework.Events
{
    /// <summary>
    /// 全局轻量级事件中心，解耦UI与业务逻辑，基于C#委托实现，非常高效。
    /// </summary>
    public static class UIEventCenter
    {
        private static readonly Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

        // ------------------ 无参数事件 ------------------
        public static void AddListener(string eventType, Action handler)
        {
            if (!eventTable.ContainsKey(eventType)) eventTable.Add(eventType, null);
            eventTable[eventType] = (Action)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Action handler)
        {
            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] = (Action)eventTable[eventType] - handler;
                if (eventTable[eventType] == null) eventTable.Remove(eventType);
            }
        }

        public static void Trigger(string eventType)
        {
            if (eventTable.TryGetValue(eventType, out Delegate d) && d is Action action)
            {
                action.Invoke();
            }
        }

        // ------------------ 单参数事件 ------------------
        public static void AddListener<T>(string eventType, Action<T> handler)
        {
            if (!eventTable.ContainsKey(eventType)) eventTable.Add(eventType, null);
            eventTable[eventType] = (Action<T>)eventTable[eventType] + handler;
        }

        public static void RemoveListener<T>(string eventType, Action<T> handler)
        {
            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] = (Action<T>)eventTable[eventType] - handler;
                if (eventTable[eventType] == null) eventTable.Remove(eventType);
            }
        }

        public static void Trigger<T>(string eventType, T arg)
        {
            if (eventTable.TryGetValue(eventType, out Delegate d) && d is Action<T> action)
            {
                action.Invoke(arg);
            }
        }
        
        // 可根据需要拓展多参数版本 Action<T1, T2>...
    }
}
