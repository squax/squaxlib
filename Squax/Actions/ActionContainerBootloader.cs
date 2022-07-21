using System.Collections.Generic;
using UnityEngine;

namespace Squax.Actions
{
    /// <summary>
    /// Bootloader for an action container list.
    /// </summary>
    public class ActionContainerBootloader : MonoBehaviour
    {
        [SerializeField]
        List<ActionContainer> actionContainerList;

        [SerializeField]
        private bool runAsInstance = false;

        void Awake()
        {
            if (actionContainerList != null)
            {
                foreach(var actionContainer  in actionContainerList)
                {
                    if (runAsInstance == true)
                    {
                        var instance = Object.Instantiate(actionContainer) as ActionContainer;

                        if (instance != null)
                        {
                            instance.StartJobs(runAsInstance);
                        }
                    }
                    else
                    {
                        if (actionContainer != null)
                        {
                            actionContainer.StartJobs(runAsInstance);
                        }
                    }
                }
            }
        }
    }
}
