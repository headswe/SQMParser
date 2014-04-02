using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArmaSQMParser
{
    public class SQMParser
    {
        public List<NodeData> Children = new List<NodeData>();
        public List<String> Errors = new List<string>();
        private String _filePath = "";
        private int _currentLine = 0;
        private int _totalLines = 0;
        Regex _classRegex = new Regex(".*class\\s");
        Regex _atrriRegex = new Regex("^.*=.*");
        private NodeData _currentNode;
        private String[] _lines;
        private Object _worker;
        public SQMParser(string path)
        {
            if (!File.Exists(path))
                Errors.Add("File does not exist..");
            _filePath = path;
        }
        public void ReportProgress()
        {
            if (_worker == null)
                return;
            double x = (double)((double)_currentLine / (double)_lines.Length);
            int prec =  (int)(x*100);
            BackgroundWorker work = (BackgroundWorker)_worker;
            work.ReportProgress(prec);
        }
        public void Export(String path)
        {
            StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create));
            writer.NewLine = "\r\n";
            foreach (var child in Children)
            {
                child.Export(-1,writer);
            }
            writer.Close();
        }


        public void Print(int depth = 0)
        {
            foreach (var child in Children)
            {
                String str = "";
                for (int x = 0; x <= depth; x++)
                {
                    str += "-";
                }
                str = str + "> " + child.ToString();
                Console.WriteLine(str);
                child.Print(0);
            }
        }




        public void Process()
        {
            _lines = File.ReadAllLines(_filePath);
            _totalLines = _lines.Length;

            while(_currentLine < _totalLines)
            {
                String line = _lines[_currentLine];
                var match = _classRegex.Match(line);
                if (match.Success)
                {
                    ProcessClass(line,null);
                }
                else
                {
                    _currentLine++;
                }

            }
            int jk = 0;
        }

        private void ProcessClass(string startLine,NodeData parent)
        {
            string name = startLine.Substring(startLine.IndexOf(' ') + 1);
            ClassType type = ClassType.Unknown;
            if (name.Contains("Vehicles") && parent != null && parent.type == ClassType.Group)
            {
                type = ClassType.Vehicles;
            }
            else if (name.Contains("Item") && parent != null && parent.type == ClassType.Vehicles)
            {
                type = ClassType.Unit;
            }
            else if (name.Contains("Item") && parent != null && parent.type == ClassType.MainGroup)
            {
                type = ClassType.Group;
            }
            else if (name == "Groups")
            {
                type = ClassType.MainGroup;
            }
            else if (name.Contains("Mission"))
            {
                type = ClassType.Mission;
            }
            NodeData data = new NodeData() { ClassName = name, type = type };
            ReportProgress();

            if (parent != null)
            {
                data.Parent = parent;
                parent.Children.Add(data);
            }
            else
                Children.Add(data);
            if (startLine.Contains(";"))
            {
                _currentLine++;
                return;
            }
            _currentLine++;
            while(true)
            {
                String line = _lines[_currentLine];
                var match = _classRegex.Match(line);
                if (match.Success)
                {
                    ProcessClass(line, data);
                }
                else if (_atrriRegex.IsMatch(line))
                {
                    if (line.Contains("[]="))
                    {
                        ProcessArray(line, data);
                    }
                    else
                    {
                        ProcessAtribute(line, data);
                    }
                }
                else if (Regex.IsMatch(line, "(.*)};"))
                {
                    break;
                }
                _currentLine++;
            }
        }


        private void ProcessAtribute(string line, NodeData data)
        {
            String key = line.Substring(0, line.IndexOf('='));
            key = key.Trim();
            key = key.Trim(new char[] {'\t','\"',';'});

            String value = line.Substring(line.IndexOf('=') + 1, line.LastIndexOf(';')-line.IndexOf('='));
            value = value.Trim();
            value = value.Trim(new char[] {'\t',';'});
            if (value[0] == '\"')
            {
                value = value.Substring(value.IndexOf('\"')+1, value.LastIndexOf('\"')-1);
            }
            double number = 0f;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out number))
            {
                data.Atributes.Add(key, number);
            }
            else
            {
                data.Atributes.Add(key, value);
            }
        }

        private void ProcessArray(string startLine, NodeData data)
        {
            String[] split = startLine.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
            String key = split[0];
            key = key.Trim();
            key = key.Trim(new char[] { '\t', '\"', ';' });
            // multiline array
            if (split.Count() <= 1)
            {
                List<String> list = new List<string>();
                while (true)
                {
                    _currentLine++;
                    String line = _lines[_currentLine];
                    if (Regex.IsMatch(line, "(.*)};"))
                    {
                        break;
                    }
                    else if(!Regex.IsMatch(line, "(.*){"))
                    {
                        line = line.Trim();
                        line = line.Trim(new char[] { ',', '\t', '\"', ';' });
                        list.Add(line);
                    }
                }
                data.Atributes.Add(key, list);
            }
            else
            {
                String value = split[1];
                value = value.Trim(new char[] { '\t', '\"', ';' ,'{','}'});
                String[] values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() >= 3)
                {
                    double x = 0;
                    double y = 0;
                    double z = 0;
                    double.TryParse(values[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out x);
                    double.TryParse(values[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out y);
                    double.TryParse(values[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out z);
                    Vector3 vec = new Vector3() { x = x, y = y, z = z };
                    data.Atributes.Add(key, vec);
                }
            }
        }

        public void SetupBackgroundWorker(System.ComponentModel.BackgroundWorker Thread)
        {
            _worker = Thread;
        }
    }
}
