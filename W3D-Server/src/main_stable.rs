// A basic JSON-protocol game server in Rust using async TCP.
// It supports multiple clients and interprets MOVE and CHAT events from JSON messages.

use tokio::net::TcpListener;
use tokio::io::{AsyncReadExt, AsyncWriteExt, split, WriteHalf};
use tokio::sync::broadcast;
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
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
    Move { x: f32, y: f32 },
    Chat { message: String },
}

#[derive(Debug, Deserialize)]
struct InitMessage {
    id: String,
    space_id: String,
}

#[derive(Debug)]
struct PlayerInfo {
    id: String,
    space_id: Uuid,
    writer: WriteHalf<tokio::net::TcpStream>,
}
#[derive(Debug, Serialize)]
#[serde(tag = "type")]
enum ServerMessage {
    Spawn { id: String, model_url: String },
}

type Clients = Arc<Mutex<HashMap<String, PlayerInfo>>>;

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

            clients.lock().unwrap().insert(id.clone(), PlayerInfo {
                id: id.clone(),
                space_id,
                writer,
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
                    Ok(ClientMessage::Move { x, y }) => {
                        println!("{} moved to x: {}, y: {}", id, x, y);
                        tx.send(GameEvent::PlayerMove {
                            id: id.clone(),
                            x,
                            y,
                        }).unwrap();
                    }
                    Ok(ClientMessage::Chat { message }) => {
                        println!("{} says: {}", id, message);
                        tx.send(GameEvent::PlayerChat {
                            id: id.clone(),
                            message,
                        }).unwrap();
                    }
                    Err(e) => {
                        println!("Failed to parse message from {}: {:?}", id, e);
                    }
                }
            }

            println!("{} disconnected", id);
            clients.lock().unwrap().remove(&id);
        });
    }
}
