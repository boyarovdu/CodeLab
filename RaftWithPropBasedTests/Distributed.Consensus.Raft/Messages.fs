namespace Distributed.Consensus.Raft

open Distributed.Consensus.Raft.LeaderElection

type RaftMessage =
    | LeaderElection of LeaderElectionMessage
    | LogReplication


            

