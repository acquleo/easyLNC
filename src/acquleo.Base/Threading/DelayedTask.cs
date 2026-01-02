using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace tet.Base.Threading
{
    /// <summary>
    /// Run a task after a delay
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class DelayedTask<A>
    {
        readonly Dictionary<string, CancellationTokenSource> taskMap = new Dictionary<string, CancellationTokenSource>();
        readonly object _lock = new object();

        class TaskData
        {
            public Action<A> action;
            public A arg;
            public string key;

        }
        /// <summary>
        /// Run a cancellable task after a delay
        /// </summary>
        /// <param name="key"></param>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        public void RunDelayed(string key, TimeSpan delay, Action<A> action, A arg)
        {
            lock (_lock)
            {
                Cancel(key);

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                taskMap.Add(key, cancellationTokenSource);

                TaskData taskData = new TaskData()
                {
                    key = key,
                    action = action,
                    arg = arg
                };

                Task.Delay(delay, cancellationTokenSource.Token).ContinueWith((t) =>
                {
                    if (t.IsCanceled) return;

                    taskData.action(taskData.arg);

                    Cancel(taskData.key);
                });
            }
        }

        /// <summary>
        /// Cancel a pending task and run a function immediately
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        public void Run(string key, Action<A> action, A arg)
        {
            lock (_lock)
            {
                Cancel(key);

                action(arg);
            }
        }

        /// <summary>
        /// Cancel a pending task
        /// </summary>
        /// <param name="key"></param>
        public void Cancel(string key)
        {
            if (taskMap.ContainsKey(key))
            {
                taskMap[key].Cancel();
                taskMap[key].Dispose();
                taskMap.Remove(key);
            }
        }
    }
}
