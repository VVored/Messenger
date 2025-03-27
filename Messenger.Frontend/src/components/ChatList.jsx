import React, { useState } from 'react'
import Chat from './Chat';
import styles from './ChatList.module.css';
import axios from 'axios';

const ChatList = ({ chats, openCreateChat, openChatMessages }) => {
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
            <button className={styles.button} onClick={openCreateChat}>Создать новый чат</button>
            <input className={styles.input} type="text" onChange={(e) => {setSearchQuery(e.target.value); globalSearchChats();}} value={searchQuery} placeholder="Поиск..."/>
            {
                searchQuery ? <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>Результат поиска</div> : <div></div>
            }
            {
                searchQuery ? globalChats?.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => openChatMessages(chat)}></Chat>
                ) : <div></div>
            }
            <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>Мои чаты</div>
            {
                chats.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => openChatMessages(chat)}></Chat>
                )
            }
        </div>
    )
}

export default ChatList;