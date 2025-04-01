import React, { useState } from 'react'
import Chat from './Chat';
import styles from './ChatList.module.css';
import axios from 'axios';
import SearchedUser from './SearchedUser';

const ChatList = ({ chats, openCreateChat, openChatMessages, setSelectedUser }) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [globalChats, setGlobalChats] = useState([]);
    const [globalUsers, setGlobalUsers] = useState([]);

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

    const userSearch = async () => {
        if (searchQuery !== '') {
            const response = await axios.get(`https://localhost:7192/api/users?searchQuery=${searchQuery}`);
            setGlobalUsers(response.data);
        }
    }

    return (
        <div style={{ width: "30vw", overflowY: "auto", minHeight: "100vh" }}>
            <button className={styles.button} onClick={openCreateChat}>Создать новый чат</button>
            <input className={styles.input} type="text" onChange={(e) => {setSearchQuery(e.target.value); globalSearchChats(); userSearch();}} value={searchQuery} placeholder="Поиск..."/>
            {
                searchQuery ? <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>Результат поиска</div> : <div></div>
            }
            {
                searchQuery && globalChats.length > 0 ? <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>Публичные чаты</div> : <div></div>
            }
            {
                searchQuery ? globalChats?.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => openChatMessages(chat)}></Chat>
                ) : <div></div>
            }
            {
                searchQuery && globalUsers.length > 0 ? <div style={{backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center'}}>Пользователи</div> : <div></div>
            }
            {
                searchQuery ? globalUsers?.map(user =>
                    <SearchedUser key={user.userId} user={user} onClick={() => setSelectedUser(user)}></SearchedUser>
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