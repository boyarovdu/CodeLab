namespace Distributed.Consensus.Raft

module MessageRouter =
    let getRecipients (nodes: Node array) (message: RaftMessage) : Node array =
        match message with
        | AcceptVote candidateId ->nodes |> Array.filter (fun node -> node.Id = candidateId)
        | RequestVote (candidateId, candidateTerm) ->  nodes |> Array.filter (fun node -> node.Id <> candidateId)
        | AppendEntry (leaderId, leaderTerm) -> nodes |> Array.filter (fun node -> node.Id <> leaderId)