using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMenager : MonoBehaviour
{
    public static TaskMenager Instance;
    void Awake() => Instance = this;
    [SerializeField] TaskPanel[] tasks = new TaskPanel[3];
    [SerializeField] EndGameMenager endGameMenager;
    
    public void SetTasks(int[] ids)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            this.tasks[i].SetupTask(this, ids[i]);
        }
    }

    List<HexField> parks = new List<HexField>();
    List<HexField> homes = new List<HexField>();
    List<HexField> block = new List<HexField>();
    List<HexField> shop = new List<HexField>();

    public List<TaskContent> taskContents = new List<TaskContent>();

    public void CollectPoints()
    {
        HexGrid.Instance.All((HexField field)=>{
            switch (field.Instance.id)
            {
                case 0:
                parks.Add(field);
                break;
                case 1:
                homes.Add(field);
                break;
                case 2:
                block.Add(field);
                break;
                case 3:
                shop.Add(field);
                break;
            }
        });

        int pointsSum = 0;
        foreach (TaskPanel item in tasks)
        {
            pointsSum += item.MakeTask();
        }
        endGameMenager.SetFinalResult(pointsSum);
        
    }

    

    public int task1()
    {
        int points = 0;
        foreach (HexField house in this.homes)
        {
            foreach (HexField item in HexGrid.Instance.Neighbors(house.Coordinates))
            {
                if (item.Instance.id == 2)
                {
                    points+=1;
                }
            }
        }

        foreach (HexField block in this.block)
        {
            bool temp = false;
            foreach (HexField item in HexGrid.Instance.Neighbors(block.Coordinates))
            {
                if (item.Instance.id == 1)
                {
                    temp = true;                
                }
            }
            if (temp==false) points-=2;
        }

        GameMenager.Instance.debug.text += $"/task1:{points}";
        return points;
        
    }

    public int task2()
    {
        int blocks =0, parks = 0, shops = 0;
        foreach (HexField house in this.homes)
        {
            foreach (HexField item in HexGrid.Instance.Neighbors(house.Coordinates))
            {
                if (item.Instance.id == 0) parks++;
                if (item.Instance.id == 2) blocks++;
                if (item.Instance.id == 3) shops++;
            }
        }
        int points =(blocks==shops&&blocks==parks)?3:0;
        GameMenager.Instance.debug.text += $"/task2:{points}";
        return points;
    }

    bool IsFieldInClaster(List<Klaster> klasters, HexField field)
    {
        foreach (Klaster item in klasters)
        {
            if (item.fields.Contains(field)) return true;
        }
        return false;
    }

    struct Klaster
    {
        public int id;
        public List<HexField> fields;

        public Klaster(int id)
        {
            this.id = id;
            this.fields = new List<HexField>();
        }

        public void Join(HexField field)
        {
            this.fields.Add(field);
            foreach (HexField item in HexGrid.Instance.Neighbors(field.Coordinates))
            {
                if (this.fields.Contains(item)) continue;
                if (item.Instance.id == id)
                {
                    
                    Join(item);
                }
            }
            
        }
    }

    public int task3()
    {
        List<Klaster> klasters = new List<Klaster>();

        foreach (HexField item in block)
        {
            if (!IsFieldInClaster(klasters, item))
            {
                Klaster klaster = new Klaster(2);
                klaster.Join(item);
                klasters.Add(klaster);
            }
        }

        int points = 0;
        foreach (Klaster item in klasters)
        {
            switch (item.fields.Count)
            {
                case 1:
                points-=1;
                break;
                case 2:
                points+=1;
                break;
                case 3:
                points+=3;
                break;
                default:
                points+=5;
                break;
            }
        }

        GameMenager.Instance.debug.text += $"/task3:{points}";
        return points;
    }

    public int task4()
    {
        List<Klaster> klasters = new List<Klaster>();

        foreach (HexField item in shop)
        {
            if (!IsFieldInClaster(klasters, item))
            {
                Klaster klaster = new Klaster(3);
                klaster.Join(item);
                klasters.Add(klaster);
            }
        }

        int points = 0;
        foreach (Klaster item in klasters)
        {
            switch (item.fields.Count)
            {
                case 1:
                points+=1;
                break;
                default:
                points-=2;
                break;
            }
        }

        GameMenager.Instance.debug.text += $"/task4:{points}";
        return points;
    }

    public int task5()
    {
        List<Klaster> klasters = new List<Klaster>();

        foreach (HexField item in homes)
        {
            if (!IsFieldInClaster(klasters, item))
            {
                Klaster klaster = new Klaster(1);
                klaster.Join(item);
                klasters.Add(klaster);
            }
        }

        int points = 0;
        Klaster max = klasters[0];
        foreach (Klaster item in klasters)
        {
            if(max.fields.Count < item.fields.Count)
            {
                max = item;
            }
        }

        List<HexField> polas = new List<HexField>();

        foreach (HexField item in max.fields)
        {
            foreach (HexField inside in HexGrid.Instance.Neighbors(item.Coordinates))
            {
                if (inside.Instance.id == 0)
                {
                    if (polas.Contains(inside)) continue;
                    polas.Add(inside);
                }
            }
        }

        points = polas.Count;
        GameMenager.Instance.debug.text += $"/task4:{points}";
        return points;
    }

    public int task6()
    {
        int points = 0;
        foreach (HexField item in block)
        {
            int parki = 0;
            int domki = 0;
            foreach (HexField inside in HexGrid.Instance.Neighbors(item.Coordinates))
            {
                if (inside.Instance.id == 0)
                {
                    parki++;
                }
                if (inside.Instance.id == 1)
                {
                    domki++;
                }
            }
            if (parki>=2) points+=2;
            if (domki>=2) points-=2;
        }
        return points;
    }

    public int task7()
    {
        int points = 0;
        
        foreach (HexField item in homes)
        {
            List<HexField> list = new List<HexField>();
            foreach (HexField inside in HexGrid.Instance.Neighbors(item.Coordinates))
            {
                if (inside.Instance.id == 0)
                {
                    points+=Look(inside, list);
                }
            }
        }
        return points;
    }

    int Look(HexField target, List<HexField> list)
    {
        list.Add(target);
        foreach (HexField inside in HexGrid.Instance.Neighbors(target.Coordinates))
        {
            if (inside.Instance.id == 0)
            {
                if (list.Contains(inside)) continue;
                Look(inside, list);
            }
            if (inside.Instance.id == 3)
            {
                return list.Count;
            }
        }
        return 0;
    }

    public int task8()
    {
        int best=0;
        int best2=0;

        int id=-1;

        if (parks.Count > best)
        {
            best2 = best;
            best = parks.Count;
            id = 0;
        }

        if (homes.Count > best)
        {
            best2 = best;
            best = homes.Count;
            id = 1;
        }

        if (block.Count > best)
        {
            best2 = best;
            best = block.Count;
            id = 2;
        }

        if (shop.Count > best)
        {
            best2 = best;
            best = shop.Count;
            id = 3;
        }

        return (best-best2);
    }


    [System.Serializable]
    public struct TaskContent
    {
        public string title;
        public string content;
        public Sprite image;
    }
}
