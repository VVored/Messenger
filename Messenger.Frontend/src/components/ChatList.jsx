import React, { useEffect } from 'react'

const getUserChat = async () => {
    const token = localStorage.getItem('token');
    if (token) {
        const response = await fetch('https://localhost:7192/api/chats/my', {
            method: 'GET',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
        });
        console.log(response);
    }
}

useEffect(() => {
    getUserChat();
}, []);

const ChatList = (id) => {
    return (
        <div>
            
        </div>
    )
}

export default ChatList;