namespace Distributed.Consensus.Raft

open System
open System.Timers

open Distributed.Consensus.Raft.LeaderElection

type Node(id: NodeId) =
    // Mutable state
    let mutable state =
        Follower
            { leader = None
              votedFor = None
              lastLogIndex = 0
              electionTerm = 0 }

    let mutable electionTerm = 0

    // Node events
    let messagesStream = Event<Message>()

    // Node leader election timer
    let electionTimer = new Timer()
    let electionTimerDelayRandom = Random()

    let resetElectionTimer () =
        electionTimer.Stop()
        electionTimer.Interval = electionTimerDelayRandom.Next(150, 300) |> ignore
        electionTimer.Start()

    // Events triggering
    let triggerRequestVoteMessage (candidate: CandidateInfo) =
        let requestVoteMessage =
            { senderId = id
              recipients = [||]
              messageType = LeaderElection(RequestVote candidate) }

        messagesStream.Trigger requestVoteMessage

    let triggerAcceptVoteMessage (candidate: CandidateInfo) =
        let acceptVoteMessage =
            { senderId = id
              recipients = [||]
              messageType = LeaderElection(AcceptVote candidate) }

        messagesStream.Trigger acceptVoteMessage

    // Events processing
    let processElectionTimeout _ =
        state <-
            match LeaderElection.startNewElectionTerm (state, electionTerm, id) with
            | Candidate ci ->
                triggerRequestVoteMessage ci
                Candidate ci
            | ns -> ns

    // Message processing
    let processAppendEntryMessage (leaderInfo: LeaderInfo) = ()

    let processRequestVoteMessage (candidate: CandidateInfo) =
        state <- LeaderElection.vote (state, candidate, triggerAcceptVoteMessage)

    let processAcceptVoteMessage (candidate: CandidateInfo) = ()

    let processLeaderElectionMessage (message: MessageType) =
        match message with
        | AppendEntry leaderInfo -> processAppendEntryMessage leaderInfo
        | RequestVote candidate -> processRequestVoteMessage candidate
        | AcceptVote candidate -> processAcceptVoteMessage candidate

    // Init module
    do
        // Init timer
        electionTimer.Elapsed.Add processElectionTimeout
        resetElectionTimer ()

    // Public members
    member this.MessagesStream = messagesStream.Publish

    member this.ProcessMessage(message: Message) =
        match message.messageType with
        | LeaderElection leaderElectionMessage -> processLeaderElectionMessage leaderElectionMessage

        ()

    interface IDisposable with
        member this.Dispose() = electionTimer.Dispose()
