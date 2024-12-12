namespace Distributed.Consensus.Raft

type NodeId = string
type ElectionTerm = int

type CandidateInfo = { votes: int }

type FollowerInfo =
    { leader: NodeId option
      votedFor: NodeId option }

type NodeRole =
    | Leader
    | Follower of FollowerInfo
    | Candidate of CandidateInfo

type NodeState =
    { electionTerm: ElectionTerm
      role: NodeRole }

type RaftMessage =
    | RequestVote of NodeId * ElectionTerm
    | AcceptVote of NodeId
    | AppendEntry of NodeId * ElectionTerm
