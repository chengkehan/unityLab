namespace jcgame
{
	namespace BehaviourTree
	{
		public class BehaviourNodeParallelSelector : BehaviourNode
		{
			protected override bool EvaluateThenExecuteInner (object input)
			{
				if(this.numChildren == 0)
				{
					return false;
				}

				for(int i = 0; i < this.numChildren; ++i)
				{
					BehaviourNode behaviourNode = this.children[i];
					if(behaviourNode != null && behaviourNode.enabled)
					{
						behaviourNode.EvaluateThenExecute(input);
					}
				}
				return true;
			}
		}
	}
}

