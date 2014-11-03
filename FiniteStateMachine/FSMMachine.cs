using System.Collections.Generic;

public class FSMMachine
{
    private FSMState currentFSMState = null;
	private StateID defaultStateId = StateID.NONE;
	private List<FSMState> allFSMStates = null;

	public FSMMachine()
	{
		allFSMStates = new List<FSMState>();
	}

    public void AddState(FSMState kState)
    {
        this.allFSMStates.Add(kState);
    }

    public FSMState GetCurrentFSMState()
    {
        return this.currentFSMState;
    }

	public bool GotoFSMState(StateID stateId)
	{
		FSMState fsmState = this.GetFSMState(stateId);
		if(fsmState == null)
		{
			return false;
		}
		if(this.currentFSMState != null)
		{
			this.currentFSMState.Exit();
		}
		this.currentFSMState = fsmState;
		this.currentFSMState.Enter();
		return true;
	}

	public FSMState GetFSMState(StateID stateId)
    {
		int numFSMStates = allFSMStates.Count;
		for(int i = 0; i < numFSMStates; ++i)
        {
			FSMState fsmState = allFSMStates[i];
			if (fsmState != null && fsmState.GetStateId() == stateId)
            {
                return fsmState;
            }
        }
        return null;
    }

    public void SetDefaultStateId(StateID stateId)
    {
		this.defaultStateId = stateId;
    }

    public void UpdateFSMMachine(float fDelta)
    {
        if (this.allFSMStates.Count != 0)
        {
            if (this.currentFSMState == null)
            {
                this.currentFSMState = this.GetFSMState(this.defaultStateId);
				if(this.currentFSMState != null)
				{
                	this.currentFSMState.Enter();
				}
            }
            if (this.currentFSMState != null)
            {
                StateID stateId = this.currentFSMState.GetStateId();
                StateID transitionStateId = this.currentFSMState.CheckTransitions();
				if (transitionStateId != stateId)
                {
                    FSMState fsmState = this.GetFSMState(transitionStateId);
                    if (fsmState != null)
                    {
                        this.currentFSMState.Exit();
                        this.currentFSMState = fsmState;
                        this.currentFSMState.Enter();
                    }
                }
                this.currentFSMState.Update(fDelta);
            }
        }
    }
}

