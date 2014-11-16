namespace jcgame
{
	namespace BehaviourTree
	{
		public class BehaviourNodeRandomSelector : BehaviourNode
		{
			protected override bool EvaluateThenExecuteInner (object input)
			{
				if(this.numChildren == 0)
				{
					return false;
				}

				System.Random random = new System.Random();
				int currentChildNodeIndex = random.Next() % this.numChildren;
				BehaviourNode behaviourNode = this.children[currentChildNodeIndex];
				return behaviourNode != null && behaviourNode.enabled && behaviourNode.EvaluateThenExecute(input);
			}
		}
	}
}

