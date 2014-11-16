namespace jcgame
{
	namespace BehaviourTree
	{
		public class PreconditionAlways : Precondition
		{
			public override bool Evaluate (object input)
			{
				return true;
			}
		}
	}
}

