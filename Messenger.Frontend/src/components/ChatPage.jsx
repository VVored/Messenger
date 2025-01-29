import React, { useEffect, useState } from 'react'
import axios from 'axios';
import ChatList from './ChatList';

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
    useEffect(() => {
        getUserChats().then((result) => {
            setChats(result.data);
        });
    }, []) 
    return (
        <div style={{display: "flex", height: "100vh"}}>
            <ChatList chats={chats}/>
            <div style={{width: "70vw", backgroundColor: "rgba(178, 178, 178, 0.5)"}}>
                
            </div>
        </div>
    )
}

export default ChatPage;