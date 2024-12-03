namespace Distributed.Consensus.Raft.LeaderElection

module LeaderElection =
    let private (|LessThan|Equals|GreaterThan|) (term1: ElectionTerm, term2: ElectionTerm) =
        match term1.CompareTo term2 with
        | r when r = 0 -> Equals
        | r when r > 0 -> GreaterThan
        | r when r < 0 -> LessThan
        // Represents impossible case, however covered so that IDE doesn't highlight this pattern match expression as
        // incomplete
        | r -> failwith $"Unexpected result %i{r} from ComapreTo(%i{term1},%i{term2}) operation detected"

    let private getNewTerm nodeState =
        1
        + match nodeState with
          | Leader _ -> 0
          | Follower followerInfo -> followerInfo.electionTerm
          | Candidate candidateInfo -> candidateInfo.electionTerm

    let private getLastLogIndex nodeState =
        match nodeState with
        | Leader _ -> 0
        | Follower followerInfo -> followerInfo.lastLogIndex
        | Candidate candidateInfo -> candidateInfo.lastLogIndex

    let private getQuorum numberOfNodes = (numberOfNodes / 2) + 1

    let startNewElectionTerm notifyCandidacy (nodeState, nodeId) =
        match nodeState with
        | Leader _ -> nodeState // Leader doesn't start new election
        | Candidate _ // If candidate doesn't collect the majority of votes it will start new election term
        | Follower _ ->
            let candidateInfo =
                { nodeId = nodeId
                  electionTerm = getNewTerm nodeState
                  votes = 1 // votes for itself as a candidate
                  lastLogIndex = getLastLogIndex nodeState }

            notifyCandidacy candidateInfo
            Candidate candidateInfo

    let vote notifyAcceptVote (nodeState: NodeState, candidate: CandidateInfo) =
        match nodeState with
        | Leader _
        | Candidate _ -> nodeState // Leader and Candidate cannot vote?
        | Follower fi ->
            let candidateIsUpToDate = candidate.lastLogIndex >= getLastLogIndex nodeState

            let nodePermittedToVote =
                match fi.votedFor, (candidate.electionTerm, fi.electionTerm) with
                | Some candidate, Equals -> false // Node already voted in this election term
                | None, Equals -> true // Let it vote in case it didn't yet vote in current term
                | _, LessThan -> false // Candidate tries to start election with outdated election term
                | _, GreaterThan -> true // If candidate starts new valid election term - then node permitted to vote

            if candidateIsUpToDate && nodePermittedToVote then
                notifyAcceptVote candidate

                Follower
                    { fi with
                        votedFor = Some candidate
                        electionTerm = candidate.electionTerm }
            else
                nodeState

    let tryBecomeLeader (notifyNodeBecomeLeader) (nodeState: NodeState, numberOfNodes: int, receivedNewVote: bool) =
        let quorum = getQuorum numberOfNodes

        match nodeState with
        | Leader _ -> nodeState
        | Follower _ -> nodeState
        | Candidate ci ->
            let newVotes = ci.votes + (if receivedNewVote then 1 else 0)

            printf $"Node %s{ci.nodeId} received new vote. Total votes: %i{newVotes}"
            
            if newVotes >= quorum then
                let leaderInfo = { nodeId = ci.nodeId }
                
                printf $"Node %s{ci.nodeId} became a leader"
                
                notifyNodeBecomeLeader leaderInfo
                Leader leaderInfo
            else
                Candidate { ci with votes = newVotes }

    let acknowledgeLeaderHeartbeat (nodeState: NodeState, leader: LeaderInfo) =
        match nodeState with
        | Leader _ -> nodeState // TODO: cover leader heartbeat to another leader
        | Follower fi -> Follower { fi with leader = Some leader }
        | Candidate ci ->
            Follower
                { leader = Some leader
                  votedFor = None
                  electionTerm = ci.electionTerm
                  lastLogIndex = ci.lastLogIndex }
