using System;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JobDispatcher : MonoBehaviour
{

    internal static JobDispatcher instance;

    private Queue<Action> jobs = new Queue<Action>();

    public void Awake()
    {
        instance = this;
        Debug.Log("[TUFHelper] Job Dispatcher New Instance: " + SceneManager.GetActiveScene().name);
    }

    public void Update()
    {
        while (jobs.Count > 0)
        {
            try
            {
                jobs.Dequeue().Invoke();
            }
            catch(Exception ex)
            {
                Main.Logger.LogException(ex);
            }
        }
    }

    internal static void AddJob(Action newJob)
    {
        instance.jobs.Enqueue(newJob);
    }

    internal static void AddJobs(Queue<Action> newJobs)
    {
        foreach (Action ac in newJobs)
        {
            instance.jobs.Enqueue(ac);
        }
    }

}
