/**
 * Thin SignalR helper for ECJ EventQuiz.
 * Talks to QuizHub at /hubs/quiz (started from WPF ServerUI).
 *
 * Usage:
 *   const hub = createQuizHub()
 *   await hub.connect()
 *   hub.on('LobbyUpdated', dto => { ... })
 *   await hub.createSession(1)
 */
import * as signalR from '@microsoft/signalr'

// Hub-URL: overrives via localStorage ('ecj-hub-url') eller VITE_HUB_URL –
// nyttigt når deltagere skal bruge hostens LAN-IP i stedet for localhost.
function defaultHubUrl() {
  try {
    const stored = window.localStorage.getItem('ecj-hub-url')
    if (stored) return stored
  } catch {
    // privado mode m.m. – ignorér
  }
  return import.meta.env?.VITE_HUB_URL ?? 'http://localhost:5188/hubs/quiz'
}

export function createQuizHub(hubUrl) {
  const url = hubUrl ?? defaultHubUrl()
  let connection = null

  async function connect() {
    if (connection?.state === signalR.HubConnectionState.Connected) return connection

    connection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()

    await connection.start()
    return connection
  }

  async function stop() {
    if (!connection) return
    await connection.stop()
    connection = null
  }

  function on(eventName, handler) {
    if (!connection) throw new Error('Forbind først med connect()')
    connection.on(eventName, handler)
  }

  function off(eventName, handler) {
    connection?.off(eventName, handler)
  }

  async function invoke(method, ...args) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Ikke forbundet til hub')
    }
    return connection.invoke(method, ...args)
  }

  return {
    connect,
    stop,
    on,
    off,
    get state() {
      return connection?.state ?? signalR.HubConnectionState.Disconnected
    },
    // Hub-metoder (matcher QuizHub.cs)
    createSession: (quizId) => invoke('CreateSession', quizId),
    joinAsHost: (roomId) => invoke('JoinAsHost', roomId),
    joinLobby: (roomId, name) => invoke('JoinLobby', roomId, name),
    startQuiz: (roomId) => invoke('StartQuiz', roomId),
    openQuestion: (roomId) => invoke('OpenQuestion', roomId),
    submitAnswer: (roomId, questionId, answerOptionId) =>
      invoke('SubmitAnswer', roomId, questionId, answerOptionId),
    closeQuestion: (roomId) => invoke('CloseQuestion', roomId),
    nextQuestion: (roomId) => invoke('NextQuestion', roomId),
    endQuiz: (roomId) => invoke('EndQuiz', roomId),
  }
}
