using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public bool isActive;

    public string title;
    public string description;
    public string tracker;
    public int expReward;
    public int healthImprovement;

    public QuestGoal goal;

    public void Complete()
    {
        isActive = false;
    }
}
