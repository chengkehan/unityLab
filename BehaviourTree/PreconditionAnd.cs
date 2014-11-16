namespace jcgame
{
	namespace BehaviourTree
	{
		public class PreconditionAnd : Precondition
		{
			private Precondition precondition0 = null;

			private Precondition precondition1 = null;

			public PreconditionAnd (Precondition precondition0, Precondition precondition1)
			{
				this.precondition0 = precondition0;
				this.precondition1 = precondition1;
			}

			public override bool Evaluate (object input)
			{
				return 
					this.precondition0 != null && this.precondition0.Evaluate(input) && 
					this.precondition1 != null && this.precondition1.Evaluate(input);
			}
		}
	}
}

