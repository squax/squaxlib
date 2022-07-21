using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Squax.Patterns;

namespace Squax.Actions
{
    /// <summary>
    /// Manages every active Action in the scene.
    /// </summary>
    public class ActionController : UnitySingleton<ActionController>
    {
        /// <summary>
        /// A list of running actions.
        /// </summary>
        List<Action> jobs = new List<Action>();

        /// <summary>
        /// Is the controller running any jobs?
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return jobs.Count > 0;
            }
        }

        void Awake()
        {
            OnAwake();
        }

        /// <summary>
        /// Tick all jobs.
        /// </summary>
        void Update()
        {
            // Tick each job.
            for (int nIndex = 0; nIndex < jobs.Count; ++nIndex)
            {
                Action job = jobs[nIndex];

                job.Update();

                if (job.HasEnded == true)
                {
                    jobs.RemoveAt(nIndex--);

                    var nextJobs = job.End();

                    if (nextJobs != null)
                    {
                        for (int i = 0; i < nextJobs.Count; ++i)
                        {
                            Action nextJob = nextJobs[i];

                            if (jobs.Contains(nextJob) == false)
                            {
                                StartJob(nextJob, job);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tick all jobs.
        /// </summary>
        void FixedUpdate()
        {
            // Tick each job.
            for (int nIndex = 0; nIndex < jobs.Count; ++nIndex)
            {
                Action job = jobs[nIndex];

                job.FixedUpdate();

                if (job.HasEnded == true)
                {
                    jobs.RemoveAt(nIndex--);

                    var nextJobs = job.End();

                    if (nextJobs != null)
                    {
                        for (int i = 0; i < nextJobs.Count; ++i)
                        {
                            Action nextJob = nextJobs[i];

                            if (jobs.Contains(nextJob) == false)
                            {
                                StartJob(nextJob, job);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tick all jobs.
        /// </summary>
        void LateUpdate()
        {
            // Tick each job.
            for (int nIndex = 0; nIndex < jobs.Count; ++nIndex)
            {
                Action job = jobs[nIndex];

                job.FixedUpdate();

                if (job.HasEnded == true)
                {
                    jobs.RemoveAt(nIndex--);

                    var nextJobs = job.End();

                    if (nextJobs != null)
                    {
                        for (int i = 0; i < nextJobs.Count; ++i)
                        {
                            Action nextJob = nextJobs[i];

                            if (jobs.Contains(nextJob) == false)
                            {
                                StartJob(nextJob, job);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if we can start a potential job.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool CanStartJob(Action action)
        {
            if (action == null)
            {
                return false;
            }

            return action.CurrentState == Action.State.Idle;
        }

        /// <summary>
        /// Enter job.
        /// </summary>
        /// <param name="action"></param>
        public void StartJob(Action action, Action parent = null)
        {
            if (action == null)
            {
                return;
            }

            if (action.Enter(parent) == true)
            {
                jobs.Add(action);
            }
        }
    }
}

