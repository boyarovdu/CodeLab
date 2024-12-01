namespace Distributed.Consensus.Raft.LeaderElection

type NodeId = string

type LeaderInfo = { nodeId: NodeId }

type CandidateInfo =
    { nodeId: NodeId
      electionTerm: int
      votes: int
      lastLogIndex: int }

type FollowerInfo =
    { leader: LeaderInfo option
      votedFor: CandidateInfo option
      electionTerm: int
      lastLogIndex: int }
    
type NodeState =
    | Leader
    | Follower of FollowerInfo
    | Candidate of CandidateInfo

type MessageType =
    | RequestVote of CandidateInfo
    | AcceptVote of CandidateInfo
    | AppendEntry of LeaderInfo
