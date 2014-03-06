using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	internal interface ILoadOnDemandTreeOwner<T>
	{
		T CreateValue( int index );
	}

	internal interface IBinaryTreeNodeOwner<T>
	{





		bool RecalculateHeight();







		bool VerifyBalanceOfChild( LoadOnDemandTree<T>.BinaryTreeNode node );
	}

	internal class LoadOnDemandTree<T> :
		ICollection<T>,
		IBinaryTreeNodeOwner<T>
	{
		#region Constants

		// MD 7/23/10 - TFS35969
		// Made internal to be used in other places.
		//// MD 9/24/09 - TFS19150
		//private const int BTreeLoadFactor = 32;
		internal const int BTreeLoadFactor = 32;

		#endregion Constants

		#region Member Variables

		private ILoadOnDemandTreeOwner<T> owner;
		private int count;

		// MD 7/23/10 - TFS35969
		// Made protected to be used in derived classes.
		//private BinaryTreeNode head;
		protected BinaryTreeNode head;

		private BinaryTreeNode lastAccessedNode;

		#endregion Member Variables

		#region Constructor

		public LoadOnDemandTree( ILoadOnDemandTreeOwner<T> owner )
		{
			this.owner = owner;
		}

		#endregion Constructor

		#region Interfaces

		#region IBinaryTreeNodeOwner<T> Members

		// MD 1/24/08
		// Made changes to allow for VS2008 style unit test accessors
		//bool IBinaryTreeNodeOwner<T>.RecalculateHeight()
		public bool RecalculateHeight()
		{
			return false;
		}

		// MD 1/24/08
		// Made changes to allow for VS2008 style unit test accessors
		//bool IBinaryTreeNodeOwner<T>.VerifyBalanceOfChild( LoadOnDemandTree<T>.BinaryTreeNode node )
		public bool VerifyBalanceOfChild( LoadOnDemandTree<T>.BinaryTreeNode node )
		{
			Debug.Assert( node == this.head );
			return BinaryTreeNode.EnsureBalance( ref this.head );
		}

		#endregion

		#region ICollection<TValue> Members

		void ICollection<T>.Add( T item )
		{
			Utilities.DebugFail( "Items cannot be added directly" );
		}

		void ICollection<T>.Clear()
		{
			this.head = null;
			this.count = 0;
		}

		bool ICollection<T>.Contains( T item )
		{
			foreach ( T value in this )
			{
				if ( value.Equals( item ) )
					return true;
			}

			return false;
		}

		void ICollection<T>.CopyTo( T[] array, int arrayIndex )
		{
			foreach ( T value in this )
				array[ arrayIndex++ ] = value;
		}

		int ICollection<T>.Count
		{
			get { return this.count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<T>.Remove( T item )
		{
			Utilities.DebugFail( "Items cannot be removed directly" );
			return false;
		}

		#endregion

		#region IEnumerable<TValue> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			// MD 12/1/11 - TFS96113
			// Moved all code to the new overload.
			// MD 2/23/12 - 12.1 - Table Support
			// Pass in True for the new enumerateForwards parameter.
			//return this.GetEnumeratorHelper(0, Int32.MaxValue).GetEnumerator();
			return this.GetEnumeratorHelper(0, Int32.MaxValue, true).GetEnumerator();
		}

		// MD 12/1/11 - TFS96113
		// Added a way to enumerate only nodes in a certain range.
		// MD 2/23/12 - 12.1 - Table Support
		// Added an enumerateForwards parameter.
		//public IEnumerable<T> GetEnumeratorHelper(int startIndex, int endIndex)
		public IEnumerable<T> GetEnumeratorHelper(int startIndex, int endIndex, bool enumerateForwards)
		{
			// MD 7/23/10 - TFS35969
			// Moved the code that enumerates over all nodes in order to the GetNodesEnumerator method so it 
			// could be used in other places.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)


			#endregion // Old Code
			// MD 12/1/11 - TFS96113
			// Pass off the start and end indexes to the GetNodesEnumerator so we only get nodes which contain items with those indexes.
			//using (IEnumerator<LoadOnDemandTree<T>.BinaryTreeNode> enumerator = this.GetNodesEnumerator())
			// MD 2/23/12 - 12.1 - Table Support
			//foreach (BinaryTreeNode currentNode in this.GetNodesEnumerator(startIndex, endIndex))
			foreach (BinaryTreeNode currentNode in this.GetNodesEnumerator(startIndex, endIndex, enumerateForwards))
			{
				T[] values = currentNode.Values;

				// MD 12/1/11 - TFS96113
				// We may not iterate all items here.
				//for (int i = 0; i < LoadOnDemandTree<T>.BTreeLoadFactor; i++)
				int nodeStartIndex = 0;
				if (currentNode.FirstItemIndex < startIndex)
					nodeStartIndex = startIndex - currentNode.FirstItemIndex;

				// 1/11/12 - TFS99216
				// This was incorrect and was returning the wrong end index relative to the node so we were not iterating all items in the range.
				//int nodeEndIndex = BTreeLoadFactor - 1;
				//int absoluteEndIndex = currentNode.FirstItemIndex + nodeEndIndex;
				//if (endIndex < absoluteEndIndex)
				//    nodeEndIndex = absoluteEndIndex - endIndex;
				int nodeEndIndex = Math.Min(endIndex - currentNode.FirstItemIndex, BTreeLoadFactor - 1);

				// MD 2/16/12 - 12.1 - Table Support
				//for (int i = nodeStartIndex; i <= nodeEndIndex; i++)
				//{
				//    T value = values[i];
				//    if (value != null)
				//        yield return value;
				//}
				if (enumerateForwards)
				{
					for (int i = nodeStartIndex; i <= nodeEndIndex; i++)
					{
						T value = values[i];
						if (value != null)
							yield return value;
					}
				}
				else
				{
					for (int i = nodeEndIndex; i >= nodeStartIndex; i--)
					{
						T value = values[i];
						if (value != null)
							yield return value;
					}
				}
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable<T>)this ).GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		// MD 7/23/10 - TFS35969
		// Added a virtual method to create nodes so derived classes could create dervied nodes.
		#region CreateNode

		internal virtual BinaryTreeNode CreateNode(int nodeStartIndex, IBinaryTreeNodeOwner<T> owner)
		{
			return new BinaryTreeNode(nodeStartIndex, owner, this);
		} 

		#endregion // CreateNode

		// MD 7/23/10 - TFS35969
		// Added a way to enumerate over all nodes in order.
		#region GetNodesEnumerator

		protected IEnumerable<LoadOnDemandTree<T>.BinaryTreeNode> GetNodesEnumerator()
		{
			// MD 12/1/11 - TFS96113
			// Moved all code to the new overload.
			// MD 2/23/12 - 12.1 - Table Support
			// Pass True for the new enumerateForwards parameter.
			//return this.GetNodesEnumerator(0, Int32.MaxValue);
			return this.GetNodesEnumerator(0, Int32.MaxValue, true);
		}

		// MD 12/1/11 - TFS96113
		// Added a way to enumerate only nodes in a certain range.
		// MD 2/23/12 - 12.1 - Table Support
		// Added an enumerateForwards parameter.
		//protected IEnumerable<LoadOnDemandTree<T>.BinaryTreeNode> GetNodesEnumerator(int startIndex, int endIndex)
		protected IEnumerable<LoadOnDemandTree<T>.BinaryTreeNode> GetNodesEnumerator(int startIndex, int endIndex, bool enumerateForwards)
		{
			Stack<LoadOnDemandTree<T>.BinaryTreeNode> valueNodes = new Stack<LoadOnDemandTree<T>.BinaryTreeNode>();
			LoadOnDemandTree<T>.BinaryTreeNode nextNode = this.head;

			while (true)
			{
				// Push the next node and all nodes in its left branch onto the stack of value nodes
				// MD 2/16/12 - 12.1 - Table Support
				// Pass a parameter to determine whether to push the left or right branch.
				//LoadOnDemandTree<T>.PushNodeAndRecursiveBranch(nextNode, valueNodes);
				LoadOnDemandTree<T>.PushNodeAndRecursiveBranch(nextNode, valueNodes, enumerateForwards);

				// If the stack is empty, all items have been enumerated, end the enumerator
				if (valueNodes.Count == 0)
					yield break;

				// Pop the next node off the stack and return its value
				LoadOnDemandTree<T>.BinaryTreeNode currentNode = valueNodes.Pop();

				// MD 12/1/11 - TFS96113
				// Added a way to enumerate only nodes in a certain range.
				//yield return currentNode;
				if (startIndex < (currentNode.FirstItemIndex + BTreeLoadFactor) && currentNode.FirstItemIndex <= endIndex)
					yield return currentNode;

				// The next node to be added to the stack will be the current node's right child (and all 
				// the nodes in its left branch).
				// MD 2/16/12 - 12.1 - Table Support
				// Use the opposite nodes if we should enumerate backwards.
				//nextNode = currentNode.RightChild;
				if (enumerateForwards)
					nextNode = currentNode.RightChild;
				else
					nextNode = currentNode.LeftChild;
			}
		} 

		#endregion // GetNodesEnumerator

		#region GetValue

		// MD 10/20/10 - TFS36617
		// Made this internal so it could be used outside this class.
		//private T GetValue( int index, bool createdIfNotFound, out FindState state )
		internal T GetValue(int index, bool createdIfNotFound, out FindState state)
		{
			// MD 9/24/09 - TFS19150
			// Now each node will hold multiple values, so find the start index in the node and the index into the node.
			int indexInNode = index % LoadOnDemandTree<T>.BTreeLoadFactor;
			int nodeStartIndex = index - indexInNode;

			// Get the node continaing the value needed
			BinaryTreeNode node = null;
			if ( this.head == null )
			{
				// MD 11/1/10
				// Found while fixing TFS56976
				// We were always creating the head node and ignoring the createdIfNotFound parameter. Now it is honored.
				if (createdIfNotFound == false)
				{
					state = LoadOnDemandTree<T>.FindState.ValueNotFound;
					return default(T);
				}

				state = FindState.ValueInserted;

				// MD 9/24/09 - TFS19150
				// Now each node will hold multiple values.
				//node = this.head = new BinaryTreeNode( index, this.owner.CreateValue( index ), this );
				// MD 7/23/10 - TFS35969
				// Use the new virtual method to create the nodes.
				//node = this.head = new BinaryTreeNode<T>( nodeStartIndex, this );
				node = this.head = this.CreateNode(nodeStartIndex, this);

				node.Values[ indexInNode ] = this.owner.CreateValue( index );
			}
			else
			{
				if ( this.lastAccessedNode != null )
				{
					// MD 9/24/09 - TFS19150
					// Check to see if we need the same node that was last accessed.
					if ( nodeStartIndex == this.lastAccessedNode.FirstItemIndex )
					{
						node = this.lastAccessedNode;
					}
					// MD 9/24/09 - TFS19150
					// Now each node will hold multiple values, check with the start index in the node.
					//if ( index == this.lastAccessedNode.FirstItemIndex + 1 )
					else if ( nodeStartIndex == this.lastAccessedNode.FirstItemIndex + LoadOnDemandTree<T>.BTreeLoadFactor )
					{
						#region Perform shortcut for 1-greater search

						
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


						// First get the right child of the last accessed node
						node = this.lastAccessedNode.RightChild;

						if ( node != null )
						{
							// If the right child exists, find the left-most descendant of it.
							// If the left-most descendant is not the node we are looking for, it will become
							// the node to start the default search from, because the new value will be inserted as
							// its left child.
							while ( true )
							{
								BinaryTreeNode leftChild = node.LeftChild;

								if ( leftChild == null )
									break;

								node = leftChild;
							}
						}
						else
						{
							
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)

						}

						#endregion Perform shortcut for 1-greater search
					}
					// MD 9/24/09 - TFS19150
					// Now each node will hold multiple values, check with the start index in the node.
					//else if ( index == this.lastAccessedNode.FirstItemIndex - 1 )
					else if ( nodeStartIndex == this.lastAccessedNode.FirstItemIndex - LoadOnDemandTree<T>.BTreeLoadFactor )
					{
						#region Perform shortcut for 1-less search

						
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


						// First get the left child of the last accessed node
						node = this.lastAccessedNode.LeftChild;

						if ( node != null )
						{
							// If the left child exists, find the right-most descendant of it.
							// If the right-most descendant is not the node we are looking for, it will become
							// the node to start the default search from, because the new value will be inserted as
							// its right child.
							while ( true )
							{
								BinaryTreeNode rightChild = node.RightChild;

								if ( rightChild == null )
									break;

								node = rightChild;
							}
						}
						else
						{
							
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)

						}

						#endregion Perform shortcut for 1-less search
					}
				}

				if ( node == null )
					node = this.head;

				node = node.FindOrAdd( index, createdIfNotFound, this, out state );
			}

			// If the node is null, return the default for the value type
			if ( node == null )
				return default( T );

			this.lastAccessedNode = node;

			// If a node was returned, but it did not previously exist in the tree, increment the count.
			if ( state == FindState.ValueInserted )
				this.count++;

			// MD 9/24/09 - TFS19150
			// Now each node will hold multiple values, so get the item from the offset index in the ndoe.
			//return node.Value;
			return node.Values[ indexInNode ];
		}

		#endregion GetValue

		#region PushNodeAndRecursiveBranch

		// MD 2/16/12 - 12.1 - Table Support
		// Renamed and added a parameter so we could push either branch.
		//private static void PushNodeAndLeftBranch( BinaryTreeNode node, Stack<BinaryTreeNode> valueNodes )
		private static void PushNodeAndRecursiveBranch(BinaryTreeNode node, Stack<BinaryTreeNode> valueNodes, bool useLeftBranch)
		{
			while ( true )
			{
				if ( node == null )
					break;

				// Push the current node
				valueNodes.Push( node );

				// Traverse the left branch of the current node so the node in that 
				// branch can also be pushed.
				//node = node.LeftChild;
				// MD 2/16/12 - 12.1 - Table Support
				if (useLeftBranch)
					node = node.LeftChild;
				else
					node = node.RightChild;
			}
		}

		#endregion PushNodeAndRecursiveBranch

		#region TryGetValue

		public bool TryGetValue( int index, out T value )
		{
			// Try to get the value in the tree
			FindState state;
			value = this.GetValue( index, false, out state );

			// Return true if the value existed in the tree
			return state == FindState.ValueFound;
		}

		#endregion TryGetValue

		#endregion Methods

		#region Properties

		#region Count

		public int Count
		{
			get { return this.count; }
		}

		#endregion Count

		#region Indexer[ int ]

		public T this[ int index ]
		{
			get
			{
				// Get the value in the tree, force it to be created if it does not exist
				FindState state;
				return this.GetValue( index, true, out state );
			}
		}

		#endregion Indexer[ TKey ]

		#endregion Properties


		#region BinaryTreeNode class



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal class BinaryTreeNode :
			IBinaryTreeNodeOwner<T>
		{
			#region Member Variables

			private int height;

			// MD 9/24/09 - TFS19150
			// Renamed for clarity becasue not the node holds multiple items.
			//private int index;
			private int firstItemIndex;

			private IBinaryTreeNodeOwner<T> parent;

			private BinaryTreeNode leftChild;
			private BinaryTreeNode rightChild;

			// MD 9/24/09 - TFS19150
			// Now each node will hold multiple values.
			//private T value;
			// MD 7/23/10 - TFS35969
			// Made protected to be used in dervied classes.
			//private T[] values;
			protected T[] values;

			// MD 7/23/10 - TFS35969
			// Gave the nodes a reference back to the tree.
			protected LoadOnDemandTree<T> tree;

			#endregion Member Variables

			#region Constructor

			// MD 9/24/09 - TFS19150
			// Now each node will hold multiple values, so the value shouldn't be passed into the constructor.
			//public BinaryTreeNode( int index, T value, IBinaryTreeNodeOwner<T> parent )
			// MD 7/23/10 - TFS35969
			// The constructor now takes a reference back to the tree.
			//public BinaryTreeNode(int index, IBinaryTreeNodeOwner<T> parent)
			public BinaryTreeNode(int index, IBinaryTreeNodeOwner<T> parent, LoadOnDemandTree<T> tree)
			{
				this.firstItemIndex = index;

				// MD 9/24/09 - TFS19150
				// Now each node will hold multiple values.
				//this.value = value;
				this.values = new T[ LoadOnDemandTree<T>.BTreeLoadFactor ];

				this.parent = parent;

				// MD 7/23/10 - TFS35969
				this.tree = tree;
			}

			#endregion Constructor

			#region Interfaces

			#region IBinaryTreeNodeOwner<T> Members

			// MD 1/24/08
			// Made changes to allow for VS2008 style unit test accessors
			//bool IBinaryTreeNodeOwner<T>.RecalculateHeight()
			//{
			//    return this.RecalculateHeight();
			//}

			// MD 1/24/08
			// Made changes to allow for VS2008 style unit test accessors
			//bool IBinaryTreeNodeOwner<T>.VerifyBalanceOfChild( LoadOnDemandTree<T>.BinaryTreeNode node )
			public bool VerifyBalanceOfChild( LoadOnDemandTree<T>.BinaryTreeNode node )
			{
				if ( node == this.leftChild )
					return BinaryTreeNode.EnsureBalance( ref this.leftChild );
				else if ( node == this.rightChild )
					return BinaryTreeNode.EnsureBalance( ref this.rightChild );
				else
				{
					Utilities.DebugFail( "Invalid child node was passed in." );
					return false;
				}
			}

			#endregion

			#endregion Interfaces

			#region Methods

			#region EnsureBalance

			internal static bool EnsureBalance( ref BinaryTreeNode parentNode )
			{
				BinaryTreeNode leftChildNode = parentNode.leftChild;
				BinaryTreeNode rightChildNode = parentNode.rightChild;

				int leftChildHeight = GetNodeHeight( leftChildNode );
				int rightChildHeight = GetNodeHeight( rightChildNode );

				if ( leftChildHeight < rightChildHeight - 1 )
				{
					BinaryTreeNode leftGrandChildNode = rightChildNode.leftChild;
					BinaryTreeNode rightGrandChildNode = rightChildNode.rightChild;

					int leftGrandChildHeight = GetNodeHeight( leftGrandChildNode );
					int rightGrandChildHeight = GetNodeHeight( rightGrandChildNode );

					if ( leftGrandChildHeight < rightGrandChildHeight )
					{
						#region Perform Right-Right Rotaion

						
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


						BinaryTreeNode B = rightChildNode.leftChild;

						// Set the new parent of the old right child
						rightChildNode.parent = parentNode.parent;

						// Reassign children
						parentNode.RightChild = B;
						rightChildNode.LeftChild = parentNode;

						// Recalculate the height of the old parent, which is the only height that should change
						parentNode.RecalculateHeight();

						// Return the old right child node as the new parent in this sub-tree
						parentNode = rightChildNode;

						#endregion Perform Right-Right Rotaion
					}
					else
					{
						#region Perform Right-Left Rotation

						
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


						BinaryTreeNode B = leftGrandChildNode.leftChild;
						BinaryTreeNode C = leftGrandChildNode.rightChild;

						// Set the new parent of the old left grandchild child
						leftGrandChildNode.parent = parentNode.parent;

						// Reassign children
						leftGrandChildNode.LeftChild = parentNode;
						leftGrandChildNode.RightChild = rightChildNode;
						parentNode.RightChild = B;
						rightChildNode.LeftChild = C;

						// Recalculate the height of the nodes that would have changed
						parentNode.RecalculateHeight();
						rightChildNode.RecalculateHeight();
						leftGrandChildNode.RecalculateHeight();

						// Return the old left grand child node as the new parent in this sub-tree
						parentNode = leftGrandChildNode;

						#endregion Perform Right-Left Rotation
					}

					return true;
				}

				if ( rightChildHeight < leftChildHeight - 1 )
				{
					BinaryTreeNode leftGrandChildNode = leftChildNode.leftChild;
					BinaryTreeNode rightGrandChildNode = leftChildNode.rightChild;

					int leftGrandChildHeight = GetNodeHeight( leftGrandChildNode );
					int rightGrandChildHeight = GetNodeHeight( rightGrandChildNode );

					if ( rightGrandChildHeight < leftGrandChildHeight )
					{
						#region Perform Left-Left Rotation

						
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


						BinaryTreeNode B = leftChildNode.rightChild;

						// Set the new parent of the old left child
						leftChildNode.parent = parentNode.parent;

						// Reassign children
						parentNode.LeftChild = B;
						leftChildNode.RightChild = parentNode;

						// Recalculate the height of the old parent, which is the only height that should change
						parentNode.RecalculateHeight();

						// Return the old left child node as the new parent in this sub-tree
						parentNode = leftChildNode;

						#endregion Perform Left-Left Rotation
					}
					else
					{
						#region Perform Left-Right Rotation

						
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


						BinaryTreeNode B = rightGrandChildNode.leftChild;
						BinaryTreeNode C = rightGrandChildNode.rightChild;

						// Set the new parent of the old right grandchild child
						rightGrandChildNode.parent = parentNode.parent;

						// Reassign children
						rightGrandChildNode.LeftChild = leftChildNode;
						rightGrandChildNode.RightChild = parentNode;
						parentNode.LeftChild = C;
						leftChildNode.RightChild = B;

						// Recalculate the height of the nodes that would have changed
						parentNode.RecalculateHeight();
						leftChildNode.RecalculateHeight();
						rightGrandChildNode.RecalculateHeight();

						// Return the old right grand child node as the new parent in this sub-tree
						parentNode = rightGrandChildNode;

						#endregion Perform Left-Right Rotation
					}

					return true;
				}

				return false;
			}

			#endregion EnsureBalance

			#region FindOrAdd

			public BinaryTreeNode FindOrAdd(
				int index,
				bool createdIfNotFound,
				LoadOnDemandTree<T> tree,
				out FindState state )
			{
				// MD 9/24/09 - TFS19150
				// Now each node will hold multiple values, so find the start index in the node and the index into the node.
				int indexInNode = index % LoadOnDemandTree<T>.BTreeLoadFactor;
				int nodeStartIndex = index - indexInNode;

				state = FindState.ValueNotFound;
				BinaryTreeNode currentNode = this;
				BinaryTreeNode returnNode = null;

				while ( true )
				{
					// If the index to search for matches this value's index
					// MD 9/24/09 - TFS19150
					// Now each node will hold multiple values, check with the start index in the node.
					//if ( index == currentNode.index )
					if ( nodeStartIndex == currentNode.firstItemIndex )
					{
						// MD 9/24/09 - TFS19150
						// The state is not always ValueFound now since the node hold multiple items and the value might 
						// not be present in the node yet.
						//state = FindState.ValueFound;
						if ( currentNode.Values[ indexInNode ] == null )
						{
							if ( createdIfNotFound )
							{
								currentNode.Values[ indexInNode ] = tree.owner.CreateValue( index );
								state = FindState.ValueInserted;
							}
							else
							{
								state = FindState.ValueNotFound;
							}
						}
						else
						{
							state = FindState.ValueFound;
						}
						
						return currentNode;
					}

					BinaryTreeNode parentNode = currentNode;

					// MD 9/24/09 - TFS19150
					// Now each node will hold multiple values, check with the start index in the node.
					//if ( index < currentNode.index )
					if ( nodeStartIndex < currentNode.firstItemIndex )
					{
						currentNode = currentNode.leftChild;

						if ( currentNode == null )
						{
							if ( createdIfNotFound )
							{
								state = FindState.ValueInserted;

								// MD 9/24/09 - TFS19150
								// Now each node will hold multiple values.
								//returnNode = new BinaryTreeNode( index, tree.owner.CreateValue( index ), this );
								// MD 7/23/10 - TFS35969
								// Use the new virtual method to create the nodes.
								//returnNode = new BinaryTreeNode( nodeStartIndex, this );
								returnNode = tree.CreateNode(nodeStartIndex, this);

								returnNode.Values[ indexInNode ] = tree.owner.CreateValue( index );

								currentNode = returnNode;
								parentNode.LeftChild = currentNode;
							}

							break;
						}
					}
					else
					{
						currentNode = currentNode.rightChild;

						if ( currentNode == null )
						{
							if ( createdIfNotFound )
							{
								state = FindState.ValueInserted;

								// MD 9/24/09 - TFS19150
								// Now each node will hold multiple values.
								//returnNode = new BinaryTreeNode( index, tree.owner.CreateValue( index ), this );
								// MD 7/23/10 - TFS35969
								// Use the new virtual method to create the nodes.
								//returnNode = new BinaryTreeNode( nodeStartIndex, this );
								returnNode = tree.CreateNode(nodeStartIndex, this);

								returnNode.Values[ indexInNode ] = tree.owner.CreateValue( index );

								currentNode = returnNode;
								parentNode.RightChild = currentNode;
							}

							break;
						}
					}
				}

				// If the value had to be created, recalculate the node height and possibly rebalance the tree.
				if ( state == FindState.ValueInserted )
				{
					IBinaryTreeNodeOwner<T> parent = currentNode.parent;

					while ( currentNode != null )
					{
						parent = currentNode.parent;

						// If we perform a balance, the heights above will not change, so break out of the loop
						if ( parent.VerifyBalanceOfChild( currentNode ) )
							break;

						// If the height of the current node doesn't change, no node above it will change, 
						// so break out of the loop
						if ( parent.RecalculateHeight() == false )
							break;

						Debug.Assert( Math.Abs( GetNodeHeight( currentNode.rightChild ) - GetNodeHeight( currentNode.leftChild ) ) <= 1 );
						Debug.Assert( currentNode.height == Math.Max( GetNodeHeight( currentNode.rightChild ), GetNodeHeight( currentNode.leftChild ) ) + 1 );
						Debug.Assert( currentNode.leftChild == null || currentNode.rightChild == null || currentNode.leftChild.firstItemIndex < currentNode.rightChild.firstItemIndex );

						currentNode = parent as BinaryTreeNode;
					}
				}

				return returnNode;
			}

			#endregion FindOrAdd

			#region GetNodeHeight

			private static int GetNodeHeight( BinaryTreeNode node )
			{
				return node == null ? -1 : node.height;
			}

			#endregion GetNodeHeight

			#region RecalculateHeight

			public bool RecalculateHeight()
			{
				int leftHeight = GetNodeHeight( this.leftChild );
				int rightHeight = GetNodeHeight( this.rightChild );

				int oldHeight = this.height;

				if ( leftHeight < rightHeight )
					this.height = rightHeight + 1;
				else
					this.height = leftHeight + 1;

				return this.height != oldHeight;
			}

			#endregion RecalculateHeight

			#endregion Methods

			#region Properties

			#region FirstItemIndex

			// MD 9/24/09 - TFS19150
			// Renamed for clarity becasue not the node holds multiple items.
			//public int Index
			public int FirstItemIndex
			{
				get { return this.firstItemIndex; }
			}

			#endregion FirstItemIndex

			#region LeftChild

			public BinaryTreeNode LeftChild
			{
				get { return this.leftChild; }
				set
				{
					if ( this.leftChild != value )
					{
						this.leftChild = value;

						if ( this.leftChild != null )
							this.leftChild.parent = this;
					}
				}
			}

			#endregion LeftChild

			#region Parent

			public IBinaryTreeNodeOwner<T> Parent
			{
				get { return this.parent; }
			}

			#endregion Parent

			#region RightChild

			public BinaryTreeNode RightChild
			{
				get { return this.rightChild; }
				set
				{
					if ( this.rightChild != value )
					{
						this.rightChild = value;

						if ( this.rightChild != null )
							this.rightChild.parent = this;
					}
				}
			}

			#endregion RightChild

			// MD 9/24/09 - TFS19150
			// Now each node will hold multiple values.
			#region Old Code

			//#region Value
			//
			//public T Value
			//{
			//    get { return this.value; }
			//}
			//
			//#endregion Value

			#endregion Old Code
			#region Values

			public T[] Values
			{
				get { return this.values; }
			}

			#endregion Values

			#endregion Properties
		}

		#endregion BinaryTreeNode class

		#region FindState enum

		internal enum FindState
		{
			ValueNotFound,
			ValueFound,
			ValueInserted,
		}

		#endregion FindState enum
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved