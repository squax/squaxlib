using RSG;
using System.Collections.Generic;
using UnityEngine;

namespace Squax.Actions
{
    /// <summary>
    /// Action container acts as the root object.
    /// </summary>
    [CreateAssetMenu(menuName = "Squax/Action Container")]
    public class ActionContainer : ScriptableObject
    {
        /// <summary>
        /// A list of all the actions in the container.
        /// </summary>
        [SerializeField]
        private List<Action> actions;

        /// <summary>
        /// Start all entry action jobs.
        /// </summary>
        public void StartJobs(bool runAsInstance = false)
        {
            List<Action> instances = new List<Action>();

            foreach (var action in actions)
            {
                if (runAsInstance == true)
                {
                    var actionInstance = Object.Instantiate(action) as Action;

                    if (actionInstance.IsStartupAction == true)
                    {
                        if (ActionController.Instance.CanStartJob(actionInstance) == true)
                        {
                            ActionController.Instance.StartJob(actionInstance);
                        }
                    }

                    instances.Add(actionInstance);

                    continue;
                }

                if (action != null && action.IsStartupAction == true)
                {
                    if (ActionController.Instance.CanStartJob(action) == true)
                    {
                        ActionController.Instance.StartJob(action);
                    }
                }
            }

            if (runAsInstance == true)
            {
                actions = instances;
            }
        }

        public void Interrupt()
        {
            foreach (var action in actions)
            {
                action.Interrupt();
            }
        }

        public bool HasJobsRunning()
        {
            foreach (var action in actions)
            {
                if(action.CurrentState == Action.State.Running)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
