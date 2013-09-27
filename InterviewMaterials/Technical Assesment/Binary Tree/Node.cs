namespace Technical_Assesment
{
    public class Node<T>
    {

        #region Fields

        public T Value { get; set; }
        public Node<T> parent { get; set; }
        public Node<T> left { get; set; }
        public Node<T> right { get; set; }

        private bool visited;

        #endregion

        #region Properties

        public bool IsRoot { get { return parent == null; } }

        public bool IsLeaf { get { return left == null && right == null; } }

        #endregion

        #region  Constructor
        public Node() { }

        public Node(T val) : this(val, null)
        {
        }

        public Node(T val, Node<T> par) : this(val, par, null, null)
        {
        }

        public Node(T val, Node<T> par, Node<T> l, Node<T> r)
        {
            Value = val;

            if (par != null)
            {
                parent = par;
            }

            if(l != null){
                left = l;
            }

            if(r != null){
                right = r;
            }
        }

        #endregion
    }
}
