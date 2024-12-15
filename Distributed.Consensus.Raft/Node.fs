namespace Distributed.Consensus.Raft

open System
open System.Timers

type TimersConfig = {
    electionMinTimeoutMs: int
    electionMaxTimeoutMs: int
    heartBeatTimeoutMs: int
}

type InternalMailboxMessage =
    | ProcessElectionTimeout of AsyncReplyChannel<NodeState>
    | ProcessHeartbeatTimeout
    | GetCurrentState of AsyncReplyChannel<NodeState>
    | ProcessRaftMessage of RaftMessage * AsyncReplyChannel<NodeState>

type DiagnosticLogEntry =
    { nodeId: NodeId
      timeStamp: string
      mailboxMessage: InternalMailboxMessage
      initialState: NodeState
      finalState: NodeState
      mailboxQueueSize: int }

type Node(nodeId: NodeId, clusterSize, timersConfig: TimersConfig) =

    (* --- OBSERVABLE MESSAGES STREAM --- *)
    let nodeMessageEvent = Event<NodeId * RaftMessage>()
    let diagnosticLogEntryEvent = Event<DiagnosticLogEntry>()

    (* --- TIMERS --- *)
    let electionTimerDelayRandom = Random()

    let electionTimer, heartbeatTimer = new Timer(), new Timer(timersConfig.heartBeatTimeoutMs)

    // Resetting election timer could happen concurrently, so we need to make it thread-safe
    let resetElectionTimer () =
        electionTimer.Stop()
        electionTimer.Interval <- electionTimerDelayRandom.Next(timersConfig.electionMinTimeoutMs, timersConfig.electionMaxTimeoutMs)
        electionTimer.Start()

    (* --- EVENT TRIGGERS --- *)
    let triggerRequestVoteMessage (electionTerm) =
        nodeMessageEvent.Trigger(nodeId, RequestVote(nodeId, electionTerm))

    let triggerAcceptVoteMessage (candidateId: NodeId) =
        nodeMessageEvent.Trigger(nodeId, AcceptVote candidateId)

    let triggerAppendEntryMessage (electionTerm) =
        nodeMessageEvent.Trigger(nodeId, AppendEntry(nodeId, electionTerm))

    (* --- TIMER HANDLERS --- *)
    let processHeartbeatTimeout (state: NodeState) =
        match state.role with
        | Leader ->
            triggerAppendEntryMessage state.electionTerm
            state
        | _ -> state


    let processElectionTimeout (currentState: NodeState) =
        resetElectionTimer ()

        match Raft.tryStartNewElectionTerm nodeId currentState with
        | true, newState ->
            match newState.role with
            | Candidate ci ->
                triggerRequestVoteMessage newState.electionTerm
                newState
            | _ -> newState
        | false, newState -> newState

    (* --- MESSAGE PROCESSING --- *)
    let processAppendEntryMessage (nodeId, electionTerm) (nodeState: NodeState) =
        resetElectionTimer ()
        Raft.acknowledgeLeaderHeartbeat (nodeId, electionTerm) nodeState

    let processRequestVoteMessage (candidateId, candidateTerm) (nodeState: NodeState) =
        match Raft.tryVote (candidateId, candidateTerm) (nodeState: NodeState) with
        | true, newState ->
            triggerAcceptVoteMessage candidateId
            newState
        | _, newState -> newState

    let processAcceptVoteMessage (nodeId) (state: NodeState) =
        match Raft.tryBecomeLeader clusterSize true state with
        | true, newState ->
            do triggerAppendEntryMessage newState.electionTerm
            newState
        | false, newState -> newState

    (* --- NODE MAILBOX --- *)
    let processCommand (nodeMessage: InternalMailboxMessage) (nodeState: NodeState) =

        match nodeMessage with
        | ProcessElectionTimeout chan ->
            let newState = processElectionTimeout nodeState
            chan.Reply newState
            newState
        | ProcessHeartbeatTimeout -> processHeartbeatTimeout nodeState
        | ProcessRaftMessage(raftMessage, chan) ->
            let newState =
                match raftMessage with
                | AppendEntry(nodeId, electionTerm) -> processAppendEntryMessage (nodeId, electionTerm) nodeState
                | RequestVote(nodeId, electionTerm) -> processRequestVoteMessage (nodeId, electionTerm) nodeState
                | AcceptVote nodeId -> processAcceptVoteMessage (nodeId) nodeState

            chan.Reply newState
            newState
        | GetCurrentState chan ->
            chan.Reply nodeState
            nodeState

    let diagnosticMailbox =
        MailboxProcessor<DiagnosticLogEntry>.Start(fun inbox ->
            let rec loop () =
                async {
                    let! msg = inbox.Receive()
                    diagnosticLogEntryEvent.Trigger(msg)
                    return! loop ()
                }

            loop ())

    // Mailbox processor allows to get rid of mutable state, making it impossible to introduce concurrency issues
    // related to node state changes. State is "locked" inside the mailbox recurring loop.
    let mailbox =
        MailboxProcessor<InternalMailboxMessage>.Start(fun inbox ->
            let rec loop (state: NodeState) =
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

                    diagnosticMailbox.Post(diagnosticLogEntry)

                    return! loop (finalState)
                }

            loop
                { electionTerm = 0
                  role = Follower { leader = None; votedFor = None } })

    (* --- TYPE INITIALIZATION --- *)
    do
        // Init timers
        electionTimer.Elapsed.Add(fun _ -> mailbox.PostAndAsyncReply(ProcessElectionTimeout) |> ignore)
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
        mailbox.PostAndAsyncReply(fun chan -> ProcessRaftMessage(message, chan))

    member this.ForceBecomeCandidate() =
        mailbox.PostAndAsyncReply(ProcessElectionTimeout) 

    member this.GetState() = mailbox.PostAndReply(GetCurrentState)
    member this.Id = nodeId

    interface IDisposable with
        member this.Dispose() =
            try
                electionTimer.Dispose()
                heartbeatTimer.Dispose()
            finally
                mailbox.Dispose()
