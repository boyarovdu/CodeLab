namespace Distributed.Consensus.Raft.LeaderElection

type NodeId = string
type ElectionTerm = int

type LeaderInfo = { nodeId: NodeId
                    electionTerm: ElectionTerm }

type CandidateInfo =
    { nodeId: NodeId
      electionTerm: ElectionTerm
      votes: int
      lastLogIndex: int }

type FollowerInfo =
    { leader: LeaderInfo option
      votedFor: CandidateInfo option
      electionTerm: ElectionTerm
      lastLogIndex: int }
    
type NodeState =
    | Leader of LeaderInfo
    | Follower of FollowerInfo
    | Candidate of CandidateInfo

type LeaderElectionMessage =
    | RequestVote of CandidateInfo
    | AcceptVote of CandidateInfo
    | AppendEntry of LeaderInfo