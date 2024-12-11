namespace Distributed.Consensus.Raft.LeaderElection

module LeaderElection =
    let private (|OlderTerm|SameTerm|NewerTerm|) (termToCompare: ElectionTerm, otherTerm: ElectionTerm) =
        match termToCompare.CompareTo otherTerm with
        | r when r = 0 -> SameTerm
        | r when r > 0 -> NewerTerm
        | r when r < 0 -> OlderTerm
        // Represents impossible case, however covered so that IDE doesn't highlight this pattern match expression as
        // incomplete :)
        | r -> failwith $"Unexpected result %i{r} from ComapreTo(%i{termToCompare},%i{otherTerm}) operation detected"

    let private getElectionTerm nodeState =
        match nodeState with
        | Leader li -> li.electionTerm
        | Follower fi -> fi.electionTerm
        | Candidate ci -> ci.electionTerm

    let private getNewTerm nodeState = getElectionTerm nodeState + 1

    let private getQuorum numberOfNodes = (numberOfNodes / 2) + 1

    let tryStartNewElectionTerm nodeId nodeState =
        match nodeState with
        | Leader _ -> false, nodeState // Leader doesn't start new election
        | Candidate _ // If candidate doesn't collect the majority of votes it will start new election term
        | Follower _ ->
            true,
            Candidate
                { nodeId = nodeId
                  electionTerm = getNewTerm nodeState
                  votes = 1 } // votes for itself as a candidate

    let tryVote (candidate: CandidateInfo) (nodeState: NodeState) =
        let handleSameTermVote candidate nodeState =
            match nodeState with
            | Follower fi ->
                match fi.votedFor with
                | Some _ -> false, Follower fi
                | None -> true, Follower { fi with votedFor = Some candidate }
            | _ -> false, nodeState

        let handleNewTermVote candidate nodeState : bool * NodeState =
            match nodeState with
            | Follower fi ->
                true,
                Follower
                    { fi with
                        votedFor = Some candidate
                        electionTerm = candidate.electionTerm }
            | _ ->
                true,
                Follower
                    { leader = None
                      electionTerm = candidate.electionTerm
                      votedFor = Some candidate }


        let candidateTermComparison = (candidate.electionTerm, getElectionTerm nodeState)

        match candidateTermComparison with
        | SameTerm -> handleSameTermVote candidate nodeState
        | OlderTerm -> false, nodeState
        | NewerTerm -> handleNewTermVote candidate nodeState

    let tryBecomeLeader (numberOfNodes: int) (receivedNewVote: bool) (nodeState: NodeState) =
        let quorum = getQuorum numberOfNodes

        match nodeState with
        | Leader _
        | Follower _ -> false, nodeState
        | Candidate ci ->
            let actualNumberOfVotes = ci.votes + (if receivedNewVote then 1 else 0)

            if actualNumberOfVotes >= quorum then
                true,
                Leader
                    { nodeId = ci.nodeId
                      electionTerm = ci.electionTerm }
            else
                false, Candidate { ci with votes = actualNumberOfVotes }

    let acknowledgeLeaderHeartbeat (leader: LeaderInfo) (nodeState: NodeState) =
        match nodeState with
        | Leader thisNodeLeaderInfo ->
            match (thisNodeLeaderInfo.electionTerm, leader.electionTerm) with
            | NewerTerm -> nodeState // If this node is a leader with greater term
            | SameTerm -> // this situation must not happen, but just in case switching it to Follower mode
                Follower
                    { leader = Some leader
                      votedFor = None
                      electionTerm = leader.electionTerm }
            // If heartbeat comes from another leader with higher election term node must switch to Follower
            | _ ->
                Follower
                    { leader = Some leader
                      votedFor = None
                      electionTerm = leader.electionTerm }
        | Follower fi ->
            match (fi.electionTerm, leader.electionTerm) with
            | NewerTerm 
            | SameTerm -> nodeState
            | OlderTerm -> // If heartbeat comes from another leader with higher election term node must switch to Follower
                Follower
                    { leader = Some leader
                      votedFor = None
                      electionTerm = leader.electionTerm }
        | Candidate ci ->
            Follower
                { leader = Some leader
                  votedFor = None
                  electionTerm = ci.electionTerm }
