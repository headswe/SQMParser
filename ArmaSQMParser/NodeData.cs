using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaSQMParser
{
    public enum ClassType:int {Group,MainGroup,Vehicles,Unit,Mission,Intel,Intro,OutroWin,OutroLoose,Markers,Unknown}
    public struct Vector3
    {
        public double x;
        public double y;
        public double z;
        public override string ToString()
        {
            return "{"+x+"."+y+"."+z+"}";
        }
    }
    public class NodeData
    {
        public string ClassName = "";
        public ClassType type = ClassType.Unknown;
        public Dictionary<String, dynamic> Atributes = new Dictionary<string, dynamic>();
        public List<NodeData> Children = new List<NodeData>();
        public NodeData Parent = null;
        public String GetDescription()
        {
            if(type == ClassType.Unit)
            {
                if(Atributes.ContainsKey("description"))
                    return Atributes["description"];
                else if(Atributes.ContainsKey("vehicle"))
                    return Atributes["vehicle"];
            }
            else if(type == ClassType.Group)
            {
                
                foreach(var child in Children)
                {
                    var str = ClassName;
                    try
                    {
                        str = child.Children.Where(item => item.Atributes.ContainsKey("description")).First().Atributes["description"];
                    }
                    catch (Exception ex)
                    {
                        // TODO BAD
                    }
                    return str +"(GROUP)";
                }
                
            }
            return ClassName;
        }
        public override string ToString()
        {
            if (type == ClassType.Unit || type == ClassType.Group)
            {
                String key = GetDescription();
                return key;
            }
            else
                return ClassName;
        }
        public NodeData GetFirstUnit()
        {
            NodeData node = null;

            foreach (var child in Children)
            {
                try
                {
                    node = child.Children.Where(item => item.Atributes.ContainsKey("vehicle")).First();
                }
                catch (Exception ex)
                {
                    // TODO BAD
                }
                if (node != null)
                    break;
            }

            return node;
        }
        public void Print(int depth = 0)
        {
            depth += 1;
            foreach (var child in Children)
            {
                String str = "";
                for (int x = 0; x <= depth; x++)
                {
                    str += "-";
                }
                str = str + "> " + child.ToString();
                Console.WriteLine(str);
                child.Print(depth);
                Console.ReadKey();
            }

        }

        internal void Export(int depth, StreamWriter writer)
        {
            String tabs = "";
            for (int x = 0; x <= depth; x++)
            {
                tabs += "\t";
            }
            if (type == ClassType.Unit || type == ClassType.Group)
            {
                writer.WriteLine(tabs + "class Item" + Parent.Children.IndexOf(this));
            }
            else
            {
                writer.WriteLine(tabs + "class " + this.ClassName);
            }
            writer.WriteLine(tabs+"{");
            depth += 1;
            String atriTabs = tabs + "\t";
            if (Atributes.ContainsKey("items"))
            {
                Atributes["items"] = (double)Children.Count;
            }
            foreach (var child in Atributes)
            {
                String str = "";
                
                if(child.Value is string)
                {
                    str += child.Key+"=\""+child.Value+"\";";
                }
                else if(child.Value is Double)
                {
                    str += child.Key+"="+child.Value+";";
                }
                else if (child.Value.GetType().IsGenericType && child.Value.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    List<string> objectList = child.Value;
                    str += child.Key + "=\n"+atriTabs+"{\n";
                    foreach (var s in objectList)
                    {
                        if(s != objectList.Last())
                        {
                            str += atriTabs + atriTabs +"\""+ s + "\",\n";
                        }
                        else
                            str += atriTabs+atriTabs+"\""+ s + "\"\n"+atriTabs+"};";
                    }
                }
                else if (child.Value is Vector3)
                {
                    Vector3 vec = (Vector3)child.Value;
                    str += child.Key + "={" + vec.x + "!" + vec.y + "!" + vec.z + "};";
                    str = str.Replace(",", ".");
                    str = str.Replace("!", ",");
                }
                writer.WriteLine(atriTabs+str);
            }
            foreach (var child in Children)
            {
                child.Export(depth,writer);
            }
            writer.WriteLine(tabs+"};");
        }
    }
}
