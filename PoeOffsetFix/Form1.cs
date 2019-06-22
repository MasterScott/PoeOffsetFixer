using System;
using System.Linq;
using System.Windows.Forms;
using HudBotWPF.Core;
using PoeHUD.Controllers;
using PoeOffsetFix.Logic;

namespace PoeOffsetFix
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LogWindow.SetBox(logTextBox);
            AddDefaultOffsets();
            UpdateTree();
            offsetsHierarchy.AfterSelect += SelectTreeNode;
        }

        private void SelectTreeNode(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is StructureOffset structOffset)
            {
                o_baseAddress.Text = structOffset.BaseAddress.ToString("x");
            }
            else if (e.Node.Tag is DataOffset dataOffset)
            {
                label2.Text = $"Found offsets: {string.Join(", ", dataOffset.FoundOffsets.Select(x => x.ToString("x")))}";
            }
        }

        private OffsetsFixer OffsetsFixer;
        private PoeProcessController PoeProcess;

        private void AddDefaultOffsets()
        {
            var ingameState = new StructureOffset("IngameState", null);
            ingameState.BaseAddress = Utils.ToAddr("32347F7030");
            ingameState.MaxStructSize = 26000;
            var leagueName = new DataOffset("LeagueName", new StringResultCompare(true, "Standard"));
            ingameState.Childs.Add(leagueName);
            PoeProcess = new PoeProcessController();
            PoeProcess.ConnectToProcess();
            OffsetsFixer = new OffsetsFixer(GameController.Instance.Memory, ingameState);
        }

        private void UpdateTree()
        {
            offsetsHierarchy.Nodes.Clear();
            var initialNode = new TreeNode(OffsetsFixer.InitialStructure.Name);
            initialNode.Tag = OffsetsFixer.InitialStructure;
            offsetsHierarchy.Nodes.Add(initialNode);

            foreach (var initialStructureChild in OffsetsFixer.InitialStructure.Childs)
            {
                var childNode = new TreeNode(initialStructureChild.Name);
                childNode.Tag = initialStructureChild;
                initialNode.Nodes.Add(childNode);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OffsetsFixer.FixStructureChilds();
        }
    }
}
