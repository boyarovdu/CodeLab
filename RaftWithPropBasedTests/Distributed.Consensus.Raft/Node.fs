namespace Distributed.Consensus.Raft

open System
open System.Timers

open Distributed.Consensus.Raft.LeaderElection

type NodeEvent =
    | TimerElapsed of TimerType
    | IncomingMessage of Message

and TimerType =
    | ElectionTimeout
    | HeartbeatTimeout

type Node(nodeId: NodeId, nodeIds: NodeId array) =

    (* --- OBSERVABLE MESSAGES STREAM --- *)
    let messagesStream = Event<Message>()

    (* --- TIMERS --- *)
    // TODO: make it configurable
    let electionMinTimeout, electionMaxTimeout, heartBeatTimeout = (1500, 3000, 500)
    let electionTimerDelayRandom = Random()
    let electionTimerLock = obj ()

    let electionTimer, heartbeatTimer =
        new Timer(electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout)), new Timer(heartBeatTimeout)

    // Resetting election timer could happen concurrently, so we need to make it thread-safe
    let resetElectionTimerSafe () =
        lock electionTimerLock (fun () ->
            electionTimer.Stop()

            electionTimer.Interval = electionTimerDelayRandom.Next(electionMinTimeout, electionMaxTimeout)
            |> ignore

            electionTimer.Start())

    (* --- EVENT TRIGGERS --- *)
    let triggerRequestVoteMessage (candidate: CandidateInfo) =
        let requestVoteMessage =
            { senderId = nodeId
              recipients = nodeIds
              messageType = LeaderElection(RequestVote candidate) }

        messagesStream.Trigger requestVoteMessage

    let triggerAcceptVoteMessage (candidate: CandidateInfo) =
        let acceptVoteMessage =
            { senderId = nodeId
              recipients = [| candidate.nodeId |]
              messageType = LeaderElection(AcceptVote candidate) }

        messagesStream.Trigger acceptVoteMessage

    let triggerAppendEntryMessage (leaderInfo: LeaderInfo) =
        let appendEntryMessage =
            { senderId = nodeId
              recipients = nodeIds
              messageType = LeaderElection(AppendEntry leaderInfo) }

        messagesStream.Trigger appendEntryMessage

        heartbeatTimer.Start()
        electionTimer.Stop()


    (* --- TIMER HANDLERS --- *)
    let processHeartbeatTimeout state =
        do
            match state with
            | Leader _ -> triggerAppendEntryMessage { nodeId = nodeId }
            | _ -> ignore ()

        state

    let processElectionTimeout currentState =
        match LeaderElection.tryStartNewElectionTerm nodeId currentState with
        | true, newState ->
            do
                match newState with
                | Candidate ci ->
                    triggerRequestVoteMessage ci
                    resetElectionTimerSafe ()
                | Follower _ -> resetElectionTimerSafe ()
                | _ -> ignore ()

            newState
        | false, newState -> newState

    (* --- MESSAGE PROCESSING --- *)
    let processAppendEntryMessage (leaderInfo: LeaderInfo) =
        let ns = LeaderElection.acknowledgeLeaderHeartbeat leaderInfo
        resetElectionTimerSafe ()
        ns

    let processRequestVoteMessage candidate currentState =
        match LeaderElection.tryVote candidate currentState with
        | true, newState ->
            triggerAcceptVoteMessage candidate
            newState
        | _, ns -> ns

    let processAcceptVoteMessage (candidate: CandidateInfo) state =
        match LeaderElection.tryBecomeLeader nodeIds.Length true state with
        | true, newState ->
            do
                match newState with
                | Leader li -> triggerAppendEntryMessage li
                | _ -> ignore ()

            newState
        | false, ns -> ns

    (* --- NODE MAILBOX --- *)
    let processNodeEvent nodeEvent nodeState =
        match nodeEvent with
        | TimerElapsed ElectionTimeout -> processElectionTimeout nodeState
        | TimerElapsed HeartbeatTimeout -> processHeartbeatTimeout nodeState
        | IncomingMessage message ->
            match message.messageType with
            | LeaderElection(AppendEntry leaderInfo) -> processAppendEntryMessage leaderInfo nodeState
            | LeaderElection(RequestVote candidate) -> processRequestVoteMessage candidate nodeState
            | LeaderElection(AcceptVote candidateInfo) -> processAcceptVoteMessage candidateInfo nodeState

    // Mailbox processor allows to get rid of mutable state, making it impossible to introduce concurrency issues
    // related to node state changes. State is "locked" inside the mailbox recurring loop.
    let mailbox =
        MailboxProcessor<NodeEvent>.Start(fun inbox ->
            let rec loop state =
                async {
                    let! msg = inbox.Receive()
                    return! loop (processNodeEvent msg state)
                }

            loop (
                Follower // Nodes start as followers
                    { leader = None
                      votedFor = None
                      lastLogIndex = 0
                      electionTerm = 0 }
            ))

    (* --- TYPE INITIALIZATION --- *)
    do
        // Init timers
        electionTimer.Elapsed.Add(fun _ -> mailbox.Post(TimerElapsed ElectionTimeout))
        electionTimer.AutoReset <- false

        heartbeatTimer.Elapsed.Add(fun _ -> mailbox.Post(TimerElapsed HeartbeatTimeout))
        heartbeatTimer.AutoReset <- true

    (* --- PUBLIC MEMBERS --- *)
    member this.MessagesStream = messagesStream.Publish

    member this.Start() = resetElectionTimerSafe ()

    member this.PostMessage(message: Message) =
        mailbox.Post(NodeEvent.IncomingMessage message)

    member this.Id = nodeId

    interface IDisposable with
        member this.Dispose() = electionTimer.Dispose()
