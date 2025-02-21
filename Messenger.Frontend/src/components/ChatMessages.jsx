import React, { useEffect, useState } from 'react'
import axios from 'axios';
import Message from './Message';
import { HubConnectionBuilder } from "@microsoft/signalr"

const sendMessage = async (e, chatId, content) => {
    if (e.key === 'Enter') {
        if (content !== '') {
            const token = localStorage.getItem('token');
            await fetch('https://localhost:7192/api/messages', {
                method: 'POST',
                body: JSON.stringify({ chatId, content }),
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
        }
    }
}

const getChatMessages = async (chatId) => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`https://localhost:7192/api/messages/chats/${chatId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    })
    return response;
}

const ChatMessages = ({ chat }) => {

    const [messages, setMessages] = useState([]);
    const [currentMessage, setCurrentMessage] = useState('');
    const [connection, setConnection] = useState(null);

    const StartConnection = async (chatId) => {
        const connection = new HubConnectionBuilder()
        .withUrl(`https://localhost:7192/chat?access_token=${localStorage.getItem('token')}`)
        .build();
        
        connection.start().then(res => {
            connection.invoke("JoinChat", chatId + '')
                .catch(err => {
                    console.log(err);
                });
        }).catch(err => {
            console.log(err);
        });
        connection.on('newMessage', message => { console.log(message); setMessages(prev => [...prev, message])});
        connection.onclose(() => StartConnection());
        setConnection(connection);
    }

    useEffect(() => {
        getChatMessages(chat.chatId).then(response => { setMessages(response.data) });
        StartConnection(chat.chatId).catch(error => {
            if (connection.state === 'Disconnected') {
                setTimeout(() => StartConnection(chat.chatId), 3000);
            }
        });
    }, [chat]);


    return (
        <div>
            {
                messages.map(message => {
                    return <Message key={message.messageId} message={message} />
                })
            }
            <div style={{ position: 'fixed', bottom: '0', width: '100%' }}>
                <input type="text" onChange={(e) => setCurrentMessage(e.target.value)} onKeyDown={(e) => sendMessage(e, chat.chatId, currentMessage)} placeholder="Text your message" />
            </div>
        </div>
    )
}

export default ChatMessages;