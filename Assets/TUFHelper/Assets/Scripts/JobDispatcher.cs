using System;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class JobDispatcher : MonoBehaviour
{

    internal static JobDispatcher instance;

    private Queue<Action> jobs = new Queue<Action>();

    public void Awake()
    {
        instance = this;
        DisableDuplicateEventSystems();
        Debug.Log("[TUFHelper] Job Dispatcher New Instance: " + SceneManager.GetActiveScene().name);
    }

    private static void DisableDuplicateEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>(true);
        if (eventSystems.Length <= 1)
        {
            return;
        }

        EventSystem keep = EventSystem.current ?? eventSystems[0];
        foreach (EventSystem eventSystem in eventSystems)
        {
            if (eventSystem != null && eventSystem != keep)
            {
                eventSystem.gameObject.SetActive(false);
            }
        }
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
