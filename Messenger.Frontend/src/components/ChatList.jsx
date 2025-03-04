import React, { useState } from 'react'
import Chat from './Chat';
import styles from './ChatList.module.css';
import axios from 'axios';

const ChatList = ({ chats, setSelectedChat }) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [globalChats, setGlobalChats] = useState([]);

    const globalSearchChats = async () => {
        if (searchQuery !== '') {
            const response = await axios.get(`https://localhost:7192/api/chats?searchQuery=${searchQuery}`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            })
            setGlobalChats(response.data);
        }
    }

    return (
        <div style={{ width: "30vw", overflowY: "auto", minHeight: "100vh" }}>
            <button className={styles.button}>Create new chat</button>
            <input className={styles.input} type="text" onChange={(e) => {setSearchQuery(e.target.value); globalSearchChats();}} value={searchQuery} placeholder="Global search"/>
            {
                searchQuery ? <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>Global search result</div> : <div></div>
            }
            {
                searchQuery ? globalChats.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => setSelectedChat(chat)}></Chat>
                ) : <div></div>
            }
            <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>My chats</div>
            {
                chats.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => setSelectedChat(chat)}></Chat>
                )
            }
        </div>
    )
}

export default ChatList;