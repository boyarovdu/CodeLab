namespace Distributed.Consensus.Raft

open System
open System.Timers
open System.Reactive.Disposables

open Distributed.Consensus.Raft.LeaderElection

type InternalMailboxMessage =
    | ProcessElectionTimeout
    | ProcessHeartbeatTimeout
    | GetCurrentState of AsyncReplyChannel<NodeState>
    | ProcessNodeMessage of RaftMessage

type DiagnosticLogEntry =
    { nodeId: NodeId
      timeStamp: string
      mailboxMessage: InternalMailboxMessage
      initialState: NodeState
      finalState: NodeState
      mailboxQueueSize: int }

type RaftNode(nodeId: NodeId, clusterSize) =

    (* --- OBSERVABLE MESSAGES STREAM --- *)
    let nodeMessageEvent = Event<NodeId * RaftMessage>()
    let diagnosticLogEntryEvent = Event<DiagnosticLogEntry>()

    (* --- TIMERS --- *)
    // TODO: make it configurable
    let electionMinTimeout, electionMaxTimeout, heartBeatTimeout = (150, 300, 100)
    let electionTimerDelayRandom = Random()

    let electionTimer, heartbeatTimer = new Timer(), new Timer(heartBeatTimeout)

    // Resetting election timer could happen concurrently, so we need to make it thread-safe
    let resetElectionTimer () =
        electionTimer.Stop()
        electionTimer.Interval <- electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout)
        electionTimer.Start()

    (* --- EVENT TRIGGERS --- *)
    let triggerRequestVoteMessage (candidate: CandidateInfo) =
        nodeMessageEvent.Trigger(nodeId, LeaderElection(RequestVote candidate))

    let triggerAcceptVoteMessage (candidate: CandidateInfo) =
        nodeMessageEvent.Trigger(nodeId, LeaderElection(AcceptVote candidate))

    let triggerAppendEntryMessage (leaderInfo: LeaderInfo) =
        nodeMessageEvent.Trigger(nodeId, LeaderElection(AppendEntry leaderInfo))

    (* --- TIMER HANDLERS --- *)
    let processHeartbeatTimeout state =
        do
            match state with
            | Leader li -> triggerAppendEntryMessage li
            | _ -> ignore ()

        state

    let processElectionTimeout currentState =
        resetElectionTimer ()

        match LeaderElection.tryStartNewElectionTerm nodeId currentState with
        | true, newState ->
            match newState with
            | Candidate ci ->
                triggerRequestVoteMessage ci
                newState
            | _ -> newState
        | false, newState -> newState

    (* --- MESSAGE PROCESSING --- *)
    let processAppendEntryMessage (leaderInfo: LeaderInfo) =
        resetElectionTimer ()
        LeaderElection.acknowledgeLeaderHeartbeat leaderInfo

    let processRequestVoteMessage candidate currentState =
        match LeaderElection.tryVote candidate currentState with
        | true, newState ->
            triggerAcceptVoteMessage candidate
            newState
        | _, newState -> newState

    let processAcceptVoteMessage (candidate: CandidateInfo) state =
        match LeaderElection.tryBecomeLeader clusterSize true state with
        | true, newState ->
            do
                match newState with
                | Leader li -> triggerAppendEntryMessage li
                | _ -> ignore ()

            newState
        | false, newState -> newState

    (* --- NODE MAILBOX --- *)
    let processCommand (nodeMessage: InternalMailboxMessage) nodeState =

        match nodeMessage with
        | ProcessElectionTimeout -> processElectionTimeout nodeState
        | ProcessHeartbeatTimeout -> processHeartbeatTimeout nodeState
        | ProcessNodeMessage raftMessage ->
            match raftMessage with
            | LeaderElection(AppendEntry leaderInfo) -> processAppendEntryMessage leaderInfo nodeState
            | LeaderElection(RequestVote candidate) -> processRequestVoteMessage candidate nodeState
            | LeaderElection(AcceptVote candidateInfo) -> processAcceptVoteMessage candidateInfo nodeState
            | LogReplication -> failwith "todo"
        | GetCurrentState chan ->
            chan.Reply nodeState
            nodeState

    // Mailbox processor allows to get rid of mutable state, making it impossible to introduce concurrency issues
    // related to node state changes. State is "locked" inside the mailbox recurring loop.
    let mailbox =
        MailboxProcessor<InternalMailboxMessage>.Start(fun inbox ->
            let rec loop state =
                async {
                    let! msg = inbox.Receive()
                    let finalState = processCommand msg state

                    let diagnosticLogEntry =
                        { nodeId = nodeId
                          timeStamp = DateTime.Now.ToString("hh:mm:ss.fff")
                          mailboxMessage = msg
                          initialState = state
                          finalState = finalState
                          mailboxQueueSize = inbox.CurrentQueueLength }

                    diagnosticLogEntryEvent.Trigger(diagnosticLogEntry)

                    return! loop (finalState)
                }

            loop (
                Follower // Nodes start as followers
                    { leader = None
                      votedFor = None
                      electionTerm = 0 }
            ))

    (* --- TYPE INITIALIZATION --- *)
    do
        // Init timers
        electionTimer.Elapsed.Add(fun _ -> mailbox.Post(ProcessElectionTimeout))
        electionTimer.AutoReset <- false

        heartbeatTimer.Elapsed.Add(fun _ -> mailbox.Post(ProcessHeartbeatTimeout))
        heartbeatTimer.AutoReset <- true

    (* --- PUBLIC MEMBERS --- *)
    member this.MessagesStream = nodeMessageEvent.Publish
    member this.DiagnosticLogStream = diagnosticLogEntryEvent.Publish

    member this.Start() =
        resetElectionTimer ()
        heartbeatTimer.Start()

    member this.ProcessMessage(message: RaftMessage) =
        mailbox.Post(ProcessNodeMessage message)

    member this.GetState() =
        mailbox.PostAndReply(GetCurrentState)


    member this.Id = nodeId

    interface IDisposable with
        member this.Dispose() =
            try
                electionTimer.Dispose()
                heartbeatTimer.Dispose()
            finally
                mailbox.Dispose()

module RaftMessageDelivery =
    let getRecipients (nodes: RaftNode array) (message: RaftMessage) : RaftNode array =
        match message with
        | LeaderElection(AcceptVote ci) -> nodes |> Array.filter (fun node -> node.Id = ci.nodeId)
        | LeaderElection(RequestVote ci) -> nodes |> Array.filter (fun node -> node.Id <> ci.nodeId)
        | LeaderElection(AppendEntry li) -> nodes |> Array.filter (fun node -> node.Id <> li.nodeId)
        | LogReplication -> failwith "todo"
