using System;
using System.Collections.Generic;
using Technical_Assesment.Sorting;

namespace Technical_Assesment
{

    public class BTree<T> // : ICollection<T> where T : IComparable<T>
    {

        #region Properties

        public bool IsEmpty { get { return root == null; } }

        public int NodeCount { get { return nodeCnt; } }

        public ISortable<T> SortRoutine { get; private set; } 

        #endregion

        #region Fields
        
        private Node<T> root;
        private int nodeCnt;

        #endregion

        #region construtor
         
        public BTree(ISortable<T> routine ) : this(null, routine)
        {
        } 

        public BTree(Node<T> r, ISortable<T> routine )
        {
            if (r != null)
            {
                root = r;

                foreach (Node<T> node in Transveral(r))
                {
                    ++nodeCnt;
                }
            }

            SortRoutine = routine;
        }

        #endregion

        #region Private Methods

        private static IEnumerable<Node<T>> Transveral(Node<T> node)
        {
            if (node != null)
            {
                if (node.left != null)
                {
                    foreach (Node<T> leftNode in Transveral(node.left))
                    {
                        yield return leftNode;
                    }
                }

                yield return node;

                if (node.right != null)
                {
                    foreach (Node<T> rightNode in Transveral(node.right))
                    {
                        yield return rightNode;
                    }
                }
            }
        }

        private void Insert(T item)
        {
            Node<T> tmpNode = root;
            bool found = false;

            if (SortRoutine == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("SortRoutine");
                // ReSharper restore NotResolvedInText
            }

            while (!found)
            {
                int cmpVal = SortRoutine.CompareTo(tmpNode.Value, item);

                //int cmpVal = tmpNode.Value.CompareTo(item);

                if (cmpVal < 0)
                {
                    if (tmpNode.left == null)
                    {
                        tmpNode.left = new Node<T>(item, tmpNode);
                        ++nodeCnt;
                        return;
                    }

                    tmpNode = tmpNode.left;
                }
                else if (cmpVal > 0)
                {
                    if (tmpNode.right == null)
                    {
                        tmpNode.right = new Node<T>(item, tmpNode);
                        ++nodeCnt;
                        return;
                    }

                    tmpNode = tmpNode.right;
                }
                else
                {
                    // they are equal, throw and exception ;)
                    throw new ArgumentException("Node's value already exist [ " + tmpNode.Value + " ]");
                    //tmpNode = tmpNode.right;
                }
            }
        }

        private Node<T> Find(T item)
        {
            foreach (Node<T> node in Transveral(root))
            {
                if (node.Value.Equals(item))
                {
                    return node;
                }
            }

            return null;
        }

        #endregion

        public void Add(T item)
        {
            if (root == null)
            {
                root = new Node<T>(item);
                ++nodeCnt;
            }
            else
            {
                Insert(item);
            }
        }

        public bool Contains(T item)
        {
            if (IsEmpty)
            {
                return false;
            }

            if (SortRoutine == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("SortRoutine");
                // ReSharper restore NotResolvedInText
            }
            
            Node<T> tmpNode = root;

            while (tmpNode != null)
            {
                int cmpVal = SortRoutine.CompareTo(tmpNode.Value, item);
                //int cmpVal = tmpNode.Value.CompareTo(item);

                if (cmpVal == 0)
                {
                    return true;
                } 

                if (cmpVal < 0)
                {
                    tmpNode = tmpNode.left;
                }else if(cmpVal > 0){
                    tmpNode = tmpNode.right;
                }
            }

            return false;
        }

        public bool Remove(T item)
        {
            Node<T> node = Find(item);

            if (node == null)
            {
                return false;
            }

            --nodeCnt;

            List<T> vals = new List<T>();
            // find all nodes to keep

            foreach (Node<T> tmpNode in Transveral(node.left))
            {
                vals.Add(tmpNode.Value);
            }

            foreach (Node<T> tmpNode in Transveral(node.right))
            {
                vals.Add(tmpNode.Value);
            }

            if (node.parent != null)
            {
                if (node.parent.left == node)
                {
                    node.parent.left = null;
                }
                else
                {
                    node.parent.right = null;
                }
                node.parent = null;
            }
            else
            {
                root = null;
            }

            // add them back ;)
            foreach (T val in vals)
            {
                Add(val);
            }

            return true;
        }
    }

}
