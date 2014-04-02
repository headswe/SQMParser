using ArmaSQMParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQMeditor
{
    class SQMTreeNode : TreeNode
    {
        public NodeData data;
        public String atrkey;
        public override string ToString()
        {
            if ((atrkey != String.Empty &&  atrkey != null) && data != null )
            {
                return atrkey+"="+data.Atributes[atrkey].ToString();
            }
            else if (data != null)
            {
                return data.ToString();
            }
            else
                return base.ToString();
        }
        public SQMTreeNode() : base()
        {
        }

        public SQMTreeNode(String text,NodeData newdata,String key = "") : base(text)
        {
            if (newdata != null)
                data = newdata;
            if (key != String.Empty)
                atrkey = key;
        }
    }
}
