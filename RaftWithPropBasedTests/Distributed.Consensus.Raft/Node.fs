namespace Distributed.Consensus.Raft

open System
open System.Timers

open Distributed.Consensus.Raft.LeaderElection

type Node(id: NodeId, nodeIds: NodeId array) =

    (* --- MUTABLE STATE --- *)
    let stateLock = obj ()

    let mutable state =
        Follower // Nodes start as followers
            { leader = None
              votedFor = None
              lastLogIndex = 0
              electionTerm = 0 }

    (*
    I thought about using mailbox processor for solving all the concurrency issues(including timers) inside this type, 
    but all work in such case will be done on one thread, which could become a bottleneck if there are high-frequency 
    events or many nodes. That's why I prefer to use locks for state updates
    *)
    let updateStateSafe f =
        lock stateLock (fun () -> state <- f state)

    // TODO: Node must know all other reachable nodes in the network
    // let mutable nodeIds: NodeId array = [||]

    (* --- OBSERVABLE MESSAGES STREAM --- *)
    let messagesStream = Event<Message>()

    (* --- TIMERS --- *)
    // TODO: make it configurable
    let electionMinTimeout = 1500
    let electionMaxTimeout = 3000
    let heartBeatTimeout = 500
    let electionTimerDelayRandom = Random()
    let electionTimerLock = obj ()

    let electionTimer =
        new Timer(electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout))

    let heartbeatTimer = new Timer(heartBeatTimeout)

    let resetElectionTimerSafe () =
        lock electionTimerLock (fun () ->
            electionTimer.Stop()

            electionTimer.Interval = electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout)
            |> ignore

            electionTimer.Start())


    (* --- EVENT TRIGGERS --- *)
    let triggerRequestVoteMessage (candidate: CandidateInfo) =
        let requestVoteMessage =
            { senderId = id
              recipients = nodeIds
              messageType = LeaderElection(RequestVote candidate) }

        messagesStream.Trigger requestVoteMessage

    let triggerAcceptVoteMessage (candidate: CandidateInfo) =
        let acceptVoteMessage =
            { senderId = id
              recipients = [| candidate.nodeId |]
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

    (* --- LEADER ELECTION LOGIC FUNCs PREPARATION --- *)
    let startNewElectionTerm =
        LeaderElection.startNewElectionTerm (id, triggerRequestVoteMessage)

    let vote = LeaderElection.vote triggerAcceptVoteMessage
    let tryBecomeLeader = LeaderElection.tryBecomeLeader triggerAppendEntryMessage

    (* --- TIMER HANDLERS --- *)
    let processHeartbeatTimeout _ =
        match state with
        | Leader _ -> triggerAppendEntryMessage { nodeId = id }
        | _ -> ()

    let processElectionTimeout _ =
        match state with
        | Leader _ -> () // Leader doesn't start new election
        | Candidate _
        | Follower _ ->
            updateStateSafe startNewElectionTerm
            resetElectionTimerSafe ()

    (* --- MESSAGE PROCESSING --- *)
    let processAppendEntryMessage (leaderInfo: LeaderInfo) =
        updateStateSafe (LeaderElection.acknowledgeLeaderHeartbeat leaderInfo)
        resetElectionTimerSafe ()

    let processRequestVoteMessage (candidate: CandidateInfo) = updateStateSafe (vote candidate)

    let processAcceptVoteMessage (candidate: CandidateInfo) =
        updateStateSafe (tryBecomeLeader nodeIds.Length true)

    let processLeaderElectionMessage (message: MessageType) =
        match message with
        | AppendEntry leaderInfo -> processAppendEntryMessage leaderInfo
        | RequestVote candidateInfo -> processRequestVoteMessage candidateInfo
        | AcceptVote candidateInfo -> processAcceptVoteMessage candidateInfo

    (* --- TYPE INITIALIZATION --- *)
    do
        // Init timers
        electionTimer.Elapsed.Add processElectionTimeout
        electionTimer.AutoReset <- false

        heartbeatTimer.Elapsed.Add processHeartbeatTimeout
        heartbeatTimer.AutoReset <- true

    (* --- PUBLIC MEMBERS --- *)
    member this.MessagesStream = messagesStream.Publish

    member this.Start() = resetElectionTimerSafe ()

    member this.ProcessMessage(message: Message) =
        match message.messageType with
        | LeaderElection leaderElectionMessage -> processLeaderElectionMessage leaderElectionMessage

    member this.Id = id

    interface IDisposable with
        member this.Dispose() = electionTimer.Dispose()
