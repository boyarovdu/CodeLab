namespace Distributed.Consensus.Raft.LeaderElection

module LeaderElection =
    let private (|OldTerm|SameTerm|NewTerm|) (termToCompare: ElectionTerm, otherTerm: ElectionTerm) =
        match termToCompare.CompareTo otherTerm with
        | r when r = 0 -> SameTerm
        | r when r > 0 -> NewTerm
        | r when r < 0 -> OldTerm
        // Represents impossible case, however covered so that IDE doesn't highlight this pattern match expression as
        // incomplete :)
        | r -> failwith $"Unexpected result %i{r} from ComapreTo(%i{termToCompare},%i{otherTerm}) operation detected"

    let private getElectionTerm nodeState =
        match nodeState with
        | Leader li -> li.electionTerm
        | Follower followerInfo -> followerInfo.electionTerm
        | Candidate candidateInfo -> candidateInfo.electionTerm
    
    let private getNewTerm nodeState =
        getElectionTerm nodeState
        + 1
     
    let private getLastLogIndex nodeState =
        match nodeState with
        | Leader _ -> 0
        | Follower followerInfo -> followerInfo.lastLogIndex
        | Candidate candidateInfo -> candidateInfo.lastLogIndex

    let private getQuorum numberOfNodes = (numberOfNodes / 2) + 1

    let private isCandidateUpToDate (candidate: CandidateInfo) nodeState =
        candidate.lastLogIndex >= getLastLogIndex nodeState

    let tryStartNewElectionTerm nodeId nodeState =
        match nodeState with
        | Leader _ -> false, nodeState // Leader doesn't start new election
        | Candidate _ // If candidate doesn't collect the majority of votes it will start new election term
        | Follower _ ->
            true,
            Candidate
                { nodeId = nodeId
                  electionTerm = getNewTerm nodeState
                  votes = 1 // votes for itself as a candidate
                  lastLogIndex = getLastLogIndex nodeState }

    let tryVote (candidate: CandidateInfo) (nodeState: NodeState) =
        let handleSameTermVote candidate fi candidateIsUpToDate =
            match fi.votedFor with
            | Some _ -> false, Follower fi
            | None ->
                if candidateIsUpToDate then
                    true, Follower { fi with votedFor = Some candidate }
                else
                    false, Follower fi

        let handleNewTermVote candidate fi candidateIsUpToDate =
            if candidateIsUpToDate then
                true,
                Follower
                    { fi with
                        votedFor = Some candidate
                        electionTerm = candidate.electionTerm }
            else
                false,
                Follower
                    { fi with
                        votedFor = None
                        electionTerm = candidate.electionTerm }

        match nodeState with
        | Leader _
        | Candidate _ -> (false, nodeState) // Leader and Candidate cannot vote
        | Follower fi ->
            let candidateIsUpToDate = isCandidateUpToDate candidate nodeState
            let candidateTermComparison = (candidate.electionTerm, getElectionTerm nodeState)

            match candidateTermComparison with
            | SameTerm -> handleSameTermVote candidate fi candidateIsUpToDate
            | OldTerm -> false, nodeState
            | NewTerm -> handleNewTermVote candidate fi candidateIsUpToDate

    let tryBecomeLeader (numberOfNodes: int) (receivedNewVote: bool) (nodeState: NodeState) =
        let quorum = getQuorum numberOfNodes

        match nodeState with
        | Leader _
        | Follower _ -> false, nodeState
        | Candidate ci ->
            let latestNumberVotes = ci.votes + (if receivedNewVote then 1 else 0)

            if latestNumberVotes >= quorum then
                true,
                Leader
                    { nodeId = ci.nodeId
                      electionTerm = ci.electionTerm }
            else
                false, Candidate { ci with votes = latestNumberVotes }

    let acknowledgeLeaderHeartbeat (leader: LeaderInfo) (nodeState: NodeState) =
        match nodeState with
        | Leader li ->
            if li.electionTerm > leader.electionTerm then
                nodeState
            // If heartbeat comes from another leader with higher election term node must switch to Follower
            else 
                Follower
                    { leader = Some leader
                      votedFor = None
                      electionTerm = leader.electionTerm
                      lastLogIndex = 0 } // TODO: Implement log replication
        | Follower fi -> Follower { fi with leader = Some leader }
        | Candidate ci ->
            Follower
                { leader = Some leader
                  votedFor = None
                  electionTerm = ci.electionTerm
                  lastLogIndex = ci.lastLogIndex }
