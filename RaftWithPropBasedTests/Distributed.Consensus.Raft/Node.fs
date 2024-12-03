namespace Distributed.Consensus.Raft

open System
open System.Timers

open Distributed.Consensus.Raft.LeaderElection

type Node(id: NodeId, nodeIds: NodeId array) =
    // Mutable state
    let mutable state = 
        Follower // Nodes start as followers
            { leader = None
              votedFor = None
              lastLogIndex = 0
              electionTerm = 0 }

    // // TODO: make it configurable
    // let mutable nodeIds: NodeId array = [|"1"; "2"|]

    // Node events
    let messagesStream = Event<Message>()

    // Timers
    let electionMinTimeout = 1500
    let electionMaxTimeout = 3000
    let electionTimerDelayRandom = Random()
    let electionTimer = new Timer(electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout))
    
    let heartBeatTimeout = 500 
    let heartbeatTimer = new Timer(heartBeatTimeout)

    let resetElectionTimer () =
        electionTimer.Stop()
        electionTimer.Interval = electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout) |> ignore
        electionTimer.Start()

    // Events triggering
    let triggerRequestVoteMessage (candidate: CandidateInfo) =
        let requestVoteMessage =
            { senderId = id
              recipients = nodeIds
              messageType = LeaderElection(RequestVote candidate) }

        messagesStream.Trigger requestVoteMessage

    let triggerAcceptVoteMessage (candidate: CandidateInfo) =
        let acceptVoteMessage =
            { senderId = id
              recipients = [|candidate.nodeId|]
              messageType = LeaderElection(AcceptVote candidate) }

        messagesStream.Trigger acceptVoteMessage

    let triggerAppendEntryMessage (leaderInfo: LeaderInfo) =
        let appendEntryMessage =
            { senderId = id
              recipients = nodeIds
              messageType = LeaderElection(AppendEntry leaderInfo) }

        messagesStream.Trigger appendEntryMessage
        heartbeatTimer.Start()
        electionTimer.Stop()

    // Events processing
    let processHeartbeatTimeout _ =
        match state with
        | Leader _ -> triggerAppendEntryMessage { nodeId = id }
        | _ -> ()

    let processElectionTimeout _ =
        match state with
        | Leader _ -> () // Leader doesn't start new election
        | Candidate _
        | Follower _ ->
            state <- LeaderElection.startNewElectionTerm triggerRequestVoteMessage (state, id)
            resetElectionTimer ()

    // Message processing
    let processAppendEntryMessage (leaderInfo: LeaderInfo) =
        state <- LeaderElection.acknowledgeLeaderHeartbeat (state, leaderInfo)
        resetElectionTimer()

    let processRequestVoteMessage (candidate: CandidateInfo) =
        state <- LeaderElection.vote triggerAcceptVoteMessage (state, candidate)

    let processAcceptVoteMessage (candidate: CandidateInfo) =
        state <- LeaderElection.tryBecomeLeader triggerAppendEntryMessage (state, nodeIds.Length, true)

    let processLeaderElectionMessage (message: MessageType) =
        match message with
        | AppendEntry leaderInfo -> processAppendEntryMessage leaderInfo
        | RequestVote candidateInfo -> processRequestVoteMessage candidateInfo
        | AcceptVote candidateInfo -> processAcceptVoteMessage candidateInfo

    // Init type
    do
        // Init timers
        electionTimer.Elapsed.Add processElectionTimeout
        electionTimer.AutoReset <- false
        
        heartbeatTimer.Elapsed.Add processHeartbeatTimeout
        heartbeatTimer.AutoReset <- true

    // Public members
    member this.MessagesStream = messagesStream.Publish

    member this.Start() =
        resetElectionTimer()
        // heartbeatTimer.Start()
    
    member this.ProcessMessage(message: Message) =
        match message.messageType with
        | LeaderElection leaderElectionMessage -> processLeaderElectionMessage leaderElectionMessage
        
    member this.Id = id
    
    interface IDisposable with
        member this.Dispose() = electionTimer.Dispose()
