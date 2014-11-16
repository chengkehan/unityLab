namespace jcgame
{
	namespace BehaviourTree
	{
		public class PreconditionNever : Precondition
		{
			public override bool Evaluate (object input)
			{
				return false;
			}
		}
	}
}

