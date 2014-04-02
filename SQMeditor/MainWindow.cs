using ArmaSQMParser;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SQMeditor
{
    public partial class MainWindow : Form
    {
        private SQMParser Parser;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            DialogResult result = OpenFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Parser = new SQMParser(OpenFileDialog.FileName);
                Parser.SetupBackgroundWorker(Thread);
                ProgressBar.Show();
                Thread.RunWorkerAsync();
            }
        }

        private void Thread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Parser != null)
            {
                Parser.Process();
            }
        }

        private void Thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void Thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Hide();
            ProgressBar.Value = 0;
            CreateTree(null);
        }

        private void CreateTree(SQMTreeNode rootnode)
        {
            foreach (var data in Parser.Children)
            {
                SQMTreeNode node = ProcessChild(data,rootnode);
                if (rootnode != null)
                {
                    rootnode.Nodes.Add(node);
                }
                else
                {
                    NodesView.Nodes.Add(node);
                }
            }
        }

        private static SQMTreeNode ProcessChild(NodeData data,SQMTreeNode rootnode)
        {
            SQMTreeNode node = new SQMTreeNode(data.ToString(), data);
            foreach (var atr in data.Atributes.Keys)
            {
                var value = data.Atributes[atr];
                if (value.GetType().IsGenericType)
                {
                    value = value[0];
                }
                SQMTreeNode childnode = new SQMTreeNode(atr, data, atr);
                childnode.Text = childnode.ToString();
                childnode.Tag = node;
                node.Nodes.Add(childnode);
            }
            foreach (var child in data.Children)
            {
                SQMTreeNode childnode = ProcessChild(child, node);
                node.Nodes.Add(childnode);
            }
            return node;
        }

        private void NodesView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            SQMTreeNode NewNode = (SQMTreeNode)e.Item;
            if (NewNode.atrkey != null|| NewNode.data.type == ClassType.Mission || NewNode.data.type == ClassType.Intel || NewNode.data.type == ClassType.Markers || NewNode.data.type == ClassType.MainGroup)
                return;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void NodesView_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void NodesView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void NodesView_DragDrop_1(object sender, DragEventArgs e)
        {
            SQMTreeNode NewNode;

            if (e.Data.GetDataPresent(typeof(SQMTreeNode)))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                SQMTreeNode DestinationNode = (SQMTreeNode)((TreeView)sender).GetNodeAt(pt);
                NewNode = (SQMTreeNode)e.Data.GetData(typeof(SQMTreeNode));
                if (DestinationNode.TreeView == NewNode.TreeView && DestinationNode.Parent != null && DestinationNode != NewNode)
                {
                    NewNode.Parent.Nodes.Remove(NewNode);
                  //  NewNode.Parent.Text = NewNode.Parent.ToString();
                    NodeData data = NewNode.data;
                    data.Parent.Children.Remove(data);
                    DestinationNode.data.Parent.Children.Insert(DestinationNode.data.Parent.Children.IndexOf(DestinationNode.data), data);
                    DestinationNode.Parent.Text = DestinationNode.Parent.ToString();
                    DestinationNode.Parent.Nodes.Insert(DestinationNode.Parent.Nodes.IndexOf(DestinationNode),NewNode);
                    DestinationNode.Parent.Expand();
                    DestinationNode.TreeView.SelectedNode = NewNode;
                }
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Parser.Export(saveFileDialog.FileName);
              //  Parser.SetupBackgroundWorker(Thread);
              //  ProgressBar.Show();
             //   Thread.RunWorkerAsync();
            }
        }

        private void NodesView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (!e.Node.IsVisible)
                return;
            SQMTreeNode node = (SQMTreeNode)e.Node;
            Rectangle itemRect = e.Bounds;
            itemRect.X += 1; // shift one
            Rectangle textRect = new Rectangle(itemRect.Left, itemRect.Top, itemRect.Right - itemRect.Left, itemRect.Bottom - itemRect.Y);

            int textHeight = (int)TextRenderer.MeasureText(e.Node.Text, DefaultFont).Height;
            int textWidth = (int)TextRenderer.MeasureText(e.Node.Text, DefaultFont).Width;

            textRect.Height = textHeight;
            textRect.Width = textWidth + 3;
            Color textColor = Color.Black;
            if (node.atrkey == null || node.atrkey == String.Empty)
            {
                switch (node.data.type)
                {
                    case ClassType.Unit:
                        textColor = GetSideColor(node.data.Atributes["side"]);
                        break;
                    case ClassType.Group:
                        textColor = GetSideColor(node.data.GetFirstUnit().Atributes["side"]);
                        break;
                }
            }
        //    if ((e.State & TreeNodeStates.Selected) != 0)
          //  {
         //       e.Graphics.FillRectangle(SystemBrushes.Highlight, textRect);
          //  }
            String str = node.data.ToString();
            if(node.atrkey != null && node.atrkey != String.Empty)
            {
                str = node.atrkey + "=" + node.data.Atributes[node.atrkey];
            }
            TextRenderer.DrawText(e.Graphics, str.Trim(), DefaultFont, textRect.Location,textColor );
            e.DrawDefault = false;
        }
        private Color GetSideColor(string side = "")
        {
            switch (side)
            {
                case "GUER":
                    return Color.Green;
                case "WEST":
                    return Color.Blue;
                case "EAST":
                    return Color.Red;
            }
            return Color.Black;
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {

        }
    }

}