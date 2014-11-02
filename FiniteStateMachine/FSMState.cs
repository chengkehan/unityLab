public class FSMState
{
    private StateID stateId;
    private FSMControl fsmControl;

    public FSMState(StateID stateId, FSMControl fsmControl)
    {
		this.stateId = stateId;
		this.fsmControl = fsmControl;
    }

    public virtual StateID CheckTransitions()
    {
        return this.stateId;
    }

    public virtual void Enter()
    {
		// Override in sub class
    }

    public virtual void Exit()
    {
		// Override in sub class
    }

    public StateID GetStateId()
    {
        return this.stateId;
    }

	public FSMControl GetFSMControl()
	{
		return this.fsmControl;
	}

    public virtual void Update(float fDelta)
    {
		// Override in sub class
    }
}

