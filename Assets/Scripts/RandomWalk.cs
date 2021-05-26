using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://math.stackexchange.com/questions/3683205/explanation-of-c-source-code-for-random-walk
public class RandomWalk
{
    public List<Vector3Int> PList = new List<Vector3Int>();

    public void RunScript(int time, Vector3Int Start)
    {
        List<Vector3Int> pList = new List<Vector3Int>();
        Walker w = new Walker();
        //Random random = new Random(seed);
        for (int i = 0; i < time; i++)
        {
            int rnd = Random.Range(0, 6);
            w.step(rnd);
            //check if it's valid
            pList.Add(w.pos());
        }
        foreach (var p in pList)
        {
            PList.Add(new Vector3Int(Start.x + p.x, Start.y + p.y, Start.z + p.z));
        }

    }

    public class Walker
    {
        public int x;
        public int y;
        public int z;
        public int rnd;

        public Walker()
        {
            x = 0;
            y = 0;
            z = 0;
            rnd = 0;
        }

        public Vector3Int pos()
        {
            Vector3Int posPt = new Vector3Int(x, y, z);
            return posPt;
        }

        public int step(int rnd)
        {
            int choice = rnd;
            if (choice == 0)
            {
                x++;
            }
            else if (choice == 1)
            {
                x--;
            }
            else if (choice == 2)
            {
                y++;
            }
            else if (choice == 3)
            {
                y--;
            }
            else if (choice == 4)
            {
                z++;
            }
            else if (choice == 5)
            {
                z--;
            }

            return choice;
        }
    }

}
