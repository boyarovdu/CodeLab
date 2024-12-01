namespace Distributed.Consensus.Raft

open Distributed.Consensus.Raft.LeaderElection

type LeaderElectionMessageType = MessageType

type MessageType = LeaderElection of LeaderElectionMessageType

type Message =
    { senderId: NodeId
      recipients: NodeId array
      messageType: MessageType }
