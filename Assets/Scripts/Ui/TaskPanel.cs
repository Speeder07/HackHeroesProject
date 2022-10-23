using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskPanel : MonoBehaviour
{
    TaskMenager taskMenager;
    [SerializeField] TMP_Text taskId;
    [SerializeField] TMP_Text taskContent;
    [SerializeField] Image image;
    [SerializeField] int id;
    public void SetupTask(TaskMenager taskMenager, int id)
    {
        this.taskMenager = taskMenager;
        this.id = id;
        taskId.text = id.ToString();

        TaskMenager.TaskContent content = taskMenager.taskContents[id-1];
        taskId.text = content.title;
        taskContent.text = content.content;
        image.sprite = content.image;
    }

    public int MakeTask()
    {

        switch (id)
        {
            case 1:
            return taskMenager.task1();
            break;
            case 2:
            return taskMenager.task2();
            break;
            case 3:
            return taskMenager.task3();
            break;
            case 4:
            return taskMenager.task4();
            break;
            case 5:
            return taskMenager.task5();
            break;
            case 6:
            return taskMenager.task6();
            break;
            case 7:
            return taskMenager.task7();
            break;
            case 8:
            return taskMenager.task8();
            break;
            default:
            return 0;
        }
        return 0;
    }
}
