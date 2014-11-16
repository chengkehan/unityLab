namespace jcgame
{
	namespace BehaviourTree
	{
		public abstract class BehaviourNode
		{
			public const int MAX_CHILD_NODE_AMOUNT = 10;

			protected BehaviourNode[] children = new BehaviourNode[BehaviourNode.MAX_CHILD_NODE_AMOUNT];

			protected int numChildren = 0;

			public string name
			{
				set;
				get;
			}

			public BehaviourNode parent
			{
				set;
				get;
			}

			public bool enabled
			{
				set;
				get;
			}

			public Precondition precondition
			{
				set;
				get;
			}

			public BehaviourNode ()
			{
				this.enabled = true;
			}

			public bool AddChildNode(BehaviourNode childNode)
			{
				if(childNode == null)
				{
					return false;
				}
				if(!this.IsLegalChildNodeIndex(this.numChildren))
				{
					return false;
				}

				this.children[this.numChildren] = childNode;
				++this.numChildren;

				return true;
			}

			public bool EvaluateThenExecute(object input)
			{
				return this.enabled && (this.precondition == null || this.precondition.Evaluate(input)) && this.EvaluateThenExecuteInner(input);
			}

			protected abstract bool EvaluateThenExecuteInner(object input);

			protected bool IsLegalChildNodeIndex(int index)
			{
				return index > 0 && index < BehaviourNode.MAX_CHILD_NODE_AMOUNT;
			}
		}
	}
}

