using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Models
{
    public class TreeNode<T>
    {
        public T Name { get; set; }
        public TreeNode<T> Parent { get; set; }
        public List<TreeNode<T>> Children { get; set; }
        public int GetHeight()
        {
            int height = 1;
            TreeNode<T> cureent = this;
            while (cureent.Parent != null)
            {
                height++;
                cureent = cureent.Parent;
            }
            return height;
        }
    }
}
