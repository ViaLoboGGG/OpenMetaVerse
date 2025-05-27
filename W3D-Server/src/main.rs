// A basic JSON-protocol game server in Rust using async TCP.
// It supports multiple clients and interprets MOVE and CHAT events from JSON messages.

use tokio::net::TcpListener;
use tokio::io::{AsyncReadExt, AsyncWriteExt, split, WriteHalf};
use tokio::sync::broadcast;
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::Mutex;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Clone)]
enum GameEvent {
    PlayerJoin(String),
    PlayerMove { id: String, x: f32, y: f32 },
    PlayerChat { id: String, message: String },
}

#[derive(Debug, Deserialize)]
#[serde(tag = "type")]
enum ClientMessage {
    Init { id: String, space_id: String },
    Move { x: f32, y: f32 },
    Chat { message: String },
}

#[derive(Debug, Deserialize)]
struct InitMessage {
    id: String,
    space_id: String,
}

#[derive(Debug, Serialize)]
#[serde(tag = "type")]
enum ServerMessage {
    Spawn { id: String, model_url: String },
    Chat { id: String, message: String },
    Move { id: String, x: f32, y: f32 },
}

#[derive(Debug)]
struct PlayerInfo {
    id: String,
    space_id: Uuid,
    writer: Arc<Mutex<WriteHalf<tokio::net::TcpStream>>>,
}

type Clients = Arc<Mutex<HashMap<String, PlayerInfo>>>;

async fn handle_move(id: &str, space_id: Uuid, x: f32, y: f32, clients: &Clients, tx: &broadcast::Sender<GameEvent>) {
    tx.send(GameEvent::PlayerMove {
        id: id.to_string(),
        x,
        y,
    }).unwrap();

    let move_msg = ServerMessage::Move {
        id: id.to_string(),
        x,
        y,
    };
    let json = serde_json::to_string(&move_msg).unwrap();

    let clients_guard = clients.lock().await;
    for client in clients_guard.values() {
        if client.space_id == space_id {
            let mut writer = client.writer.lock().await;
            let _ = writer.write_all(json.as_bytes()).await;
            let _ = writer.write_all(b"\n").await;
        }
    }
}

async fn handle_chat(id: &str, space_id: Uuid, message: String, clients: &Clients, tx: &broadcast::Sender<GameEvent>) {
    tx.send(GameEvent::PlayerChat {
        id: id.to_string(),
        message: message.clone(),
    }).unwrap();

    let chat_msg = ServerMessage::Chat {
        id: id.to_string(),
        message,
    };
    let json = serde_json::to_string(&chat_msg).unwrap();

    let clients_guard = clients.lock().await;
    for client in clients_guard.values() {
        if client.space_id == space_id {
            let mut writer = client.writer.lock().await;
            let _ = writer.write_all(json.as_bytes()).await;
            let _ = writer.write_all(b"\n").await;
        }
    }
}

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    let listener = TcpListener::bind("127.0.0.1:4000").await?;
    println!("Server listening on 127.0.0.1:4000");

    let (event_tx, _event_rx) = broadcast::channel::<GameEvent>(100);
    let clients: Clients = Arc::new(Mutex::new(HashMap::new()));

    loop {
        let (socket, _) = listener.accept().await?;
        let tx = event_tx.clone();
        let clients = clients.clone();

        tokio::spawn(async move {
            let (mut reader, writer) = split(socket);
            let mut buffer = [0u8; 1024];

            let n = match reader.read(&mut buffer).await {
                Ok(0) => return,
                Ok(n) => n,
                Err(_) => return,
            };

            let init_str = String::from_utf8_lossy(&buffer[..n]).trim().to_string();
            let init: InitMessage = match serde_json::from_str(&init_str) {
                Ok(i) => i,
                Err(e) => {
                    println!("Failed to parse InitMessage: {:?}", e);
                    return;
                }
            };

            let id = init.id;
            let space_id = match Uuid::parse_str(&init.space_id) {
                Ok(uuid) => uuid,
                Err(e) => {
                    println!("Invalid space_id from {}: {:?}", id, e);
                    return;
                }
            };

            println!("{} connected to space {}", id, space_id);

            clients.lock().await.insert(id.clone(), PlayerInfo {
                id: id.clone(),
                space_id,
                writer: Arc::new(Mutex::new(writer)),
            });

            tx.send(GameEvent::PlayerJoin(id.clone())).unwrap();

            let mut read_buffer = [0u8; 1024];
            loop {
                let n = match reader.read(&mut read_buffer).await {
                    Ok(0) => break,
                    Ok(n) => n,
                    Err(_) => break,
                };
                if n == 0 { break; }

                let raw = &read_buffer[..n];
                let text = String::from_utf8_lossy(raw);
                println!("Received from {}: {}", id, text);

                match serde_json::from_str::<ClientMessage>(&text) {
                    Ok(ClientMessage::Init { id: init_id, space_id: init_space_id }) => {
                        // Only allow this once per client (if needed)
                        println!("Init received from {} for space {}", init_id, init_space_id);
                        // optionally store again / validate
                    }
                    Ok(ClientMessage::Move { x, y }) => {
                        handle_move(&id, space_id, x, y, &clients, &tx).await;
                    }
                    Ok(ClientMessage::Chat { message }) => {
                        handle_chat(&id, space_id, message, &clients, &tx).await;
                    }
                    Err(e) => {
                        println!("Failed to parse message from {}: {:?}", id, e);
                    }
                }
            }

            println!("{} disconnected", id);
            clients.lock().await.remove(&id);
        });
    }
}
