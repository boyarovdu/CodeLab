namespace Distributed.Consensus.Raft

module Raft =
    let private (|OlderTerm|SameTerm|NewerTerm|) (termToCompare: ElectionTerm, otherTerm: ElectionTerm) =
        match termToCompare.CompareTo otherTerm with
        | r when r = 0 -> SameTerm termToCompare
        | r when r > 0 -> NewerTerm termToCompare
        | r when r < 0 -> OlderTerm termToCompare
        | _ -> failwith "Invalid term comparison"

    let private getNewTerm (electionTerm: ElectionTerm) = electionTerm + 1

    let private getQuorum numberOfNodes = (numberOfNodes / 2) + 1

    let tryStartNewElectionTerm _ (nodeState: NodeState) : bool * NodeState =
        match nodeState.role with
        | Leader -> false, nodeState // Leader doesn't start new election
        | Candidate _ // If candidate doesn't collect the majority of votes it will start new election term
        | Follower _ ->
            true, { electionTerm = getNewTerm nodeState.electionTerm; role = Candidate { votes = 1 } }

    let tryVote (candidateId, candidateTerm) (nodeState: NodeState) =
        let handleSameCandidateTerm (candidateId, candidateTerm) nodeState : (bool * NodeState) =
            match nodeState.role with
            | Follower fi ->
                match fi.votedFor with
                | Some _ -> false, nodeState
                | None -> true, { nodeState with role = Follower { fi with votedFor = Some candidateId } }
            | _ -> false, nodeState

        let handleNewerCandidateTerm (candidateId, candidateTerm) nodeState : bool * NodeState =
            match nodeState.role with
            | Follower fi ->
                true, { electionTerm = candidateTerm; role = Follower { fi with votedFor = Some candidateId } }
            | _ ->
                true, { electionTerm = candidateTerm; role = Follower { leader = None; votedFor = Some candidateId } }

        match (candidateTerm, nodeState.electionTerm) with
        | SameTerm candidateTerm -> handleSameCandidateTerm (candidateId, candidateTerm) nodeState
        | OlderTerm candidateTerm -> false, nodeState
        | NewerTerm candidateTerm -> handleNewerCandidateTerm (candidateId, candidateTerm) nodeState

    let tryBecomeLeader (numberOfNodes: int) (receivedNewVote: bool) (nodeState: NodeState) =
        let quorum = getQuorum numberOfNodes

        match nodeState.role with
        | Leader
        | Follower _ -> false, nodeState
        | Candidate ci ->
            let actualNumberOfVotes = ci.votes + (if receivedNewVote then 1 else 0)
            
            if actualNumberOfVotes >= quorum
            then true, { nodeState with role = Leader }
            else false, { nodeState with role = Candidate { ci with votes = actualNumberOfVotes } }

    let acknowledgeLeaderHeartbeat (leaderId, leaderTerm) (nodeState: NodeState) =
        match nodeState.role with
        | Leader ->
            match (nodeState.electionTerm, leaderTerm) with
            | NewerTerm thisNodeTerm -> nodeState // If this node is a leader with greater term
            | SameTerm thisNodeTerm
            | OlderTerm thisNodeTerm ->
                { electionTerm = leaderTerm; role = Follower { leader = Some leaderId; votedFor = None } }
        | Follower fi ->
            match (nodeState.electionTerm, leaderTerm) with
            | NewerTerm thisNodeTerm
            | SameTerm thisNodeTerm ->
                { electionTerm = nodeState.electionTerm; role = Follower { fi with leader = Some leaderId } }
            | OlderTerm thisNodeTerm ->
                // If heartbeat comes from another leader with higher election term node must switch to Follower
                { electionTerm = leaderTerm; role = Follower { leader = Some leaderId; votedFor = None } }
        | Candidate _ ->
            { nodeState with role = Follower { leader = Some leaderId; votedFor = None } }
