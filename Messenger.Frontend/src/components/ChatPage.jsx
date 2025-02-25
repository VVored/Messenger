import React, { useEffect, useState } from 'react'
import axios from 'axios';
import ChatList from './ChatList';
import ChatMessages from './ChatMessages';

const getUserChats = async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get('https://localhost:7192/api/chats/my', {
        headers: {
            'Authorization': `Bearer ${token}`
        }});
    return response;
}

const ChatPage = () => {
    const [chats, setChats] = useState([]);
    const [selectedChat, setSelectedChat] = useState(null);

    useEffect(() => {
        getUserChats().then((result) => {
            setChats(result.data);
        });
    }, []) 
    return (
        <div style={{display: "flex", height: '100%', overflowY: 'hidden'}}>
            <ChatList chats={chats} setSelectedChat={setSelectedChat}/>
            <div style={{width: "70vw", backgroundColor: "rgba(178, 178, 178, 0.5)"}}>
                {
                    selectedChat ? <ChatMessages chat={selectedChat}></ChatMessages> : <h1 style={{textAlign: "center"}}>Select a chat</h1>
                }
            </div>
        </div>
    )
}

export default ChatPage;