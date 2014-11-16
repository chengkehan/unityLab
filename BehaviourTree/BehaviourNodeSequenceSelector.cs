namespace jcgame
{
	namespace BehaviourTree
	{
		public class BehaviourNodeSequenceSelector : BehaviourNode
		{
			private int currentChildNodeIndex = -1;

			protected override bool EvaluateThenExecuteInner (object input)
			{
				if(this.numChildren == 0)
				{
					return false;
				}

				++this.currentChildNodeIndex;
				if(this.currentChildNodeIndex >= this.numChildren)
				{
					this.currentChildNodeIndex = 0;
				}
				if(this.IsLegalChildNodeIndex(this.currentChildNodeIndex))
				{
					BehaviourNode behaviourNode = this.children[this.currentChildNodeIndex];
					if(behaviourNode != null && behaviourNode.enabled && behaviourNode.EvaluateThenExecute(input))
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}

