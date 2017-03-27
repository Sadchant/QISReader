using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace QISReader.Model
{
    class BackgroundTaskManager
    {
        //
        // Register a background task with the specified taskEntryPoint, name, trigger,
        // and condition (optional).
        //
        // taskEntryPoint: Task entry point for the background task.
        // taskName: A name for the background task.
        // trigger: The trigger for the background task.
        // condition: Optional parameter. A conditional event that must be true for the task to fire.
        //
        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            // Check for existing registrations of this background task.
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    // The task is already registered.
                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }

            // Register the background task.
            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }
            var access = await BackgroundExecutionManager.RequestAccessAsync();
            BackgroundTaskRegistration task = builder.Register();
            return task;
        }

        public static void Unregister()
        {
            foreach (var t in BackgroundTaskRegistration.AllTasks)
            {
                BackgroundTaskRegistration x = (BackgroundTaskRegistration)t.Value;
                x.Unregister(true);
            }
        }
    }
}
