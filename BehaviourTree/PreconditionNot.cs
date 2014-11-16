namespace jcgame
{
	namespace BehaviourTree
	{
		public class PreconditionNot : Precondition
		{
			private Precondition precondition = null;

			public PreconditionNot (Precondition precondition)
			{
				this.precondition = precondition;
			}

			public override bool Evaluate (object input)
			{
				return this.precondition == null ? false : this.precondition.Evaluate(input);
			}
		}
	}
}

