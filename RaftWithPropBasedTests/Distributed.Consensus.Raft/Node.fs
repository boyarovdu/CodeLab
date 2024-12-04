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

type Node(id: NodeId, nodeIds: NodeId array) =

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
    let processHeartbeatTimeout state =
        match state with
        | Leader _ -> triggerAppendEntryMessage { nodeId = id }
        | _ -> ()

        state

    let processElectionTimeout state =
        match state with
        | Leader _ -> state // Leader doesn't start new election
        | Candidate _
        | Follower _ ->
            let newState = startNewElectionTerm state
            resetElectionTimerSafe ()
            newState

    (* --- MESSAGE PROCESSING --- *)
    let processAppendEntryMessage (leaderInfo: LeaderInfo) =
        let newState = LeaderElection.acknowledgeLeaderHeartbeat leaderInfo
        resetElectionTimerSafe ()
        newState

    let processRequestVoteMessage (candidate: CandidateInfo) = vote candidate

    let processAcceptVoteMessage (candidate: CandidateInfo) = tryBecomeLeader nodeIds.Length true

    (* --- NODE MAILBOX --- *)
    // Mailbox processor allows to get rid of mutable state, making it impossible to introduce concurrency issues
    // related to node state changes. State is "locked" inside the mailbox recurring loop.
    let mailbox =
        MailboxProcessor<NodeEvent>.Start(fun inbox ->
            let rec loop state =
                async {
                    let! msg = inbox.Receive()
                    return!
                        loop (
                            match msg with
                            | TimerElapsed ElectionTimeout -> processElectionTimeout state
                            | TimerElapsed HeartbeatTimeout -> processHeartbeatTimeout state
                            | IncomingMessage message ->
                                match message.messageType with
                                | LeaderElection(AppendEntry leaderInfo) -> processAppendEntryMessage leaderInfo state
                                | LeaderElection(RequestVote candidate) -> processRequestVoteMessage candidate state
                                | LeaderElection(AcceptVote candidateInfo) ->
                                    processAcceptVoteMessage candidateInfo state
                        )
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
        let msgEvent = NodeEvent.IncomingMessage message
        mailbox.Post msgEvent

    member this.Id = id

    interface IDisposable with
        member this.Dispose() = electionTimer.Dispose()
