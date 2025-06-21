import React, { useState } from 'react'
import Chat from './Chat';
import styles from './ChatList.module.css';
import axios from 'axios';
import SearchedUser from './SearchedUser';
import profileImg from '../imgs/profile.png'
import { jwtDecode } from 'jwt-decode';

const ChatList = ({ chats, openCreateChat, openChatMessages, setSelectedUser }) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [globalChats, setGlobalChats] = useState([]);
    const [globalUsers, setGlobalUsers] = useState([]);

    const getUser = async () => {
        const token = localStorage.getItem('token');
        const decoded = jwtDecode(token);
        const response = await axios.get(`http://45.144.222.67:5266/api/users/${decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]}`);
        if (response.status === 200) {
            setSelectedUser(response.data);
        }
    }

    const globalSearchChats = async () => {
        if (searchQuery !== '') {
            const response = await axios.get(`http://45.144.222.67:5266/api/chats?searchQuery=${searchQuery}`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            })
            setGlobalChats(response.data);
        }
    }

    const userSearch = async () => {
        if (searchQuery !== '') {
            const response = await axios.get(`http://45.144.222.67:5266/api/users?searchQuery=${searchQuery}`);
            setGlobalUsers(response.data);
        }
    }

    return (
        <div style={{ width: "30vw", overflowY: "auto", minHeight: "100vh" }}>
            <div style={{ display: 'flex', marginTop: '10px' }}>
                <button onClick={() => {getUser()}} style={{ background: 'none', border: 'none', width: '0%', cursor: 'pointer', margin: '0 10px' }}><img height='25px' src={profileImg} alt="" /></button>
                <button className={styles.button} onClick={openCreateChat}>Создать новый чат</button>
            </div>
            <input className={styles.input} type="text" onChange={(e) => { setSearchQuery(e.target.value); globalSearchChats(); userSearch(); }} value={searchQuery} placeholder="Поиск..." />
            {
                searchQuery ? <div style={{ backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center' }}>Результат поиска</div> : <div></div>
            }
            {
                searchQuery && globalChats.length > 0 ? <div style={{ backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center' }}>Публичные чаты</div> : <div></div>
            }
            {
                searchQuery ? globalChats?.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => openChatMessages(chat)}></Chat>
                ) : <div></div>
            }
            {
                searchQuery && globalUsers.length > 0 ? <div style={{ backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center' }}>Пользователи</div> : <div></div>
            }
            {
                searchQuery ? globalUsers?.map(user =>
                    <SearchedUser key={user.userId} user={user} onClick={() => setSelectedUser(user)}></SearchedUser>
                ) : <div></div>
            }
            <div style={{ backgroundColor: '#ccc', width: '100%', height: '4%', textAlign: 'center' }}>Мои чаты</div>
            {
                chats.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => openChatMessages(chat)}></Chat>
                )
            }
        </div>
    )
}

export default ChatList;