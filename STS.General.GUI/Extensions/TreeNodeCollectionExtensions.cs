using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace STS.General.GUI.Extensions
{
    public static class TreeNodeCollectionExtensions
    {
        public static TreeNode BuildNode(this TreeNodeCollection self, params string[] path)
        {
            var category = path[0].Split('\\');
            var name = new string[] { path[path.Length - 1] };

            path = category.Concat(name).ToArray();
            TreeNode node = null;
            for (int i = 0; i < path.Length; i++)
            {
                node = self.FindNode(path[i]);
                if (node == null)
                    node = self.Add(path[i], path[i]);

                self = node.Nodes;
            }

            return node;
        }

        public static TreeNode FindNode(this TreeNodeCollection self, string text)
        {
            return self.Cast<TreeNode>().FirstOrDefault(node => node.Text == text);
        }

        public static IEnumerable<TreeNode> Iterate(this TreeNodeCollection self)
        {
            foreach (var node in self.Cast<TreeNode>())
            {
                yield return node;

                foreach (var internalNode in Iterate(node.Nodes))
                    yield return internalNode;
            }
        }
    }
}
