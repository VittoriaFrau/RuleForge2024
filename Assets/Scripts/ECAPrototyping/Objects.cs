using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using System.IO;
using System;

namespace ECAPrototyping
{
    public class Position
    {
        private const string lastSessionIdFile = "Assets/ECAScripts/LastSessionPositionId.txt";
        private static int id = -1;
        public string Name { get; set; }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        
        public Position()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;

            Name = "pos" + GetId();
        }
        public Position(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            Name = "pos" + GetId();
        }

        private int GetId()
        {
            if (id == -1)
            {
                int lastSessionId = GetLastSessionId();
                SaveLastId(lastSessionId + 1);
                return lastSessionId + 1;
            } else
            {
                id++;
                SaveLastId(id);
                return id;
            }
        }

        private void SaveLastId(int id)
        {
            File.WriteAllText(lastSessionIdFile, id.ToString());
        }

        private int GetLastSessionId()
        {
            string[] lines = File.ReadAllLines(lastSessionIdFile);

            try
            {
                int lastId = Int32.Parse(lines[0]);
                return lastId;
            }
            catch (FormatException e)
            {
                Debug.LogError("Can't read last session ID!" + e.Message);
            }

            return -1;
        }
        
        public void Assign(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public void Assign(Position p)
        {
            this.x = p.x;
            this.y = p.y;
            this.z = p.z;
        }
        
        public void Assign(Vector3 p)
        {
            this.x = p.x;
            this.y = p.y;
            this.z = p.z;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return x + ", " + y + ", " + z;
        }

        public override bool Equals(object obj)
        {
            if(obj is Position)
            {
                Position p = obj as Position;
                return this.x == p.x && this.y == p.y && this.z == p.z;
            }
            else
            {
                return false;
            }
        }
    }

    public class Rotation
    {
        public string Name { get; set; }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        
        public void Assign(Rotation r)
        {
            this.x = r.x;
            this.y = r.y;
            this.z = r.z;
        }
        
        public void Assign(Vector3 r)
        {
            this.x = r.x;
            this.y = r.y;
            this.z = r.z;
        }
        
        public void Assign(Quaternion r)
        {
            this.x = r.eulerAngles.x;
            this.y = r.eulerAngles.y;
            this.z = r.eulerAngles.z;
        }
    }

    public class Path
    {
        public string Name { get; set; }

        public List<Position> Points { get; set; }

        public Path(List<Position> list)
        {
            Name = "Path";
            Points = list;
        }
        
        public Path(string name, List<Position> list)
        {
            Name = name;
            Points = list;
        }

        public override bool Equals(object obj)
        {
            if(obj is Path)
            {
                Path path = obj as Path;

                if(this.Points.Count != path.Points.Count)
                {
                    return false;
                }

                for(int i = 0; i < this.Points.Count; i++)
                {
                    if (!this.Points[i].Equals(path.Points[i]))
                        return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
