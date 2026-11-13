using System;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    [SerializeField] private Project project;
    [SerializeField] private TimeCounter timeCounter;

    public Project Project => project;

    public Sprint GetCurrentSprint()
    {
        if (timeCounter == null) return null;
        return GetSprintByDay(timeCounter.CurrentDay);
    }

    public Sprint GetSprintByDay(int day)
    {
        if (project == null || project.Sprints == null) return null;

        int index = day - 1;
        if (index < 0 || index >= project.Sprints.Length) return null;

        return project.Sprints[index];
    }

    public Task[] GetCurrentDayTasks()
    {
        if (timeCounter == null) return Array.Empty<Task>();
        return GetTasksByDay(timeCounter.CurrentDay);
    }

    public Task[] GetTasksByDay(int day)
    {
        Sprint sprint = GetSprintByDay(day);
        if (sprint == null || sprint.SprintTasks == null) return Array.Empty<Task>();

        return sprint.SprintTasks;
    }
}