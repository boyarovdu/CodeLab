namespace Distributed.Consensus.Raft.LeaderElection

module LeaderElection =
    let private getNewTerm currentTerm = currentTerm + 1

    let private getLastLogIndex (nodeState) =
        match nodeState with
        | Leader -> 0
        | Follower followerInfo -> followerInfo.lastLogIndex
        | Candidate candidateInfo -> candidateInfo.lastLogIndex

    let startNewElectionTerm (nodeState, currentTerm, nodeId) =
        match nodeState with
        | Leader -> nodeState // Leader doesn't start new election
        | Candidate _
        | Follower _ ->
            let candidateInfo =
                {
                  nodeId = nodeId
                  electionTerm = getNewTerm currentTerm
                  votes = 1 // votes for itself as a candidate
                  lastLogIndex = getLastLogIndex nodeState }

            Candidate candidateInfo

    let vote (nodeState: NodeState, candidate: CandidateInfo, triggerAcceptVote) =
        match nodeState with
        | Leader
        | Candidate _ -> nodeState // Leader and Candidate cannot vote?
        | Follower fi ->
            let candidateIsOutdated = getLastLogIndex nodeState > candidate.lastLogIndex
            match fi.votedFor, candidateIsOutdated with
            | None, false ->
                triggerAcceptVote candidate
                Follower { fi with votedFor = Some candidate }
            | _, _ -> nodeState


    let tryBecomeLeader (nodeState: NodeState, quorum: int) =
        match nodeState with
        | Leader -> nodeState
        | Follower _ -> nodeState
        | Candidate _ -> nodeState

    let AcknowledgeLeaderPing (nodeState: NodeState, leader: LeaderInfo) =
        match nodeState with
        | Leader -> nodeState
        | Follower _ -> nodeState
        | Candidate _ -> nodeState
